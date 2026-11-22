[CmdletBinding()]
param(
    [Parameter(Position=0)]
    [ValidateSet('hooks', 'doctor', 'restore', 'import', 'verify', 'build', 'check', 'rebase', 'anim', 'card-tests', 'api', 'api-tests', 'observe', 'publish', 'deploy', 'editor')]
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
    [string]$SeedParts,
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
if (-not $PSBoundParameters.ContainsKey('Configuration') -and $Task -in @('publish', 'deploy')) {
    $Configuration = 'Release'
}
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
# Human requester: wjdrb, 2026-10-29, retrospective item 1: connect final
# card dimensions/alpha checks to the existing local validation route.
# Human requester: wjdrb, 2026-11-19: run the repository PNG alpha gate.
# Human requester: KyuSeok Jung (wjdrb), 2026-11-22: run all five Python static
# gates (tracked references, docs artifacts, install-package policy, card
# portraits, PNG alpha) via `python -X utf8` across verify, build, check,
# publish, and deploy.
if ($Task -in @('verify', 'build', 'check', 'publish', 'deploy')) {
    foreach ($check in @(
        @('scripts/check_tracked_references.py', 'Tracked-reference check failed.'),
        @('scripts/check_docs_artifacts.py', 'Docs artifact check failed.'),
        @('scripts/check_install_package.py', 'Install-package policy check failed.'),
        @('scripts/check_card_portraits.py', 'Card portrait check failed.'),
        @('scripts/check_png_alpha.py', 'PNG alpha check failed.')
    )) {
        & python -X utf8 (Join-Path $projectRoot $check[0])
        if ($LASTEXITCODE -ne 0) { throw $check[1] }
    }
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

function Invoke-DotNetBuild([string]$BuildConfiguration) {
    if ($BuildConfiguration -eq 'Release') {
        # Godot.NET.Sdk adds GodotSharpEditor only under Debug; restore with the
        # locked Debug graph first so Release compiles without NU1004 lock drift.
        Invoke-DotNet @('restore', 'Sts2WD.csproj', '--locked-mode')
        Invoke-DotNet @('build', 'Sts2WD.csproj', '-c', 'Release', '--no-restore')
        return
    }
    Invoke-DotNet @('build', 'Sts2WD.csproj', '-c', $BuildConfiguration, '-p:RestoreLockedMode=true')
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

# Human requester: wjdrb, 2026-11-22, retrospective item 1: gate the `anim`
# source-build workflow with `rigc render --folds` at 24 fps so mesh folds
# fail on the compiled rig-c output before baking into runtime assets.
function Invoke-RigFoldCheck([string]$CompiledDirectory, [string]$LogDirectory, [string]$RequestedClip, [string]$RequestedVariant) {
    $buildDir = Join-Path $CompiledDirectory 'build'
    $modelPath = Join-Path $buildDir 'skeleton.model.json'
    if (-not (Test-Path -LiteralPath $modelPath -PathType Leaf)) {
        throw "Compiled rig model not found: $modelPath"
    }
    $model = Get-Content -LiteralPath $modelPath -Encoding UTF8 -Raw | ConvertFrom-Json
    $authoredClips = @()
    if ($null -ne $model.animations) {
        if ($model.animations -is [System.Array]) {
            $authoredClips = @($model.animations | ForEach-Object { $_.name } | Where-Object { $_ })
        } elseif ($model.animations.PSObject.Properties['name']) {
            $authoredClips = @($model.animations.name)
        } else {
            $authoredClips = @($model.animations.PSObject.Properties.Name | Where-Object { $_ })
        }
    }
    if ($authoredClips.Count -eq 0) {
        throw "Compiled rig model has no authored animations: $modelPath"
    }

    $mappedClips = switch ($RequestedClip) {
        'all' { $authoredClips }
        'idle' {
            switch ($RequestedVariant) {
                'merchant' { @('relaxed_loop') }
                'combat' { @('idle_loop') }
                'both' { @('idle_loop', 'relaxed_loop') }
                default { throw "Unsupported animation variant '$RequestedVariant'." }
            }
        }
        'relaxed' { @('relaxed_loop') }
        'powerup' { @('cast') }
        default { @($RequestedClip) }
    }

    $targetClips = @()
    foreach ($clipName in @($mappedClips)) {
        if ($clipName -cnotin $targetClips) {
            if ($clipName -cnotin $authoredClips) {
                throw "Target clip '$clipName' (from -Clip '$RequestedClip', -Variant '$RequestedVariant') is not in $modelPath (available: $($authoredClips -join ', '))"
            }
            $targetClips += $clipName
        }
    }

    $hasPhysics = (@($model.physics | Where-Object { $null -ne $_ }).Count -gt 0) -or
        (@($model.constraints | Where-Object { $null -ne $_ -and $_.type -eq 'physics' }).Count -gt 0)
    $powershell = (Get-Command powershell.exe -ErrorAction Stop).Source
    $rigWrapper = Join-Path $projectRoot '.tools/rig-c/setup/run.ps1'

    foreach ($targetClip in $targetClips) {
        $foldOut = Join-Path $LogDirectory ('folds-' + $targetClip)
        $logPrefix = Join-Path $LogDirectory ('folds-' + $targetClip)
        $rigArgs = @(
            'rigc', 'render',
            '--candidate', $buildDir,
            '--animation', $targetClip,
            '--fps', '24',
            '--max', '1024',
            '--folds',
            '--out', $foldOut
        )
        if ($hasPhysics -and $targetClip.EndsWith('_loop', [StringComparison]::Ordinal)) {
            $rigArgs += @('--preroll', '1')
        }
        # Invoke through -Command with single-quoted literals because powershell.exe -File
        # parses --out as an ambiguous common parameter (-OutVariable/-OutBuffer) on run.ps1.
        $quotedTokens = @($rigWrapper) + $rigArgs | ForEach-Object { "'" + ($_ -replace "'", "''") + "'" }
        $command = "`$ErrorActionPreference = 'Stop'; & " + ($quotedTokens -join ' ') + '; exit $LASTEXITCODE'
        Invoke-AnimationProcess $powershell @('-NoProfile', '-NonInteractive', '-ExecutionPolicy', 'Bypass', '-Command', $command) $logPrefix

        $foldsPath = Join-Path (Join-Path $foldOut ($targetClip + '@24fps')) 'folds.json'
        if (-not (Test-Path -LiteralPath $foldsPath -PathType Leaf)) {
            throw "Missing mesh-fold report '$foldsPath' for clip '$targetClip'; read $logPrefix.*.log"
        }
        $folds = Get-Content -LiteralPath $foldsPath -Encoding UTF8 -Raw | ConvertFrom-Json
        if ($folds.spec -cne 'rigc-folds/1') {
            throw "Unexpected mesh-fold report spec '$($folds.spec)' in $foldsPath; read $logPrefix.*.log"
        }
        if ($null -eq $folds.meshes) {
            throw "Invalid mesh-fold report (missing 'meshes') in $foldsPath; read $logPrefix.*.log"
        }
        $meshes = @($folds.meshes | Where-Object { $null -ne $_ })
        $foldedDetails = @()
        foreach ($mesh in $meshes) {
            $foldedFrames = @($mesh.frames | Where-Object { $null -ne $_ })
            if ($foldedFrames.Count -gt 0 -or $null -ne $mesh.worst) {
                $frameIndices = @($foldedFrames | ForEach-Object {
                    if ($null -ne $_.index) { $_.index } else { $_ }
                })
                if ($frameIndices.Count -eq 0 -and $null -ne $mesh.worst -and $null -ne $mesh.worst.index) {
                    $frameIndices = @($mesh.worst.index)
                }
                $foldedDetails += "$($mesh.slot)/$($mesh.attachment) (frames: $($frameIndices -join ', '))"
            }
        }
        if ($foldedDetails.Count -gt 0) {
            throw "Mesh fold detected in clip '$targetClip': $($foldedDetails -join '; '). Inspect $foldsPath and $logPrefix.*.log"
        }
        Write-Host "PASS: Realta mesh folds ($($targetClip): 0/$($meshes.Count) meshes folded at 24 fps)"
    }
}

Push-Location -LiteralPath $projectRoot
try {
    if ($Task -eq 'restore') {
        Invoke-DotNet @('restore', 'Sts2WD.csproj', '--locked-mode')
        return
    }
    if ($Task -eq 'build') {
        Invoke-DotNetBuild $Configuration
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
    if ($Task -eq 'import') {
        $importDir = New-ObservationDirectory 'import'
        $engineLog = Join-Path $importDir 'import.engine.log'
        Invoke-RecordedCommand -Executable $paths.GodotPath -Arguments @('--log-file', $engineLog,
            '--headless', '--editor', '--path', $projectRoot, '--import', '--quit') -LogDirectory $importDir -Step 'import' -Summary
        $importLog = Get-Content -LiteralPath (Join-Path $importDir 'import.log') -Encoding UTF8 -Raw
        if ($importLog -match 'SCRIPT ERROR:|Parse Error:|Failed (?:to load script|loading resource)') {
            Write-RecordedLogExcerpt -LogPath (Join-Path $importDir 'import.log') -IncludeTail
            throw "Resource import failed; read $importDir/import.log"
        }
        Write-Host "Imported resources; logs: $importDir"
        return
    }
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
        if ($SeedParts) {
            if ($PackagePath) { throw '-SeedParts applies only to source builds and cannot be combined with -PackagePath.' }
            if (-not (Test-Path -LiteralPath $SeedParts -PathType Container)) {
                throw "SeedParts directory not found: $SeedParts"
            }
            $SeedParts = (Resolve-Path -LiteralPath $SeedParts).ProviderPath
            if ($SeedParts.Length -gt 3) { $SeedParts = $SeedParts.TrimEnd('\', '/') }
        }
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
            $compiled = Join-Path $animationOutput 'compiled'
            $buildArguments = @('-NoProfile', '-NonInteractive', '-ExecutionPolicy', 'Bypass',
                '-File', (Join-Path $animationSource 'rigc/build.ps1'), '-Output', $compiled)
            if ($SeedParts) { $buildArguments += @('-SeedParts', $SeedParts) }
            Invoke-AnimationProcess (Get-Command powershell.exe).Source $buildArguments (Join-Path $animationOutput 'author')
            # Human requester: wjdrb, 2026-11-22, retrospective item 1: run the
            # 24 fps mesh-fold gate right after rig-c compilation.
            Invoke-RigFoldCheck $compiled $animationOutput $Clip $Variant
            Invoke-AnimationProcess $python @('-X', 'utf8', '-B', (Join-Path $projectRoot 'scripts/realta/import_game_rig.py'),
                $compiled, (Join-Path $projectRoot 'Sts2WD/animations/realta/combat')) (Join-Path $animationOutput 'bake')
            Invoke-AnimationProcess $paths.GodotPath @('--headless', '--editor', '--path', $projectRoot, '--import', '--quit') `
                (Join-Path $animationOutput 'import') -Engine
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

    New-Item -ItemType Directory -Path (Join-Path $projectRoot 'artifacts') -Force | Out-Null
    New-Item -ItemType File -Path (Join-Path $projectRoot 'artifacts/.gdignore') -Force | Out-Null
    if ($Task -eq 'check') {
        # Human requester: wjdrb, 2026-10-23: animation import and rig checks run only through `anim`.
        Invoke-DotNetBuild $Configuration
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
        pckReport = $null
        installation = $null
    }
    try {
        Invoke-RecordedCommand -Executable $dotnet -Arguments @('--version') -LogDirectory $diagnosticRoot -Step 'dotnet-version' -Summary
        Invoke-RecordedCommand -Executable $paths.GodotPath -Arguments @('--headless', '--version') -LogDirectory $diagnosticRoot -Step 'megadot-version' -Summary
        Invoke-DotNetBuild $Configuration
        # Import textures before exporting a PCK on a fresh checkout.
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
        $expectedArtifacts = if ($Configuration -eq 'Release') {
            @('Sts2WD.dll', 'Sts2WD.pck', 'Sts2WD.json')
        } else {
            @('Sts2WD.dll', 'Sts2WD.pck', 'Sts2WD.json', 'Sts2WD.pdb')
        }
        foreach ($name in $expectedArtifacts) {
            if (-not (Test-Path -LiteralPath (Join-Path $output $name) -PathType Leaf)) { throw "Missing publish artifact: $name" }
        }
        if ($Configuration -eq 'Release' -and (Test-Path -LiteralPath (Join-Path $output 'Sts2WD.pdb') -PathType Leaf)) {
            throw "Unexpected debug symbols in Release publish output: $(Join-Path $output 'Sts2WD.pdb')"
        }

        # Human requester: KyuSeok Jung (wjdrb), 2026-11-22: run Stage 2 post-export
        # PCK allowlist/provenance/bundle gate and record size/format breakdown.
        $python = (Get-Command python -ErrorAction Stop).Source
        $pckReportPath = Join-Path $diagnosticRoot 'pck-report.json'
        $run.pckReport = $pckReportPath
        Invoke-RecordedCommand -Executable $python -Arguments @(
            '-X', 'utf8',
            (Join-Path $projectRoot 'scripts/check_pck_package.py'),
            (Join-Path $output 'Sts2WD.pck'),
            '--configuration', $Configuration,
            '--json-out', $pckReportPath
        ) -LogDirectory $diagnosticRoot -Step 'pck-check'

        if ($Task -eq 'deploy') {
            $run.installation = Install-ModPackage -Source $output -Destination (Join-Path $paths.Sts2Path 'mods/Sts2WD') -BackupRoot (Join-Path $projectRoot 'artifacts/runtime') -Configuration $Configuration
        } else {
            Write-Host "Published ($Configuration): $output"
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
