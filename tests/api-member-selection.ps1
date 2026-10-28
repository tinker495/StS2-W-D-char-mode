# Human requester: wjdrb, 2026-10-25, explicitly requested retrospective item 1:
# fix generic member lookup and add an automatic check that both FromCard<T>()
# and FromCard(CardModel, bool) declarations are selected. The native factory
# contains both; the old declaration regex silently omitted the generic one.
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$output = @(& (Join-Path $repositoryRoot 'scripts/inspect-game-api.ps1') `
    -TypeName 'MegaCrit.Sts2.Core.HoverTips.HoverTipFactory' -Member 'FromCard' `
    -ContextLines 0 -MaxLines 20 6>&1 | ForEach-Object { $_.ToString() })
$declarations = @($output | Where-Object { $_ -match '^\s*\d+:\s+public static IHoverTip FromCard' })
foreach ($required in @('FromCard<T>(bool upgrade = false)', 'FromCard(CardModel card, bool upgrade = false)')) {
    if (-not @($declarations | Where-Object { $_.Contains($required) }).Count) {
        $output | Write-Host
        throw "API member selection omitted the native declaration: $required"
    }
}
$declarations | Write-Host
Write-Host 'PASS: API member selection (generic and non-generic FromCard)'
