param(
    [Parameter(Mandatory)][string]$Output,
    [string]$SeedParts
)
$ErrorActionPreference = 'Stop'
# Human requester: wjdrb, 2026-11-22, retrospective item 2: fail fast on stray
# part PNGs under the tracked rigc directory and require explicit -SeedParts overrides.
$strayParts = Join-Path $PSScriptRoot 'parts'
if (Test-Path -LiteralPath $strayParts -PathType Container) {
    $strayPngs = @(Get-ChildItem -LiteralPath $strayParts -Filter '*.png' -File -Recurse)
    if ($strayPngs.Count -gt 0) {
        throw "Stray part PNGs found under tracked directory '$strayParts' ($($strayPngs.Count) file(s)). Remove that folder and pass -SeedParts <ignored-dir> instead so loose parts never linger under tracked docs/."
    }
}
$resolvedSeedParts = $null
if (-not [string]::IsNullOrWhiteSpace($SeedParts)) {
    if (-not (Test-Path -LiteralPath $SeedParts -PathType Container)) {
        throw "SeedParts directory not found: $SeedParts"
    }
    $resolvedSeedParts = (Resolve-Path -LiteralPath $SeedParts).ProviderPath
}
$author = Join-Path $PSScriptRoot 'authoring'
$layout = Join-Path $PSScriptRoot 'layout.json'
$atlasMap = Join-Path $PSScriptRoot 'atlas.txt'
$selectAtlasMap = Join-Path $PSScriptRoot 'select_atlas.txt'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..\..')).Path
$atlasPng = Join-Path $repoRoot 'Sts2WD/animations/realta/combat/realta_skeleton.png'
$selectAtlasPng = Join-Path $repoRoot 'Sts2WD/animations/realta/select/realta_select_atlas.png'
$selectMasterPng = Join-Path $repoRoot 'Sts2WD/images/charui/realta/char_select_bg.png'
$tool = Join-Path (Get-Location) '.tools/rig-c/setup/run.ps1'

$images = Join-Path $Output 'parts'
New-Item -ItemType Directory -Force -Path $images | Out-Null
& python -X utf8 -B (Join-Path $author 'unpack_atlas.py') --atlas $atlasMap --png $atlasPng --out $images
if ($LASTEXITCODE) { throw "Failed to unpack part PNGs from $atlasPng" }
if ($resolvedSeedParts) {
    $seedPngs = @(Get-ChildItem -LiteralPath $resolvedSeedParts -Filter '*.png' -File)
    foreach ($seedPng in $seedPngs) {
        Copy-Item -LiteralPath $seedPng.FullName -Destination $images -Force
    }
}

& python -X utf8 -B (Join-Path $author 'author_motion.py') $layout --images $images --out $Output
if ($LASTEXITCODE) { throw "Authoring failed in $Output" }
& $tool rigc build --rig (Join-Path $Output 'realta.rig.json') --motion (Join-Path $Output 'realta.motion.json') --images $images --out (Join-Path $Output 'build') --pack --profile rig-play
if ($LASTEXITCODE) { throw "Rig compilation failed in $Output" }

# Compile Realta's character-select Spine overlay rig alongside the combat/merchant rig.
$selectOut = Join-Path $Output 'select'
$selectImages = Join-Path $selectOut 'parts'
New-Item -ItemType Directory -Force -Path $selectImages | Out-Null
& python -X utf8 -B (Join-Path $author 'unpack_atlas.py') --atlas $selectAtlasMap --png $selectAtlasPng --out $selectImages
if ($LASTEXITCODE) { throw "Failed to unpack select part PNGs from $selectAtlasPng" }
& python -X utf8 -B (Join-Path $author 'author_select.py') --images $selectImages --out $selectOut
if ($LASTEXITCODE) { throw "Select rig authoring failed in $selectOut" }
& $tool rigc build --rig (Join-Path $selectOut 'realta_select.rig.json') --motion (Join-Path $selectOut 'realta_select.motion.json') --images $selectImages --out (Join-Path $selectOut 'build') --pack --profile rig-play
if ($LASTEXITCODE) { throw "Select rig compilation failed in $selectOut" }
