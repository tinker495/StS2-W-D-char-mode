param(
    [Parameter(Position=0, Mandatory=$true)]
    [ValidateSet('install','install-spine','rigc','rig-create','rig-parts','rig-pose','rig-review','bun')]
    [string]$Tool,
    [Parameter(ValueFromRemainingArguments=$true)]
    [string[]]$ToolArgs
)
$ErrorActionPreference = 'Stop'
if ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT -and
    $Tool -eq 'rig-review' -and $ToolArgs.Count -gt 0 -and $ToolArgs[0] -ceq 'open' -and
    $ToolArgs -cnotcontains '--direct' -and $ToolArgs -cnotcontains '--help' -and $ToolArgs -cnotcontains '-h') {
    [Console]::Error.WriteLine(@'
Managed rig-review open requires POSIX workspace permissions and is unavailable on Windows.
Foreground single-target (recommended):
  .\.tools\rig-c\setup\run.ps1 rig-review <dir> --view-only [--animation <name>]
Direct background worker:
  .\.tools\rig-c\setup\run.ps1 rig-review open <dir> --view-only --direct [--animation <name>]
'@)
    exit 1
}

$installDir = Split-Path -Parent $PSScriptRoot
if ($Tool -in @('install', 'install-spine')) {
    $profileArgument = if ($Tool -eq 'install-spine') { '--include=dev' } else { '--omit=dev' }
    $manifests = @((Join-Path $PSScriptRoot 'package.json'), (Join-Path $PSScriptRoot 'package-lock.json'))
    Copy-Item -LiteralPath $manifests -Destination $installDir
    Push-Location -LiteralPath $installDir
    try {
        & npm.cmd ci $profileArgument @ToolArgs
        $result = $LASTEXITCODE
    } finally {
        Pop-Location
    }
    exit $result
}

$modules = Join-Path $installDir 'node_modules'
$bunDir = Join-Path $modules 'bun/bin'
$bunExe = Join-Path $bunDir 'bun.exe'
$priorPath = $env:PATH
try {
    $env:PATH = $bunDir + ';' + $env:PATH
    if ($Tool -eq 'bun') {
        & $bunExe @ToolArgs
    } else {
        $packages = @{'rigc'='core';'rig-create'='create';'rig-parts'='parts';'rig-pose'='pose';'rig-review'='review'}
        $packageDir = Join-Path $modules ('@rig-c/' + $packages[$Tool])
        $manifest = Get-Content -LiteralPath (Join-Path $packageDir 'package.json') -Raw | ConvertFrom-Json
        $entry = Join-Path $packageDir $manifest.bin.$Tool
        & $bunExe $entry @ToolArgs
    }
    $result = $LASTEXITCODE
} finally {
    $env:PATH = $priorPath
}
exit $result
