[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [Alias('Type')]
    [ValidateNotNullOrEmpty()]
    [string]$TypeName,
    [string]$Member,
    [string]$AssemblyPath,
    [string]$IlSpyPath,
    [string]$OutputDirectory,
    [ValidateRange(1, 2147483647)]
    [int]$StartLine = 1,
    [ValidateRange(1, 200)]
    [int]$MaxLines = 80,
    [ValidateRange(0, 20)]
    [int]$ContextLines = 8
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'dev-workflow.ps1')
$dotnet = Initialize-DevelopmentDotNet $projectRoot
if (-not $AssemblyPath) {
    $paths = Get-DevelopmentPaths $projectRoot $dotnet
    $AssemblyPath = Join-Path $paths.Sts2DataDir 'sts2.dll'
}
$AssemblyPath = (Resolve-Path -LiteralPath $AssemblyPath).ProviderPath
if (-not $IlSpyPath) {
    $IlSpyPath = Get-DevelopmentIlSpy $projectRoot
}
$IlSpyPath = (Get-Command $IlSpyPath -ErrorAction Stop).Source
if (-not $OutputDirectory) {
    $OutputDirectory = Join-Path $projectRoot ('artifacts/api-' + [Guid]::NewGuid().ToString('N'))
}
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $OutputDirectory) { throw 'Use a fresh -OutputDirectory so earlier evidence is preserved.' }
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
New-Item -ItemType File -Path (Join-Path $OutputDirectory '.gdignore') | Out-Null
$sourceDirectory = Join-Path $OutputDirectory 'source'
New-Item -ItemType Directory -Path $sourceDirectory | Out-Null
$assembly = Get-Item -LiteralPath $AssemblyPath
[ordered]@{
    assembly = $AssemblyPath
    productVersion = $assembly.VersionInfo.ProductVersion
    sha256 = (Get-FileHash -LiteralPath $AssemblyPath -Algorithm SHA256).Hash
    type = $TypeName
    member = $Member
    startLine = $StartLine
    maxLines = $MaxLines
    contextLines = $ContextLines
    dotnetRollForward = 'Major'
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory 'request.json') -Encoding UTF8

Write-Host "API diagnostics: $OutputDirectory"
$previousRollForward = $env:DOTNET_ROLL_FORWARD
Push-Location -LiteralPath $projectRoot
try {
    # The local ILSpy 9 tool targets .NET 8; this checkout supplies .NET 9.
    # Scope compatibility roll-forward to these child processes and restore it.
    $env:DOTNET_ROLL_FORWARD = 'Major'
    Invoke-RecordedCommand -Executable $dotnet -Arguments @('--version') `
        -LogDirectory $OutputDirectory -Step 'dotnet-version' -Quiet
    Invoke-RecordedCommand -Executable $IlSpyPath -Arguments @('--version') `
        -LogDirectory $OutputDirectory -Step 'ilspy-version' -Quiet
    Invoke-RecordedCommand -Executable $IlSpyPath -Arguments @('--disable-updatecheck', '-t', $TypeName,
        '-o', $sourceDirectory, $AssemblyPath) -LogDirectory $OutputDirectory -Step 'decompile' -Quiet
} finally {
    $env:DOTNET_ROLL_FORWARD = $previousRollForward
    Pop-Location
}

$sources = @(Get-ChildItem -LiteralPath $sourceDirectory -Filter '*.cs' -File)
if ($sources.Count -ne 1) { throw "Expected one decompiled type; found $($sources.Count). Read $OutputDirectory." }
$source = $sources[0].FullName
$lines = @(Get-Content -LiteralPath $source -Encoding UTF8)
$selected = [Collections.Generic.SortedSet[int]]::new()
if ($Member) {
    $escaped = [regex]::Escape($Member)
    # Prefer declarations over generated Godot MethodName fields and call sites.
    # This is bounded textual navigation, not a C# parser; the full type is saved.
    $declaration = '^\s*(?:public|private|protected|internal)\s+.*?(?<![\w])' + $escaped + '\s*(?:<[^<>\r\n]+>\s*)?(?:\(|\{|=>|$)'
    $memberMatches = @(for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -match $declaration) { $index }
    })
    if ($memberMatches.Count -eq 0) {
        $literal = '(?<![\w])' + $escaped + '(?![\w])'
        $memberMatches = @(for ($index = 0; $index -lt $lines.Count; $index++) {
            if ($lines[$index] -match $literal) { $index }
        })
    }
    if ($memberMatches.Count -eq 0) { throw "Member '$Member' was not found. Full source: $source" }
    foreach ($match in $memberMatches) {
        for ($index = [Math]::Max($StartLine - 1, $match - $ContextLines);
            $index -le [Math]::Min($lines.Count - 1, $match + $ContextLines); $index++) {
            [void]$selected.Add($index)
        }
    }
} else {
    for ($index = $StartLine - 1; $index -lt $lines.Count; $index++) { [void]$selected.Add($index) }
}
if ($selected.Count -eq 0) { throw "No selected lines at or after $StartLine. Full source: $source" }
Write-Host "Source: $source ($($lines.Count) lines); assembly version: $($assembly.VersionInfo.ProductVersion)"
$shown = 0
foreach ($index in $selected) {
    if ($shown -ge $MaxLines) { break }
    Write-Host ('{0,5}: {1}' -f ($index + 1), $lines[$index])
    $shown++
}
Write-Host "Shown $shown of $($selected.Count) selected lines. Full UTF-8 source and process exit records are saved above."
