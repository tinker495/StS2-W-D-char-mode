[CmdletBinding()]
param(
    [Parameter(Position=0)]
    [ValidateSet('hooks', 'doctor', 'restore', 'verify', 'build', 'check', 'rebase', 'anim', 'card-tests', 'api', 'api-tests', 'observe', 'publish', 'deploy', 'editor')]
    [string]$Task = 'build',
    [Parameter(Position=1)]
    [ValidateSet('all', 'idle', 'idle_loop', 'attack', 'hurt', 'cast', 'powerup', 'die', 'relaxed', 'relaxed_loop', 'revive')]
    [string]$Clip = 'attack',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [ValidateSet('combat', 'merchant', 'both')]
    [string]$Variant = 'combat',
    [string]$OutputDirectory,
    [string]$Baseline,
    [string]$PackagePath,
    [string]$Upstream = 'origin/main',
    [Alias('Type')]
    [string]$TypeName,
    [string]$Member,
    [string]$AssemblyPath,
    [string]$IlSpyPath,
    [string]$ScriptPath,
    [ValidateRange(1, 2147483647)]
    [int]$StartLine = 1,
    [ValidateRange(1, 200)]
    [int]$MaxLines = 80,
    [ValidateRange(0, 20)]
    [int]$ContextLines = 8,
    [ValidateSet('unspecified', 'sandbox', 'escalated', 'local')]
    [string]$ExecutionContextLabel = 'unspecified'
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
. (Join-Path $projectRoot 'scripts/dev-workflow.ps1')
$diagnosticRoot = $null
if ($Task -eq 'observe') {
    if (-not $ScriptPath) { throw 'observe requires -ScriptPath <temporary PowerShell script>.' }
    & (Join-Path $projectRoot 'scripts/observe.ps1') -ScriptPath $ScriptPath -OutputDirectory $OutputDirectory
    return
}

if ($Task -eq 'api-tests') {
    # Human requester: wjdrb, 2026-10-25: automatically check that member
    # lookup includes both generic and non-generic FromCard declarations.
    & (Join-Path $projectRoot 'tests/api-member-selection.ps1')
    return
}

if ($Task -eq 'api') {
    if (-not $TypeName) { throw 'api requires -Type <fully-qualified type name>.' }
    & (Join-Path $projectRoot 'scripts/inspect-game-api.ps1') -TypeName $TypeName -Member $Member `
        -AssemblyPath $AssemblyPath -IlSpyPath $IlSpyPath -OutputDirectory $OutputDirectory `
        -StartLine $StartLine -MaxLines $MaxLines -ContextLines $ContextLines
    return
}

if ($Task -eq 'card-tests') {
    # Human requester: wjdrb, 2026-10-25: add formal card-preview tests and
    # connect them to a local command using the installed game DLL.
    & (Join-Path $projectRoot 'scripts/test-card-previews.ps1') -Configuration $Configuration
    return
}

if ($Task -eq 'hooks') {
    & git -C $projectRoot config --local core.hooksPath .githooks
    if ($LASTEXITCODE -ne 0) { throw 'Cannot configure Git hooks.' }
    Write-Host 'Git pre-commit build check enabled for this checkout.'
    return
}

if ($Task -eq 'rebase') {
    & (Join-Path $projectRoot 'scripts/rebase.ps1') -Upstream $Upstream
    if ($LASTEXITCODE -ne 0) {
        throw 'Rebase or final validation did not complete. Follow the Git diagnostics, resolve the failure, then run git rebase --continue.'
    }
    return
}

# Human requester: wjdrb, 2026-10-24: run the full tracked-reference validator
# in pre-commit (through build), check, and the SDK/game-independent verify task.
if ($Task -in @('verify', 'build', 'check')) {
    & (Join-Path $projectRoot 'scripts/check-tracked-references.ps1')
    # Human requester: wjdrb, 2026-10-29, retrospective item 1: connect final
    # card dimensions/alpha checks to the existing local validation route.
    & (Join-Path $projectRoot '.agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1') check-cards
    if ($Task -eq 'verify') { return }
}

$dotnet = Initialize-DevelopmentDotNet $projectRoot

function Invoke-DotNet([string[]]$Arguments) {
    if ($diagnosticRoot) {
        Invoke-RecordedCommand -Executable $dotnet -Arguments $Arguments -LogDirectory $diagnosticRoot -Step $Arguments[0] -Summary
        return
    }
    & $dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet failed (exit $LASTEXITCODE): $Arguments" }
}

function New-ObservationDirectory([string]$Prefix) {
    $artifacts = Join-Path $projectRoot 'artifacts'
    New-Item -ItemType Directory -Path $artifacts -Force | Out-Null
    New-Item -ItemType File -Path (Join-Path $artifacts '.gdignore') -Force | Out-Null
    $directory = Join-Path $artifacts ($Prefix + '-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $directory | Out-Null
    return $directory
}

function Invoke-AnimationProcess([string]$Executable, [string[]]$Arguments, [string]$LogPrefix, [string]$Completion, [switch]$Engine) {
    if ($Engine) {
        # A fresh workspace log avoids MegaDot's startup crash while rotating
        # its default AppData log under restricted permissions.
        $Arguments = @('--log-file', ($LogPrefix + '.engine.log')) + $Arguments
    }
    $quotedArguments = $Arguments | ForEach-Object { '"' + $_ + '"' }
    $process = Start-Process -FilePath $Executable -ArgumentList $quotedArguments `
        -WorkingDirectory $projectRoot -WindowStyle Hidden -Wait -PassThru `
        -RedirectStandardOutput ($LogPrefix + '.stdout.log') `
        -RedirectStandardError ($LogPrefix + '.stderr.log')
    # Preserve the child result independently of decoding/displaying its logs.
    [System.IO.File]::WriteAllText($LogPrefix + '.exitcode', [string]$process.ExitCode, [System.Text.Encoding]::UTF8)
    $stdout = Get-Content -LiteralPath ($LogPrefix + '.stdout.log') -Encoding UTF8 -Raw
    $stderr = Get-Content -LiteralPath ($LogPrefix + '.stderr.log') -Encoding UTF8 -Raw
    if ($process.ExitCode -ne 0 -or "$stdout`n$stderr" -match 'SCRIPT ERROR:|Parse Error:|Failed (?:to load script|loading resource)') {
        throw "Animation stage failed (exit $($process.ExitCode)); read $LogPrefix.*.log"
    }
    if ($Completion -and -not $stdout.Contains($Completion)) {
        throw "Missing completion marker '$Completion'; read $LogPrefix.*.log"
    }
}

function Invoke-RigChecks([string]$GodotExecutable, [string]$LogDirectory) {
    # Human requester: wjdrb, 2026-10-20 retrospective item 2 added the mechanical rig check;
    # on 2026-10-23 they limited it to the `anim` workflow. Keep the authorized isolation test intact.
    foreach ($check in @(
        @('check_rig.gd', 'PASS: Realta rig checks'),
        @('realta_instance_isolation.gd', 'PASS: Realta instance isolation (4 cases)')
    )) {
        Invoke-AnimationProcess $GodotExecutable @('--headless', '--path', $projectRoot, '--script',
            (Join-Path $projectRoot ('tests/' + $check[0]))) (Join-Path $LogDirectory $check[0]) $check[1] -Engine
        Write-Host $check[1]
    }
}

Push-Location -LiteralPath $projectRoot
try {
    if ($Task -eq 'restore') {
        Invoke-DotNet @('restore', 'Sts2WD.csproj', '--locked-mode')
        return
    }
    if ($Task -eq 'build') {
        Invoke-DotNet @('build', 'Sts2WD.csproj', '-c', $Configuration, '-p:RestoreLockedMode=true')
        return
    }

    $paths = Get-DevelopmentPaths $projectRoot $dotnet

    if ($Task -eq 'doctor') {
        Invoke-DotNet @('--version')
        $paths | Format-List
        foreach ($path in @((Join-Path $paths.Sts2DataDir 'sts2.dll'), $paths.GodotPath)) {
            if (-not $path -or -not (Test-Path -LiteralPath $path)) { throw "Missing dependency: $path" }
        }
        & $paths.GodotPath --headless --version
        if ($LASTEXITCODE -ne 0) { throw 'MegaDot check failed.' }
        Write-Host 'Development dependencies are ready. BaseLib 3.4.7+ must also be enabled in-game.'
        return
    }

    if (-not (Test-Path -LiteralPath $paths.GodotPath)) { throw 'Set GodotPath in local.props.' }
    if ($Task -eq 'editor') {
        $editorLog = Join-Path (New-ObservationDirectory 'editor') 'engine.log'
        Start-Process -FilePath $paths.GodotPath -ArgumentList '--editor', '--path', ('"' + $projectRoot + '"'), '--log-file', ('"' + $editorLog + '"') -WorkingDirectory $projectRoot
        Write-Host "Editor log: $editorLog"
        return
    }

    if ($Task -eq 'anim') {
        $animationSource = Join-Path $projectRoot 'docs/characters/realta/animation'
        $animationTools = Join-Path $animationSource 'tools'
        $python = (Get-Command python -ErrorAction Stop).Source
        if ($Baseline) { $Baseline = (Resolve-Path -LiteralPath $Baseline).ProviderPath }
        if ($PackagePath) { $PackagePath = (Resolve-Path -LiteralPath $PackagePath).ProviderPath }
        if ($OutputDirectory) {
            $animationOutput = if ([System.IO.Path]::IsPathRooted($OutputDirectory)) {
                [System.IO.Path]::GetFullPath($OutputDirectory)
            } else {
                [System.IO.Path]::GetFullPath((Join-Path $projectRoot $OutputDirectory))
            }
            if (Test-Path -LiteralPath $animationOutput) { throw 'Use a fresh -OutputDirectory.' }
            New-Item -ItemType Directory -Path $animationOutput | Out-Null
        } else {
            $animationOutput = New-ObservationDirectory 'animation'
        }
        New-Item -ItemType File -Path (Join-Path $animationOutput '.gdignore') -Force | Out-Null
        Write-Host "Animation output: $animationOutput"
        $renderProject = $projectRoot
        $renderScript = Join-Path $animationTools 'render_clip.gd'
        if ($PackagePath) {
            $renderProject = Join-Path $animationOutput 'package-host'
            New-Item -ItemType Directory -Path $renderProject | Out-Null
            [System.IO.File]::WriteAllText((Join-Path $renderProject 'project.godot'), "config_version=5`n")
            Copy-Item -LiteralPath $renderScript -Destination (Join-Path $renderProject 'render_clip.gd')
            $renderScript = Join-Path $renderProject 'render_clip.gd'
        } else {
            Invoke-AnimationProcess $python @('-X', 'utf8', '-B', (Join-Path $animationSource 'author_clips.py')) `
                (Join-Path $animationOutput 'author')
            Invoke-AnimationProcess $paths.GodotPath @('--headless', '--editor', '--path', $projectRoot, '--import', '--quit') `
                (Join-Path $animationOutput 'import') -Engine
            Invoke-AnimationProcess $paths.GodotPath @('--headless', '--path', $projectRoot, '--script',
                (Join-Path $animationSource 'build_combat_rig.gd'), '--', $animationSource) `
                (Join-Path $animationOutput 'bake') 'baked res://Sts2WD/animations/realta/combat/realta_merchant_rig.tscn -> OK' -Engine
            Invoke-RigChecks $paths.GodotPath $animationOutput
        }
        $frames = Join-Path $animationOutput 'frames'
        $variants = if ($Variant -eq 'both') { @('combat', 'merchant') } else { @($Variant) }
        foreach ($renderVariant in $variants) {
            $renderArguments = @('--path', $renderProject, '--rendering-method', 'gl_compatibility',
                '--resolution', '640x420', '--position', '-10000,-10000', '--script', $renderScript,
                '--', $renderVariant, $Clip, $frames)
            if ($PackagePath) { $renderArguments += $PackagePath }
            Invoke-AnimationProcess $paths.GodotPath $renderArguments (Join-Path $animationOutput ('render-' + $renderVariant)) 'CAPTURE_COMPLETE' -Engine
        }
        $reviewArguments = @('-X', 'utf8', '-B', (Join-Path $animationTools 'review_frames.py'), $frames, (Join-Path $animationOutput 'review'))
        if ($Baseline) { $reviewArguments += @('--baseline', $Baseline) }
        Invoke-AnimationProcess $python $reviewArguments (Join-Path $animationOutput 'review') 'REVIEW_COMPLETE'
        Write-Host "Frames and comparison sheets: $animationOutput"
        return
    }

    # Import textures before exporting a PCK on a fresh checkout.
    New-Item -ItemType Directory -Path (Join-Path $projectRoot 'artifacts') -Force | Out-Null
    New-Item -ItemType File -Path (Join-Path $projectRoot 'artifacts/.gdignore') -Force | Out-Null
    if ($Task -eq 'check') {
        # Human requester: wjdrb, 2026-10-23: animation import and rig checks run only through `anim`.
        Invoke-DotNet @('build', 'Sts2WD.csproj', '-c', $Configuration, '-p:RestoreLockedMode=true')
        Write-Host 'Build check passed.'
        return
    }

    $diagnosticRoot = Join-Path $projectRoot ('artifacts/runtime/packaging-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ') + '-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $diagnosticRoot | Out-Null
    Write-Host "Diagnostics: $diagnosticRoot"
    $run = [ordered]@{
        task = $Task
        configuration = $Configuration
        executionContextLabel = $ExecutionContextLabel # Caller-reported, not permission detection.
        projectRoot = $projectRoot
        branch = (& git branch --show-current)
        commit = (& git rev-parse HEAD)
        changes = @(& git status --short)
        paths = $paths
        startedUtc = [DateTime]::UtcNow.ToString('o')
        finishedUtc = $null
        exitCode = $null
        failure = $null
        installation = $null
    }
    try {
        Invoke-RecordedCommand -Executable $dotnet -Arguments @('--version') -LogDirectory $diagnosticRoot -Step 'dotnet-version' -Summary
        Invoke-RecordedCommand -Executable $paths.GodotPath -Arguments @('--headless', '--version') -LogDirectory $diagnosticRoot -Step 'megadot-version' -Summary
        Invoke-DotNet @('build', 'Sts2WD.csproj', '-c', $Configuration, '-p:RestoreLockedMode=true')
        Invoke-RecordedCommand -Executable $paths.GodotPath -Arguments @('--log-file', (Join-Path $diagnosticRoot 'import.engine.log'),
            '--headless', '--editor', '--path', $projectRoot, '--import', '--quit') -LogDirectory $diagnosticRoot -Step 'import' -Summary
        $importLog = Get-Content -LiteralPath (Join-Path $diagnosticRoot 'import.log') -Encoding UTF8 -Raw
        if ($importLog -match 'SCRIPT ERROR:|Parse Error:|Failed (?:to load script|loading resource)') {
            Write-RecordedLogExcerpt -LogPath (Join-Path $diagnosticRoot 'import.log') -IncludeTail
            throw "Resource import failed; read $diagnosticRoot/import.log"
        }
        Invoke-DotNet @('publish', 'Sts2WD.csproj', '-c', $Configuration, '--no-restore')
        # Exec preserves real nonzero exits. Independently require export completion;
        # an environmental certificate diagnostic alone does not invalidate a PCK.
        $publishLog = Get-Content -LiteralPath (Join-Path $diagnosticRoot 'publish.log') -Encoding UTF8 -Raw
        $plainPublishLog = $publishLog -replace '\x1B\[[0-?]*[ -/]*[@-~]', ''
        if ($plainPublishLog -notmatch '\[\s*DONE\s*\]\s*savepack') {
            Write-RecordedLogExcerpt -LogPath (Join-Path $diagnosticRoot 'publish.log') -IncludeTail
            throw "Package export did not report completed savepack; read $diagnosticRoot/publish.log"
        }
        if ($plainPublishLog -match 'SCRIPT ERROR:|Parse Error:|Failed (?:to load script|loading resource)') {
            Write-RecordedLogExcerpt -LogPath (Join-Path $diagnosticRoot 'publish.log') -IncludeTail
            throw "Package export reported a resource error; read $diagnosticRoot/publish.log"
        }

        $output = Join-Path $paths.ModsPath 'Sts2WD'
        foreach ($name in @('Sts2WD.dll', 'Sts2WD.pck', 'Sts2WD.json', 'Sts2WD.pdb')) {
            if (-not (Test-Path -LiteralPath (Join-Path $output $name) -PathType Leaf)) { throw "Missing publish artifact: $name" }
        }
        if ($Task -eq 'deploy') {
            $run.installation = Install-ModPackage -Source $output -Destination (Join-Path $paths.Sts2Path 'mods/Sts2WD') -BackupRoot (Join-Path $projectRoot 'artifacts/runtime')
        } else {
            Write-Host "Published: $output"
        }
        $run.exitCode = 0
    } catch {
        $run.exitCode = 1
        $run.failure = $_.Exception.Message
        throw
    } finally {
        $run.finishedUtc = [DateTime]::UtcNow.ToString('o')
        $run | ConvertTo-Json -Depth 5 |
            Set-Content -LiteralPath (Join-Path $diagnosticRoot 'run.json') -Encoding UTF8
    }
} finally {
    Pop-Location
}
