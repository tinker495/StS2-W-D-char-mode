[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$ScriptPath,
    [string]$OutputDirectory,
    # Internal child entry point; the public entry creates the output directory
    # and records this PowerShell process through Invoke-RecordedCommand.
    [switch]$RunScript
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'dev-workflow.ps1')

if ($RunScript) {
    $observationRecord = [ordered]@{
        script = $ScriptPath
        sha256 = $null
        projectRoot = $projectRoot
        outputDirectory = $OutputDirectory
        dotnet = $null
        startedUtc = [DateTime]::UtcNow.ToString('o')
        finishedUtc = $null
        status = 'running'
        phase = 'preparation'
        failure = $null
        position = $null
        scriptStackTrace = $null
    }
    $observationExitCode = 1
    Push-Location -LiteralPath $projectRoot
    try {
        if ([IO.Path]::GetExtension($ScriptPath) -ne '.ps1') { throw 'Observation input must be a PowerShell .ps1 file.' }
        $resolvedScript = (Resolve-Path -LiteralPath $ScriptPath -ErrorAction Stop).ProviderPath
        $observationRecord.sha256 = (Get-FileHash -LiteralPath $resolvedScript -Algorithm SHA256).Hash
        $observationRecord.dotnet = Initialize-DevelopmentDotNet $projectRoot
        Invoke-RecordedCommand -Executable $observationRecord.dotnet -Arguments @('--version') `
            -LogDirectory $OutputDirectory -Step 'dotnet-version' -Quiet
        $context = [pscustomobject]@{
            ProjectRoot = $projectRoot
            OutputDirectory = $OutputDirectory
            DotNet = $observationRecord.dotnet
        }
        $observationRecord.phase = 'script'
        # PowerShell cmdlet errors and property-assignment errors terminate here.
        # Native commands inside the script use Invoke-RecordedCommand so their
        # nonzero exits also stop execution and retain separate process records.
        & $resolvedScript -Context $context
        if (-not $?) { throw "Observation script returned unsuccessfully (exit $LASTEXITCODE)." }
        $observationRecord.status = 'passed'
        $observationRecord.phase = 'complete'
        $observationExitCode = 0
    } catch {
        $observationRecord.status = 'failed'
        $observationRecord.failure = $_.Exception.Message
        $observationRecord.position = $_.InvocationInfo.PositionMessage
        $observationRecord.scriptStackTrace = $_.ScriptStackTrace
        Write-Output ($_ | Out-String)
    } finally {
        $observationRecord.finishedUtc = [DateTime]::UtcNow.ToString('o')
        $observationRecord | ConvertTo-Json -Depth 4 |
            Set-Content -LiteralPath (Join-Path $OutputDirectory 'run.json') -Encoding UTF8
        Pop-Location
    }
    exit $observationExitCode
}

$ScriptPath = [IO.Path]::GetFullPath($ScriptPath)
if (-not $OutputDirectory) {
    $OutputDirectory = Join-Path $projectRoot ('artifacts/observation-' + [Guid]::NewGuid().ToString('N'))
}
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $OutputDirectory) { throw 'Use a fresh -OutputDirectory so earlier evidence is preserved.' }
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
New-Item -ItemType File -Path (Join-Path $OutputDirectory '.gdignore') | Out-Null
Write-Host "Observation diagnostics: $OutputDirectory"
$powerShell = (Get-Process -Id $PID).Path
try {
    # The child sets Stop itself: the shared native-process logger uses Continue
    # in its own scope to retain stderr, and must not control script preparation.
    Invoke-RecordedCommand -Executable $powerShell -Arguments @('-NoProfile', '-NonInteractive',
        '-ExecutionPolicy', 'Bypass', '-File', $PSCommandPath, '-RunScript', '-ScriptPath', $ScriptPath,
        '-OutputDirectory', $OutputDirectory) -LogDirectory $OutputDirectory -Step 'observation' -Quiet
} catch {
    $recordPath = Join-Path $OutputDirectory 'run.json'
    if (Test-Path -LiteralPath $recordPath) {
        $record = Get-Content -LiteralPath $recordPath -Encoding UTF8 -Raw | ConvertFrom-Json
        Write-Host "Observation failed during $($record.phase): $($record.failure)"
    }
    throw
}
$record = Get-Content -LiteralPath (Join-Path $OutputDirectory 'run.json') -Encoding UTF8 -Raw | ConvertFrom-Json
if ($record.status -ne 'passed' -or $record.phase -ne 'complete') {
    throw "Observation exited without a completed success record; read $OutputDirectory."
}
Write-Host "Observation passed. Log and completion record: $OutputDirectory"
