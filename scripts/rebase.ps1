[CmdletBinding()]
param(
    [string]$Upstream = 'origin/main',
    # Internal sequence-editor argument, supplied by Git before replay begins.
    [string]$TodoPath
)

$ErrorActionPreference = 'Stop'

# Human requester: wjdrb, 2026-10-24, approved retrospective item 1:
# run the existing repository pre-commit check before a rebase is complete,
# including after conflict continuation. Keep its checks and criteria intact.
# Git owns the queued check so continuation also works while this script is
# absent from an intermediate commit. A failed check is rescheduled by Git.
if ($TodoPath) {
    [IO.File]::AppendAllText($TodoPath,
        "`nexec git -c core.hooksPath=.githooks hook run pre-commit`n",
        [Text.UTF8Encoding]::new($false))
    exit 0
}

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
# Git invokes its editor through sh. Quote the script as a single shell word,
# including checkout paths containing spaces, apostrophes or shell metacharacters.
$quotedScript = "'" + $PSCommandPath.Replace('\', '/').Replace("'", "'\''") + "'"
$previousSequenceEditor = $env:GIT_SEQUENCE_EDITOR
Push-Location -LiteralPath $repositoryRoot
try {
    $env:GIT_SEQUENCE_EDITOR = "powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $quotedScript -TodoPath"
    & git rebase --interactive --no-autosquash --no-autostash --reschedule-failed-exec -- $Upstream
    $rebaseExitCode = $LASTEXITCODE
} finally {
    $env:GIT_SEQUENCE_EDITOR = $previousSequenceEditor
    Pop-Location
}
exit $rebaseExitCode
