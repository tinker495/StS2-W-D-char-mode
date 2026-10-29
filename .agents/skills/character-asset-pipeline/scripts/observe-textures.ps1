[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('official-cards', 'observe-texture')]
    [string]$Command,
    [string[]]$CardIds,
    [string]$PackPath,
    [string]$ResourcePath,
    [string]$Source,
    [Parameter(Mandatory)]
    [string]$OutPath,
    [string]$GodotPath,
    [string]$GamePath
)

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
. (Join-Path $projectRoot 'scripts/dev-workflow.ps1')

# Inputs select observations, never an expected layout or acceptance threshold.
$resources = @()
if ($Command -eq 'official-cards') {
    if (-not $CardIds -or $PackPath -or $ResourcePath -or $Source) {
        throw 'official-cards requires -CardIds pool/card_id; use observe-texture for a custom pack or comparison PNG.'
    }
    foreach ($cardId in $CardIds) {
        if ($cardId -cnotmatch '^[a-z0-9_]+/[a-z0-9_]+$') { throw "Invalid card ID '$cardId'; use pool/card_id without an extension." }
    }
    if (@($CardIds | Select-Object -Unique).Count -ne $CardIds.Count) { throw 'Specify each card ID once.' }
    foreach ($cardId in $CardIds) {
        $resources += [ordered]@{ resource = "res://images/packed/card_portraits/$cardId.png"; file = ($cardId.Replace('/', '-') + '.png') }
    }
} else {
    if (-not $PackPath -or -not $ResourcePath -or $CardIds -or $GamePath) {
        throw 'observe-texture requires -PackPath and -ResourcePath; -Source is an optional local comparison PNG.'
    }
    if (-not $ResourcePath.StartsWith('res://') -or $ResourcePath.Contains('\') -or ($ResourcePath.Substring(6).Split('/') -contains '..')) {
        throw 'Use an absolute res:// resource path without parent traversal.'
    }
    $resources = @([ordered]@{ resource = $ResourcePath; file = 'texture.png' })
}

$resolvedOut = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutPath)
if (Test-Path -LiteralPath $resolvedOut) { throw 'Use a fresh -OutPath directory; existing observations are preserved.' }
if ($Source) {
    $Source = (Resolve-Path -LiteralPath $Source).ProviderPath
    if ([IO.Path]::GetExtension($Source) -ine '.png') { throw '-Source must be a PNG.' }
}
if (-not $GodotPath -or ($Command -eq 'official-cards' -and -not $GamePath)) {
    $dotnet = Initialize-DevelopmentDotNet $projectRoot
    $developmentPaths = Get-DevelopmentPaths $projectRoot $dotnet
    if (-not $GodotPath) { $GodotPath = $developmentPaths.GodotPath }
    if ($Command -eq 'official-cards' -and -not $GamePath) { $GamePath = $developmentPaths.Sts2Path }
}
if (-not $GodotPath) { throw 'Set GodotPath in local.props or pass -GodotPath.' }
$resolvedGodot = (Resolve-Path -LiteralPath $GodotPath).ProviderPath
$gameVersion = $null
if ($Command -eq 'official-cards') {
    if (-not $GamePath) { throw 'Set Sts2Path in local.props or pass -GamePath.' }
    $PackPath = Join-Path $GamePath 'SlayTheSpire2.pck'
    $gameAssembly = Join-Path $GamePath 'data_sts2_windows_x86_64/sts2.dll'
    if (Test-Path -LiteralPath $gameAssembly -PathType Leaf) {
        $gameVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($gameAssembly).ProductVersion
    }
}
$resolvedPack = (Resolve-Path -LiteralPath $PackPath).ProviderPath
if (-not (Test-Path -LiteralPath $resolvedPack -PathType Leaf)) { throw '-PackPath must identify a pack file.' }
$packInfo = Get-Item -LiteralPath $resolvedPack
$hostDirectory = Join-Path $resolvedOut '_host'
New-Item -ItemType Directory -Path $hostDirectory | Out-Null
$utf8 = [Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllText((Join-Path $resolvedOut '.gdignore'), '', $utf8)
[IO.File]::WriteAllText((Join-Path $hostDirectory 'project.godot'), "config_version=5`n", $utf8)
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'observe-textures.gd') -Destination $hostDirectory
$request = [ordered]@{
    command = $Command
    pack = $resolvedPack
    pack_size_bytes = $packInfo.Length
    pack_modified_utc = $packInfo.LastWriteTimeUtc.ToString('o')
    game_version = $gameVersion
    source = $Source
    output_directory = $resolvedOut
    resources = @($resources)
}
$requestPath = Join-Path $resolvedOut 'request.json'
[IO.File]::WriteAllText($requestPath, ($request | ConvertTo-Json -Depth 5), $utf8)
Invoke-RecordedCommand -Executable $resolvedGodot -Arguments @('--log-file', (Join-Path $resolvedOut 'engine.log'),
    '--headless', '--path', $hostDirectory, '--script', 'observe-textures.gd', '--', $requestPath) `
    -LogDirectory $resolvedOut -Step 'textures' -Summary
$manifestPath = Join-Path $resolvedOut 'manifest.json'
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) { throw "No completed observation manifest; read $resolvedOut/engine.log." }
$manifest = Get-Content -LiteralPath $manifestPath -Encoding UTF8 -Raw | ConvertFrom-Json
if (-not $manifest.complete -or @($manifest.images).Count -ne $resources.Count) { throw 'Texture observation did not complete for every requested resource.' }
Write-Host "Texture observations: $manifestPath"
$manifest
