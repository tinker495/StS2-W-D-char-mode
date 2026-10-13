[CmdletBinding()]
param(
    [ValidateSet('doctor', 'restore', 'build', 'publish', 'deploy', 'editor')]
    [string]$Task = 'build',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
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
    Invoke-DotNet @('build', 'Sts2WD.csproj', '-c', $Configuration, '-p:RestoreLockedMode=true')
    & $paths.GodotPath --headless --editor --path $projectRoot --import
    if ($LASTEXITCODE -ne 0) { throw 'MegaDot asset import failed.' }
    Invoke-DotNet @('publish', 'Sts2WD.csproj', '-c', $Configuration, '--no-restore')

    $output = Join-Path $paths.ModsPath 'Sts2WD'
    foreach ($name in @('Sts2WD.dll', 'Sts2WD.pck', 'Sts2WD.json')) {
        if (-not (Test-Path -LiteralPath (Join-Path $output $name))) { throw "Missing publish artifact: $name" }
    }
    if ($Task -eq 'deploy') {
        $destination = Join-Path $paths.Sts2Path 'mods/Sts2WD'
        New-Item -ItemType Directory -Path $destination -Force | Out-Null
        Get-ChildItem -LiteralPath $output -File | Where-Object { $_.Extension -in '.dll', '.pck', '.json', '.pdb' } |
            Copy-Item -Destination $destination -Force
        Write-Host "Installed: $destination"
    } else {
        Write-Host "Published: $output"
    }
} finally {
    Pop-Location
}
