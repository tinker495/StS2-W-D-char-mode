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

    # Human requester: wjdrb, 2026-10-25: add a separate native-scene lifecycle
    # integration check to this local command. Publish current resources first;
    # do not test a stale installed mod pack or install into the game.
    Invoke-RecordedCommand -Executable 'powershell.exe' -Arguments @('-NoProfile', '-NonInteractive',
        '-ExecutionPolicy', 'Bypass', '-File', (Join-Path $projectRoot 'dev.ps1'), 'publish',
        '-Configuration', $Configuration) -LogDirectory $output -Step 'native-package' -Quiet
    $modPack = Join-Path $paths.ModsPath 'Sts2WD/Sts2WD.pck'
    Invoke-RecordedCommand -Executable $dotnet -Arguments @('msbuild', (Join-Path $testProject 'CardPreviewTests.csproj'),
        '-getProperty:ProjectAssetsFile') -LogDirectory $output -Step 'assets-path' -Quiet
    $assetsPath = (Get-Content -LiteralPath (Join-Path $output 'assets-path.log') -Encoding UTF8 -Raw).Trim()
    $assets = Get-Content -LiteralPath $assetsPath -Encoding UTF8 -Raw | ConvertFrom-Json
    $baseLib = @($assets.libraries.PSObject.Properties | Where-Object Name -Like 'Alchyr.Sts2.BaseLib/*')
    if ($baseLib.Count -ne 1) { throw 'Expected exactly one restored BaseLib version.' }
    $basePack = $assets.packageFolders.PSObject.Properties.Name | ForEach-Object {
        Join-Path $_ ($baseLib[0].Value.path + '/Content/BaseLib.pck')
    } | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
    if (-not $basePack) { throw 'Restored BaseLib.pck is missing.' }
    [ordered]@{
        modPack = $modPack
        modSha256 = (Get-FileHash -LiteralPath $modPack -Algorithm SHA256).Hash
        basePack = $basePack
        baseSha256 = (Get-FileHash -LiteralPath $basePack -Algorithm SHA256).Hash
        saveIo = 'MockGodotFileIo (in-memory)'
        configDirectory = (Join-Path $output 'user-data')
    } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'native-environment.json') -Encoding UTF8
    try {
        Invoke-RecordedCommand -Executable $paths.GodotPath -Arguments @('--headless', '--path', $testProject,
            '--log-file', (Join-Path $output 'native-tests.engine.log'), 'res://native-layout.tscn',
            '--', '--game-pack', $gamePack, '--base-pack', $basePack, '--mod-pack', $modPack,
            '--user-data', (Join-Path $output 'user-data')) `
            -LogDirectory $output -Step 'native-tests' -Quiet
    } catch {
        Get-Content -LiteralPath (Join-Path $output 'native-tests.log') -Encoding UTF8 |
            Where-Object { $_ -match '^FAIL|^ERROR:' } | Select-Object -First 12 | Write-Host
        throw
    }
    $nativeLog = Get-Content -LiteralPath (Join-Path $output 'native-tests.log') -Encoding UTF8 -Raw
    if ($nativeLog -match '(?m)^ERROR:\s*(?!Failed to read the root certificate store\.)\S|SCRIPT ERROR:|Parse Error:|Failed (?:to load script|loading resource)' -or
        -not $nativeLog.Contains('NATIVE_CARD_PREVIEW_TESTS_PASSED')) {
        throw "Native card tests did not complete successfully; read $output/native-tests.log."
    }
    $nativeLog -split '\r?\n' | Where-Object { $_ -match '^Native card previews:|^NATIVE_CARD_PREVIEW_TESTS_PASSED' } | Write-Host
} finally {
    Pop-Location
}
