# Shared development paths, packaging diagnostics and local installation.

function Initialize-DevelopmentDotNet([string]$ProjectRoot) {
    $toolsRoot = Join-Path $ProjectRoot '.tools'
    $dotnet = Join-Path $toolsRoot 'dotnet/dotnet.exe'
    if (Test-Path -LiteralPath $dotnet) {
        $env:DOTNET_ROOT = Split-Path $dotnet
        $env:DOTNET_CLI_HOME = Join-Path $toolsRoot 'dotnet-home'
        $env:NUGET_PACKAGES = Join-Path $toolsRoot 'nuget-packages'
        $env:PATH = "$env:DOTNET_ROOT;$env:PATH"
    } else {
        $dotnet = (Get-Command dotnet -ErrorAction Stop).Source
    }
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    $env:DOTNET_NOLOGO = '1'
    return $dotnet
}

function Get-DevelopmentPaths([string]$ProjectRoot, [string]$DotNet) {
    Push-Location -LiteralPath $ProjectRoot
    try {
        $propertyJson = & $DotNet msbuild Sts2WD.csproj '-getProperty:Sts2Path,Sts2DataDir,GodotPath,ModsPath'
        if ($LASTEXITCODE -ne 0) { throw 'Cannot resolve project paths. Run the restore task first.' }
        return ($propertyJson -join "`n" | ConvertFrom-Json).Properties
    } finally {
        Pop-Location
    }
}

function Get-DevelopmentIlSpy([string]$ProjectRoot) {
    $localTool = Join-Path $ProjectRoot '.tools/ilspy/ilspycmd.exe'
    if (Test-Path -LiteralPath $localTool -PathType Leaf) { return $localTool }
    $command = Get-Command ilspycmd -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    throw 'Install ilspycmd on PATH or supply -IlSpyPath. See docs/development-tools.md.'
}

function Invoke-RecordedCommand {
    param(
        [string]$Executable,
        [string[]]$Arguments,
        [string]$LogDirectory,
        [string]$Step,
        [switch]$Quiet
    )

    $record = [ordered]@{
        executable = $Executable
        arguments = $Arguments
        workingDirectory = (Get-Location).Path
        startedUtc = [DateTime]::UtcNow.ToString('o')
        finishedUtc = $null
        exitCode = $null
        failure = $null
    }
    $logPath = Join-Path $LogDirectory "$Step.log"
    $writer = [IO.StreamWriter]::new($logPath, $false, [Text.UTF8Encoding]::new($false))
    $previousEncoding = [Console]::OutputEncoding
    $previousPreference = $ErrorActionPreference
    try {
        $null = Get-Command $Executable -ErrorAction Stop
        [Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
        # Windows PowerShell wraps native stderr in ErrorRecord. Record it and
        # judge native failure by the exit code, rather than losing the tail.
        $ErrorActionPreference = 'Continue'
        & $Executable @Arguments 2>&1 | ForEach-Object {
            $line = $_.ToString()
            $writer.WriteLine($line)
            if (-not $Quiet) { Write-Host $line }
        }
        $record.exitCode = $LASTEXITCODE
        if ($null -eq $record.exitCode -or $record.exitCode -ne 0) {
            throw "$Step failed (exit $($record.exitCode)); log: $logPath"
        }
    } catch {
        $record.failure = $_.Exception.Message
        throw
    } finally {
        $ErrorActionPreference = $previousPreference
        [Console]::OutputEncoding = $previousEncoding
        $writer.Dispose()
        $record.finishedUtc = [DateTime]::UtcNow.ToString('o')
        $record | ConvertTo-Json -Depth 4 |
            Set-Content -LiteralPath (Join-Path $LogDirectory "$Step.json") -Encoding UTF8
    }
}

function Install-ModPackage {
    param(
        [string]$Source,
        [string]$Destination,
        [string]$BackupRoot
    )

    $ErrorActionPreference = 'Stop'
    $files = @('Sts2WD.dll', 'Sts2WD.pck', 'Sts2WD.json', 'Sts2WD.pdb')
    $hashes = @{}
    foreach ($name in $files) {
        $path = Join-Path $Source $name
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing publish artifact: $path" }
        $hashes[$name] = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
    }
    if (Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue) {
        throw 'Close SlayTheSpire2 before installing. No installed files were replaced.'
    }

    $backup = Join-Path $BackupRoot ('local-install-backup-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ') + '-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $backup | Out-Null
    Write-Host "Backup: $backup"
    try {
        foreach ($name in $files) {
            $installed = Join-Path $Destination $name
            if (Test-Path -LiteralPath $installed) {
                $saved = Join-Path $backup $name
                Copy-Item -LiteralPath $installed -Destination $saved
                if ((Get-FileHash -LiteralPath $installed -Algorithm SHA256).Hash -ne
                    (Get-FileHash -LiteralPath $saved -Algorithm SHA256).Hash) {
                    throw "Backup hash mismatch: $name"
                }
            }
        }
        if (Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue) {
            throw 'SlayTheSpire2 started during backup. No installed files were replaced.'
        }
        New-Item -ItemType Directory -Path $Destination -Force | Out-Null
        foreach ($name in $files) {
            Copy-Item -LiteralPath (Join-Path $Source $name) -Destination (Join-Path $Destination $name) -Force
        }
        foreach ($name in $files) {
            if ((Get-FileHash -LiteralPath (Join-Path $Destination $name) -Algorithm SHA256).Hash -ne $hashes[$name]) {
                throw "Installed hash mismatch: $name"
            }
            Write-Host "SHA256 MATCH: $name"
        }
    } catch {
        throw "Installation failed. Backup: $backup. $($_.Exception.Message)"
    }
    Write-Host "Installed: $Destination"
    return [ordered]@{ destination = $Destination; backup = $backup; sha256 = $hashes }
}
