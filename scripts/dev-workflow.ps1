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

function Get-SpineRuntimeExecutable([string]$Sts2Path) {
    if (-not $Sts2Path) { throw 'Sts2Path is required to resolve SlayTheSpire2.exe for SpineSprite runtime execution.' }
    $exe = Join-Path $Sts2Path 'SlayTheSpire2.exe'
    if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
        throw "Missing SlayTheSpire2.exe for SpineSprite runtime execution: $exe"
    }
    return $exe
}

function Invoke-WithLocalSpineExtension {
    param(
        [Parameter(Mandatory)][string]$TargetProjectDir,
        [Parameter(Mandatory)][string]$Sts2Path,
        [Parameter(Mandatory)][scriptblock]$Action
    )
    $dll = (Join-Path $Sts2Path 'libspine_godot.windows.template_release.x86_64.dll') -replace '\\', '/'
    if (-not (Test-Path -LiteralPath $dll -PathType Leaf)) {
        throw "Missing libspine_godot runtime DLL: $dll"
    }
    $extPath = Join-Path $TargetProjectDir 'local_spine.gdextension'
    $created = -not (Test-Path -LiteralPath $extPath -PathType Leaf)
    $content = "[configuration]`nentry_symbol = `"spine_godot_library_init`"`ncompatibility_minimum = `"4.1`"`n`n[libraries]`nwindows.debug.x86_64 = `"$dll`"`nwindows.release.x86_64 = `"$dll`"`n"
    [System.IO.File]::WriteAllText($extPath, $content, [System.Text.UTF8Encoding]::new($false))
    try {
        & $Action
    } finally {
        if ($created -and (Test-Path -LiteralPath $extPath -PathType Leaf)) {
            Remove-Item -LiteralPath $extPath -Force
        }
    }
}

function Write-RecordedLogExcerpt {
    param([string]$LogPath, [switch]$IncludeTail)

    # Console excerpts are navigation aids, never command-success criteria.
    # Keep the complete UTF-8 log; bound both line count and displayed line length.
    $diagnostics = [Collections.Generic.List[string]]::new()
    $tail = [Collections.Generic.Queue[string]]::new()
    foreach ($line in [IO.File]::ReadLines($LogPath, [Text.Encoding]::UTF8)) {
        $plain = $line -replace '\x1B\[[0-?]*[ -/]*[@-~]', ''
        if ($diagnostics.Count -lt 5 -and -not $diagnostics.Contains($plain) -and
            $plain -match '(?i)\b(?:error|warning)(?=\s*:|\s+[A-Z]+\d+\s*:)|\b(?:access (?:is )?denied|permission denied|failed to read the root certificate store)\b') {
            $diagnostics.Add($plain)
        }
        if ($IncludeTail) {
            $tail.Enqueue($plain)
            if ($tail.Count -gt 8) { [void]$tail.Dequeue() }
        }
    }
    if ($diagnostics.Count -gt 0) {
        Write-Host "Diagnostics (up to 5 lines); full log: $LogPath"
        foreach ($line in $diagnostics) {
            Write-Host $(if ($line.Length -gt 400) { $line.Substring(0, 396) + ' ...' } else { $line })
        }
    }
    if ($IncludeTail -and $tail.Count -gt 0) {
        Write-Host "Last 8 log lines; full log: $LogPath"
        foreach ($line in $tail) {
            Write-Host $(if ($line.Length -gt 400) { $line.Substring(0, 396) + ' ...' } else { $line })
        }
    }
}

function Invoke-RecordedCommand {
    param(
        [string]$Executable,
        [string[]]$Arguments,
        [string]$LogDirectory,
        [string]$Step,
        [switch]$Quiet,
        [switch]$Summary
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
            if (-not $Quiet -and -not $Summary) { Write-Host $line }
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
        if ($Summary) {
            $outcome = if ($record.failure) { 'failed' } else { 'completed' }
            Write-Host "$Step $outcome (exit $($record.exitCode)); log: $logPath"
            Write-RecordedLogExcerpt -LogPath $logPath -IncludeTail:([bool]$record.failure)
        }
    }
}

function Install-ModPackage {
    param(
        [string]$Source,
        [string]$Destination,
        [string]$BackupRoot,
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration = 'Release'
    )

    $ErrorActionPreference = 'Stop'
    $installFiles = if ($Configuration -eq 'Release') {
        @('Sts2WD.dll', 'Sts2WD.pck', 'Sts2WD.json')
    } else {
        @('Sts2WD.dll', 'Sts2WD.pck', 'Sts2WD.json', 'Sts2WD.pdb')
    }
    $backupCandidates = @('Sts2WD.dll', 'Sts2WD.pck', 'Sts2WD.json', 'Sts2WD.pdb')
    $hashes = @{}
    foreach ($name in $installFiles) {
        $path = Join-Path $Source $name
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing publish artifact: $path" }
        $hashes[$name] = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
    }
    if ($Configuration -eq 'Release') {
        $unexpectedSourcePdb = Join-Path $Source 'Sts2WD.pdb'
        if (Test-Path -LiteralPath $unexpectedSourcePdb -PathType Leaf) {
            throw "Unexpected debug symbols in Release source directory: $unexpectedSourcePdb"
        }
    }
    if (Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue) {
        throw 'Close SlayTheSpire2 before installing. No installed files were replaced.'
    }

    $backup = Join-Path $BackupRoot ('local-install-backup-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ') + '-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $backup | Out-Null
    Write-Host "Backup: $backup"
    $removedFiles = @()
    try {
        foreach ($name in $backupCandidates) {
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
        if ($Configuration -eq 'Release') {
            $destPdb = Join-Path $Destination 'Sts2WD.pdb'
            if (Test-Path -LiteralPath $destPdb -PathType Leaf) {
                Remove-Item -LiteralPath $destPdb -Force
                if (Test-Path -LiteralPath $destPdb -PathType Leaf) {
                    throw "Failed to remove stale debug symbols from destination: $destPdb"
                }
                $removedFiles += 'Sts2WD.pdb'
                Write-Host 'REMOVED STALE: Sts2WD.pdb'
            }
        }
        foreach ($name in $installFiles) {
            Copy-Item -LiteralPath (Join-Path $Source $name) -Destination (Join-Path $Destination $name) -Force
        }
        foreach ($name in $installFiles) {
            if ((Get-FileHash -LiteralPath (Join-Path $Destination $name) -Algorithm SHA256).Hash -ne $hashes[$name]) {
                throw "Installed hash mismatch: $name"
            }
            Write-Host "SHA256 MATCH: $name"
        }
    } catch {
        throw "Installation failed. Backup: $backup. $($_.Exception.Message)"
    }
    Write-Host "Installed: $Destination"
    return [ordered]@{
        configuration = $Configuration
        destination = $Destination
        backup = $backup
        sha256 = $hashes
        removedFiles = $removedFiles
    }
}
