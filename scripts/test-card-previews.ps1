# Human requester: wjdrb, 2026-10-25. Run the newly requested formal card-preview
# contracts with the installed game DLL and a headless MegaDot viewport.
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'dev-workflow.ps1')
$dotnet = Initialize-DevelopmentDotNet $projectRoot
$paths = Get-DevelopmentPaths $projectRoot $dotnet
$gamePack = Join-Path $paths.Sts2Path 'SlayTheSpire2.pck'
foreach ($dependency in @($paths.GodotPath, (Join-Path $paths.Sts2DataDir 'sts2.dll'), $gamePack)) {
    if (-not $dependency -or -not (Test-Path -LiteralPath $dependency -PathType Leaf)) {
        throw "Missing card-test dependency: $dependency. Run dev.ps1 doctor."
    }
}
$output = Join-Path $projectRoot ('artifacts/card-tests-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $output | Out-Null
New-Item -ItemType File -Path (Join-Path $output '.gdignore') | Out-Null
$testProject = Join-Path $projectRoot 'tests/card-previews'
Write-Host "Card-test diagnostics: $output"
$gameAssembly = Get-Item -LiteralPath (Join-Path $paths.Sts2DataDir 'sts2.dll')
[ordered]@{
    configuration = $Configuration
    gameAssembly = $gameAssembly.FullName
    gameVersion = $gameAssembly.VersionInfo.ProductVersion
    gameSha256 = (Get-FileHash -LiteralPath $gameAssembly.FullName -Algorithm SHA256).Hash
    gamePack = $gamePack
    godot = $paths.GodotPath
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'environment.json') -Encoding UTF8

Push-Location -LiteralPath $projectRoot
try {
    Invoke-RecordedCommand -Executable $dotnet -Arguments @('--version') -LogDirectory $output -Step 'dotnet-version' -Quiet
    Invoke-RecordedCommand -Executable $paths.GodotPath -Arguments @('--headless', '--version') `
        -LogDirectory $output -Step 'megadot-version' -Quiet
    # The standalone Godot host loads Debug. Its project reference independently
    # builds and copies the requested product configuration into that host.
    Invoke-RecordedCommand -Executable $dotnet -Arguments @('build', (Join-Path $testProject 'CardPreviewTests.csproj'),
        '-c', 'Debug', "-p:ModConfiguration=$Configuration", '-p:RestoreLockedMode=true') -LogDirectory $output -Step 'build'
    try {
        Invoke-RecordedCommand -Executable $paths.GodotPath -Arguments @('--headless', '--path', $testProject,
            '--log-file', (Join-Path $output 'tests.engine.log'), '--', '--game-pack', $gamePack) `
            -LogDirectory $output -Step 'tests' -Quiet
    } catch {
        Get-Content -LiteralPath (Join-Path $output 'tests.log') -Encoding UTF8 |
            Where-Object { $_ -match '^FAIL|^ERROR:' } | Select-Object -First 12 | Write-Host
        throw
    }
    $log = Get-Content -LiteralPath (Join-Path $output 'tests.log') -Encoding UTF8 -Raw
    if ($log -match '(?m)^ERROR:\s*(?!Failed to read the root certificate store\.)\S|SCRIPT ERROR:|Parse Error:|Failed (?:to load script|loading resource)' -or
        -not $log.Contains('CARD_PREVIEW_TESTS_PASSED')) {
        throw "Card tests did not complete successfully; read $output/tests.log."
    }
    $log -split '\r?\n' | Where-Object { $_ -match '^Card previews:|^CARD_PREVIEW_TESTS_PASSED' } | Write-Host
} finally {
    Pop-Location
}
