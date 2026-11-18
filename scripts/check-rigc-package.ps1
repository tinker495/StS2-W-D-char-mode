[CmdletBinding()]
param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$PackageDir,
    [string]$Seam,
    [string[]]$MaterialLeak = @()
)

$ErrorActionPreference = 'Stop'
$package = (Resolve-Path -LiteralPath $PackageDir).Path
$repoRoot = Split-Path -Parent $PSScriptRoot
$wrapper = Join-Path $repoRoot '.tools/rig-c/setup/run.ps1'
$build = Join-Path $package 'build'
$manifest = Join-Path $package 'review.json'
$checkScript = Join-Path $PSScriptRoot 'rigc_package_checks.py'

function Invoke-RigTool([string[]]$ToolArguments) {
    & $wrapper @ToolArguments
    if ($LASTEXITCODE -ne 0) {
        throw "rig-c command failed (exit ${LASTEXITCODE}): $($ToolArguments -join ' ')"
    }
}

Invoke-RigTool @('rig-review', 'check', $manifest)

$partArguments = @($checkScript, $package, '--parts-only')
foreach ($rule in $MaterialLeak) { $partArguments += @('--material-leak', $rule) }
& python @partArguments
if ($LASTEXITCODE -ne 0) { throw "Loose PNG preflight failed (exit ${LASTEXITCODE})." }

$skeleton = Get-Content -LiteralPath (Join-Path $build 'skeleton.json') -Encoding UTF8 -Raw | ConvertFrom-Json
$animations = @($skeleton.animations.PSObject.Properties.Name | Where-Object { $_ })
if ($animations.Count -eq 0) { throw 'The compiled skeleton has no animations to verify.' }
$hasPhysics = @($skeleton.constraints | Where-Object { $_.type -eq 'physics' }).Count -gt 0

$artifactRoot = Join-Path $repoRoot 'artifacts'
$runName = 'rigc-check-{0}-{1}' -f (Get-Date -Format 'yyyyMMdd-HHmmss'), ([Guid]::NewGuid().ToString('N').Substring(0, 8))
$output = Join-Path $artifactRoot $runName
New-Item -ItemType Directory -Path $output -Force | Out-Null
Write-Host "Rig diagnostics: $output"

for ($index = 0; $index -lt $animations.Count; $index++) {
    $name = $animations[$index]
    $renderDir = Join-Path $output ('{0:D2}' -f $index)
    $arguments = @('rigc', 'render', '--candidate', $build, '--animation', $name,
        '--fps', '12', '--max', '1024', '--folds', '--out', $renderDir)
    if ($hasPhysics -and $name.EndsWith('_loop', [StringComparison]::Ordinal)) {
        $arguments += @('--preroll', '1')
    }
    if ($Seam) { $arguments += @('--seam', $Seam) }
    Invoke-RigTool $arguments
}

$checkArguments = @($checkScript, $package, $output, '--renders-only')
if ($Seam) { $checkArguments += '--seams' }
& python @checkArguments
if ($LASTEXITCODE -ne 0) { throw "Rig package diagnostics failed (exit ${LASTEXITCODE}). See $output." }
