[CmdletBinding()]
param(
    [ValidateSet('hooks', 'doctor', 'restore', 'build', 'publish', 'deploy', 'editor')]
    [string]$Task = 'build',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [ValidateSet('unspecified', 'sandbox', 'escalated', 'local')]
    [string]$ExecutionContextLabel = 'unspecified'
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
. (Join-Path $projectRoot 'scripts/dev-workflow.ps1')
$diagnosticRoot = $null
if ($Task -eq 'hooks') {
    & git -C $projectRoot config --local core.hooksPath .githooks
    if ($LASTEXITCODE -ne 0) { throw 'Cannot configure Git hooks.' }
    Write-Host 'Git pre-commit build check enabled for this checkout.'
    return
}

$toolsRoot = Join-Path $projectRoot '.tools'
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

function Invoke-DotNet([string[]]$Arguments) {
    if ($diagnosticRoot) {
        Invoke-RecordedCommand -Executable $dotnet -Arguments $Arguments -LogDirectory $diagnosticRoot -Step $Arguments[0]
        return
    }
    & $dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet failed (exit $LASTEXITCODE): $Arguments" }
}

Push-Location -LiteralPath $projectRoot
try {
    if ($Task -eq 'restore') {
        Invoke-DotNet @('restore', 'Sts2WD.csproj', '--locked-mode')
        return
    }
    if ($Task -eq 'build') {
        Invoke-DotNet @('build', 'Sts2WD.csproj', '-c', $Configuration, '-p:RestoreLockedMode=true')
        return
    }

    $propertyJson = & $dotnet msbuild Sts2WD.csproj '-getProperty:Sts2Path,Sts2DataDir,GodotPath,ModsPath'
    if ($LASTEXITCODE -ne 0) { throw 'Cannot resolve project paths. Run the restore task first.' }
    $paths = ($propertyJson -join "`n" | ConvertFrom-Json).Properties

    if ($Task -eq 'doctor') {
        Invoke-DotNet @('--version')
        $paths | Format-List
        foreach ($path in @((Join-Path $paths.Sts2DataDir 'sts2.dll'), $paths.GodotPath)) {
            if (-not $path -or -not (Test-Path -LiteralPath $path)) { throw "Missing dependency: $path" }
        }
        & $paths.GodotPath --headless --version
        if ($LASTEXITCODE -ne 0) { throw 'MegaDot check failed.' }
        Write-Host 'Development dependencies are ready. BaseLib 3.4.7+ must also be enabled in-game.'
        return
    }

    if (-not (Test-Path -LiteralPath $paths.GodotPath)) { throw 'Set GodotPath in local.props.' }
    if ($Task -eq 'editor') {
        Start-Process -FilePath $paths.GodotPath -ArgumentList '--editor', '--path', ('"' + $projectRoot + '"') -WorkingDirectory $projectRoot
        return
    }

    # Import textures before exporting a PCK on a fresh checkout.
    New-Item -ItemType Directory -Path (Join-Path $projectRoot 'artifacts') -Force | Out-Null
    New-Item -ItemType File -Path (Join-Path $projectRoot 'artifacts/.gdignore') -Force | Out-Null
    $diagnosticRoot = Join-Path $projectRoot ('artifacts/runtime/packaging-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ') + '-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $diagnosticRoot | Out-Null
    Write-Host "Diagnostics: $diagnosticRoot"
    $run = [ordered]@{
        task = $Task
        configuration = $Configuration
        executionContextLabel = $ExecutionContextLabel # Caller-reported, not permission detection.
        projectRoot = $projectRoot
        branch = (& git branch --show-current)
        commit = (& git rev-parse HEAD)
        changes = @(& git status --short)
        paths = $paths
        startedUtc = [DateTime]::UtcNow.ToString('o')
        finishedUtc = $null
        exitCode = $null
        failure = $null
        installation = $null
    }
    try {
        Invoke-RecordedCommand -Executable $dotnet -Arguments @('--version') -LogDirectory $diagnosticRoot -Step 'dotnet-version'
        Invoke-RecordedCommand -Executable $paths.GodotPath -Arguments @('--headless', '--version') -LogDirectory $diagnosticRoot -Step 'megadot-version'
        Invoke-DotNet @('build', 'Sts2WD.csproj', '-c', $Configuration, '-p:RestoreLockedMode=true')
        Invoke-RecordedCommand -Executable $paths.GodotPath -Arguments @('--headless', '--editor', '--path', $projectRoot, '--import') -LogDirectory $diagnosticRoot -Step 'import'
        Invoke-DotNet @('publish', 'Sts2WD.csproj', '-c', $Configuration, '--no-restore')

        $output = Join-Path $paths.ModsPath 'Sts2WD'
        foreach ($name in @('Sts2WD.dll', 'Sts2WD.pck', 'Sts2WD.json', 'Sts2WD.pdb')) {
            if (-not (Test-Path -LiteralPath (Join-Path $output $name) -PathType Leaf)) { throw "Missing publish artifact: $name" }
        }
        if ($Task -eq 'deploy') {
            $run.installation = Install-ModPackage -Source $output -Destination (Join-Path $paths.Sts2Path 'mods/Sts2WD') -BackupRoot (Join-Path $projectRoot 'artifacts/runtime')
        } else {
            Write-Host "Published: $output"
        }
        $run.exitCode = 0
    } catch {
        $run.exitCode = 1
        $run.failure = $_.Exception.Message
        throw
    } finally {
        $run.finishedUtc = [DateTime]::UtcNow.ToString('o')
        $run | ConvertTo-Json -Depth 5 |
            Set-Content -LiteralPath (Join-Path $diagnosticRoot 'run.json') -Encoding UTF8
    }
} finally {
    Pop-Location
}
