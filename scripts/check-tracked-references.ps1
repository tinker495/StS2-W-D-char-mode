[CmdletBinding()]
param()

# Human requester: wjdrb, 2026-10-24: fail pre-commit and CI when tracked
# Markdown or code/resource files depend on untracked or ignored local files.
# Implements the agreed full-repository scan, including the documented path conventions.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0
$repositoryRoot = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, '..'))
$repositoryPrefix = $repositoryRoot.TrimEnd([char[]]'\/') + [IO.Path]::DirectorySeparatorChar
$scanTimer = [Diagnostics.Stopwatch]::StartNew()

function Invoke-ReferenceGit([string]$Arguments, [string]$InputText = '') {
    $start = New-Object Diagnostics.ProcessStartInfo
    $start.FileName = 'git'
    $start.Arguments = $Arguments
    $start.WorkingDirectory = $repositoryRoot
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardInput = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.StandardOutputEncoding = [Text.Encoding]::UTF8
    $start.StandardErrorEncoding = [Text.Encoding]::UTF8
    $process = New-Object Diagnostics.Process
    $process.StartInfo = $start
    try {
        [void]$process.Start()
        # Drain both pipes while writing the NUL-delimited input, even for large repositories.
        $outputTask = $process.StandardOutput.ReadToEndAsync()
        $errorTask = $process.StandardError.ReadToEndAsync()
        $inputBytes = [Text.Encoding]::UTF8.GetBytes($InputText)
        $process.StandardInput.BaseStream.Write($inputBytes, 0, $inputBytes.Length)
        $process.StandardInput.Close()
        $outputText = $outputTask.GetAwaiter().GetResult()
        $errorText = $errorTask.GetAwaiter().GetResult()
        $process.WaitForExit()
        return [pscustomobject]@{ Code = $process.ExitCode; Output = $outputText; Error = $errorText }
    } finally {
        $process.Dispose()
    }
}

$listed = Invoke-ReferenceGit 'ls-files --cached -z'
if ($listed.Code -ne 0) { throw "Cannot list tracked files: $($listed.Error)" }
$tracked = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$trackedFiles = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$files = [Collections.Generic.List[string]]::new()
$repositoryDirectories = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($directory in @('Sts2WD', 'Sts2WDCode', 'docs', 'tests', 'scripts', '.agents', '.github')) {
    [void]$repositoryDirectories.Add($directory)
}
[void]$tracked.Add('') # The repository root is a tracked directory.
foreach ($file in $listed.Output.Split([char]0, [StringSplitOptions]::RemoveEmptyEntries)) {
    # Unstaged deletions must invalidate references too; staged deletions are absent from ls-files.
    if (-not [IO.File]::Exists([IO.Path]::Combine($repositoryRoot, $file))) { continue }
    if (-not $tracked.Add($file)) { continue }
    [void]$trackedFiles.Add($file)
    $files.Add($file)
    if ($file.Contains('/')) { [void]$repositoryDirectories.Add($file.Split('/')[0]) }
    $parent = $file
    while ($parent.Contains('/')) {
        $parent = $parent.Substring(0, $parent.LastIndexOf('/'))
        [void]$tracked.Add($parent)
    }
}

$issues = [Collections.Generic.List[object]]::new()
$issueKeys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$ignoreCandidates = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$pending = [Collections.Generic.List[object]]::new()
$infrastructure = @('dev.ps1', 'scripts/dev-workflow.ps1', 'Directory.Build.props',
    'Sts2PathDiscovery.props', 'Sts2WD.csproj', 'export_presets.cfg', 'local.props.example',
    'scripts/check-tracked-references.ps1')

function Add-ReferenceIssue([string]$Source, [int]$Line, [string]$Target, [string]$Reason) {
    if ($issueKeys.Add("$Source`0$Line`0$Target`0$Reason")) {
        $issues.Add([pscustomobject]@{ Source = $Source; Line = $Line; Target = $Target; Reason = $Reason })
    }
}

function Resolve-ReferencePaths([string]$Source, [string]$Target, [switch]$RootOnly, [switch]$Link) {
    $targetPath = $Target.Replace('\', '/')
    $rooted = $RootOnly -or $targetPath.StartsWith('/')
    $targetPath = $targetPath.TrimStart('/')
    $bases = @('')
    if (-not $rooted) {
        $separator = $Source.LastIndexOf('/')
        $sourceDirectory = if ($separator -ge 0) { $Source.Substring(0, $separator) } else { '' }
        $bases = @($sourceDirectory)
        # Prose and source literals also use repository-relative paths without a leading slash.
        # Markdown destinations instead follow the normal source-relative link semantics.
        if (-not $Link -and $sourceDirectory -ne '' -and $targetPath -notmatch '^\.\.?/') {
            $bases += ''
        }
    }
    foreach ($base in $bases) {
        try {
            $absolute = [IO.Path]::GetFullPath([IO.Path]::Combine($repositoryRoot, $base, $targetPath))
            if ($absolute -eq $repositoryRoot) { ''; continue }
            if ($absolute.StartsWith($repositoryPrefix, [StringComparison]::OrdinalIgnoreCase)) {
                $absolute.Substring($repositoryPrefix.Length).Replace('\', '/').TrimEnd('/')
            }
        } catch [ArgumentException] {
            # Code fragments that cannot be paths are not filesystem references.
        } catch [NotSupportedException] {
        }
    }
}

function Test-PathTemplate([string]$Target) {
    return $Target -match '[*?]|<[^>]*>|\{[^}]*\}|\.\.\.|%s|\$'
}

function Test-LocalAbsolutePath([string]$Target) {
    return $Target -cmatch '^(?:[A-Za-z]:[\\/]|/(?:Users|home)/|[Ff][Ii][Ll][Ee]:/)'
}

function Get-ReferenceLine([int[]]$Starts, [int]$Offset) {
    $position = [Array]::BinarySearch($Starts, $Offset)
    if ($position -lt 0) { return -$position - 1 }
    return $position + 1
}

function Check-MarkdownLink([string]$Source, [int]$Line, [string]$Target) {
    $targetPath = [Net.WebUtility]::HtmlDecode($Target)
    $targetPath = [regex]::Replace($targetPath, '\\([\\`*{}\[\]()#+.!_ >-])', '$1')
    $targetPath = [Uri]::UnescapeDataString($targetPath)
    if ($targetPath -eq '' -or $targetPath.StartsWith('#')) { return }
    if (Test-LocalAbsolutePath $targetPath) {
        Add-ReferenceIssue $Source $Line $Target 'local absolute path'
        return
    }
    if ($targetPath -match '^(?:(?!res:)[A-Za-z][A-Za-z0-9+.-]*:|//)') { return }
    $targetPath = ($targetPath -split '[#?]', 2)[0]
    $isResource = $targetPath.StartsWith('res://')
    if ($isResource) { $targetPath = $targetPath.Substring(6) }
    $targets = @(Resolve-ReferencePaths $Source $targetPath -RootOnly:$isResource -Link)
    foreach ($resolved in $targets) { if ($tracked.Contains($resolved)) { return } }
    Add-ReferenceIssue $Source $Line $Target 'Markdown target is not a tracked file or directory'
}

function Check-PathMention([string]$Source, [int]$Line, [string]$Target, [bool]$Markdown) {
    $targetPath = [Uri]::UnescapeDataString([Net.WebUtility]::HtmlDecode($Target)).Replace('\', '/')
    $targetPath = [regex]::Replace($targetPath, ':\d+(?:-\d+)?(?::\d+)?$', '')
    if ($targetPath -eq '' -or $targetPath.StartsWith('#')) { return }
    if ($targetPath -match '^(?:(?!res:|file:)[A-Za-z][A-Za-z0-9+.-]*:|//)' -and
        $targetPath -notmatch '^[A-Za-z]:/') { return }
    if ($Source -eq 'scripts/check-tracked-references.ps1' -and -not $targetPath.StartsWith('res://')) { return }
    if (-not $Markdown -and $Source -in $infrastructure -and
        $targetPath -match '^(?:\./|/)?(?:\.tools(?:/|$)|artifacts(?:/|$)|local\.props$)') { return }
    if (Test-LocalAbsolutePath $targetPath) {
        Add-ReferenceIssue $Source $Line $Target 'local absolute path'
        return
    }
    $isResource = $targetPath.StartsWith('res://')
    if ($isResource) { $targetPath = $targetPath.Substring(6) }

    if ($Markdown -and $targetPath -match '(?:^|/)\.tools(?:/|$)') {
        $toolPath = $targetPath.Substring($targetPath.IndexOf('.tools'))
        if ($toolPath.TrimEnd('/') -match '^\.tools(?:/(?:dotnet(?:/dotnet\.exe)?|megadot(?:/.*)?|gh/bin/gh\.exe|nuget-packages|dotnet-home))?$') { return }
        Add-ReferenceIssue $Source $Line $Target 'personal tool or decompiler cache'
        return
    }
    if ($isResource -and $targetPath -match '^(?:drafts|artifacts)(?:/|$)') {
        Add-ReferenceIssue $Source $Line $Target 'local draft or artifact dependency'
        return
    }
    if (Test-PathTemplate $targetPath) { return }
    if ($targetPath -match '(?:^|/)drafts/.') {
        Add-ReferenceIssue $Source $Line $Target 'local draft or artifact dependency'
        return
    }
    if ($Markdown -and $targetPath -match '^(?:\./)?artifacts(?:/|$)') {
        if ($targetPath.TrimEnd('/') -match '^(?:\./)?artifacts(?:/mods/Sts2WD(?:/.*)?|/\.gdignore)?$') { return }
        # These runnable examples create their own before/after output and optional baseline.
        if ($Source -eq 'docs/characters/realta/animation/reproduction.md' -and
            $targetPath -match '^artifacts/attack-(?:before(?:/frames)?|after)$') { return }
        Add-ReferenceIssue $Source $Line $Target 'ephemeral artifact reference'
        return
    }
    if ($Markdown -and $targetPath.TrimEnd('/') -match '(?:^|/)drafts$') { return }
    if (-not $Markdown -and $Source.EndsWith('.import') -and
        $targetPath -match '^\.godot/imported/[^/]+\.ctex$') { return }

    # Human requester: wjdrb, 2026-10-29: exempt only this bundled inspector's
    # excluded project-data directory names. Resource URLs and paths inside that
    # directory remain dependencies and continue through the normal checks.
    if (-not $Markdown -and -not $isResource -and
        $Source -ceq '.agents/skills/godot-nodes-scenes/scripts/check_godot_project.py' -and
        $targetPath -cin @('.godot', '.godot/')) { return }

    # Human requester: wjdrb, 2026-10-24: preserve skill sources. General examples
    # there are templates for other repositories, not this checkout's dependencies.
    # Explicit links, repository paths and the prohibited local namespaces still apply.
    if ($Markdown -and $Source.StartsWith('.agents/skills/') -and -not $isResource -and
        $targetPath -notmatch '^(?:/|\.\.?/)' -and
        -not $repositoryDirectories.Contains($targetPath.Split('/')[0])) { return }

    $targets = @(Resolve-ReferencePaths $Source $targetPath -RootOnly:$isResource)
    foreach ($resolved in $targets) { if ($tracked.Contains($resolved)) { return } }
    if (-not $Markdown -and $isResource -and $targetPath -match '^(?:Sts2WD|Sts2WDCode|docs|tests)(?:/|$)') {
        Add-ReferenceIssue $Source $Line $Target 'repository resource is not tracked'
        return
    }
    foreach ($resolved in $targets) {
        $absolute = [IO.Path]::Combine($repositoryRoot, $resolved)
        if ([IO.File]::Exists($absolute) -or [IO.Directory]::Exists($absolute)) {
            Add-ReferenceIssue $Source $Line $Target 'workspace target exists but is not tracked'
            return
        }
    }
    # Human requester: wjdrb, 2026-10-24: permit only these three external evidence
    # filenames. Check existing workspace copies above; paths containing them are not exempt.
    if ($Markdown -and $Target -cin @('sts2.dll', 'BaseLib.dll', 'SlayTheSpire2.pck')) { return }
    # Missing body mentions may be proposed filenames or historical paths. Git-ignored
    # destinations still fail even before a local copy has been created.
    if ($Markdown -and -not $isResource) {
        foreach ($resolved in $targets) {
            $ignorePath = if ($targetPath.EndsWith('/')) { $resolved + '/' } else { $resolved }
            [void]$ignoreCandidates.Add($ignorePath)
            $pending.Add([pscustomobject]@{ Source = $Source; Line = $Line; Target = $Target; Path = $ignorePath })
        }
    }
}

$extensions = @('.md', '.cs', '.gd', '.tscn', '.tres', '.import', '.json', '.cfg', '.ps1', '.csproj', '.py', '.sh', '.props')
# Keep placeholders intact (including spaces inside <...>) when finding prose/code paths.
$pathUnit = '(?:<[^<>\r\n]*>|\{[^{}\r\n]*\}|[^\s`"''()\[\],;=<>|])'
$tokenPattern = '(?<![\w.:/\\-])(?=' + $pathUnit + '*(?:[/\\]|\.[A-Za-z_]))' + $pathUnit + '+'
$quotedPattern = '(?<quote>["'']|`+)(?<path>(?:(?!\k<quote>)[^\r\n])+)\k<quote>'
$linkPattern = '(?<!\\)!?\[(?:\\.|[^\[\]\\]|\[(?<label>)|\](?<-label>))*(?(label)(?!))\]\(\s*(?:<(?<angle>[^>\r\n]+)>|(?<bare>(?:\\.|[^\\\s()]|\((?<nested>)|\)(?<-nested>))+)(?(nested)(?!)))(?:\s+(?:"[^"]*"|''[^'']*''|\([^)]*\)))?\s*\)'
$definitionPattern = '(?m)^[ \t]{0,3}\[(?:\\.|[^\]\\])+\]:[ \t]*(?:\r?\n[ \t]*)?(?:<(?<angle>[^>\r\n]+)>|(?<bare>\S+))'
$scanCount = 0
foreach ($file in $files) {
    if ([IO.Path]::GetExtension($file) -notin $extensions -and $file -ne 'local.props.example') { continue }
    $scanCount++
    Write-Verbose ('{0:F3}s: {1}' -f $scanTimer.Elapsed.TotalSeconds, $file)
    $markdown = $file.EndsWith('.md', [StringComparison]::OrdinalIgnoreCase)
    $importFile = $file.EndsWith('.import')
    $hasImportSource = $false
    $content = [IO.File]::ReadAllText([IO.Path]::Combine($repositoryRoot, $file), [Text.Encoding]::UTF8)
    $scanText = $content
    if ($file.EndsWith('.tscn')) {
        # Godot property keys and NodePath values address animation data and scene nodes.
        # Preserve newlines so diagnostic line numbers still match the source file.
        $scanText = [regex]::Replace($scanText, '(?m)^[\w/]+[ \t]*=[ \t]*', '')
        $scanText = [regex]::Replace($scanText, 'NodePath\("[^"\r\n]*"\)', '')
    }
    $lineStarts = [Collections.Generic.List[int]]::new()
    $lineStarts.Add(0)
    for ($offset = $scanText.IndexOf("`n"); $offset -ge 0; $offset = $scanText.IndexOf("`n", $offset + 1)) {
        $lineStarts.Add($offset + 1)
    }
    $starts = $lineStarts.ToArray()
    $mentionKeys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $wholePaths = [Collections.Generic.List[object]]::new()
    foreach ($match in [regex]::Matches($scanText, $quotedPattern)) {
        $token = $match.Groups['path'].Value
        if ($token -match '\s' -and ($token -match '^(?:res://|file:/|[A-Za-z]:[\\/]|/(?:Users|home)/|\.?\.?/|[\w.-]+/)' -or $token -match '\.[A-Za-z_][\w.-]*$') -and
            $token -notmatch '\s(?:-[A-Za-z]|[|+])') {
            $wholePaths.Add($match)
            $lineNumber = Get-ReferenceLine $starts $match.Index
            if ($mentionKeys.Add("$lineNumber`0$token")) { Check-PathMention $file $lineNumber $token $markdown }
        }
    }
    $wholeIndex = 0
    foreach ($match in [regex]::Matches($scanText, $tokenPattern)) {
        while ($wholeIndex -lt $wholePaths.Count -and
            $match.Index -ge ($wholePaths[$wholeIndex].Index + $wholePaths[$wholeIndex].Length)) { $wholeIndex++ }
        if ($wholeIndex -lt $wholePaths.Count -and $match.Index -ge $wholePaths[$wholeIndex].Index) { continue }
        $token = $match.Value.Trim([char[]]'!:')
        if (-not $token.EndsWith('...')) { $token = $token.TrimEnd('.') }
        if ($token -match '[/\\]|^[\w.-]*\.[A-Za-z_][\w.-]*$') {
            $lineNumber = Get-ReferenceLine $starts $match.Index
            if ($mentionKeys.Add("$lineNumber`0$token")) { Check-PathMention $file $lineNumber $token $markdown }
        }
    }
    # Inspect raw text too, including XML elements and scene values masked above.
    foreach ($match in [regex]::Matches($content, '(?<![\w.:/\\])(?:[A-Za-z]:[\\/]|/(?:Users|home)/|(?i:file:/))[^\s`"''<>]*')) {
        $lineNumber = 1 + [regex]::Matches($content.Substring(0, $match.Index), '\n').Count
        if ($mentionKeys.Add("$lineNumber`0$($match.Value)")) { Check-PathMention $file $lineNumber $match.Value $markdown }
    }

    if ($importFile) {
        foreach ($match in [regex]::Matches($content, '(?m)^[ \t]*source_file[ \t]*=[ \t]*"([^"]+)"')) {
            $hasImportSource = $true
            $sourceTarget = $match.Groups[1].Value
            if (-not $sourceTarget.StartsWith('res://') -or -not $trackedFiles.Contains($sourceTarget.Substring(6))) {
                $lineNumber = 1 + [regex]::Matches($content.Substring(0, $match.Index), '\n').Count
                Add-ReferenceIssue $file $lineNumber $sourceTarget 'import source_file is not a tracked file'
            }
        }
        if (-not $hasImportSource) { Add-ReferenceIssue $file 1 'source_file' 'import source_file is missing' }
        $sibling = $file.Substring(0, $file.Length - '.import'.Length)
        if (-not $trackedFiles.Contains($sibling)) {
            Add-ReferenceIssue $file 1 $sibling 'import sibling is not a tracked file'
        }
    }
    if ($markdown) {
        $linkText = [Text.StringBuilder]::new()
        $fence = ''
        foreach ($line in [regex]::Split($content, '\r?\n')) {
            $masked = $line
            if ($line -match '^\s{0,3}(`{3,}|~{3,})') {
                $marker = $Matches[1]
                if ($fence -eq '') { $fence = $marker }
                elseif ($marker[0] -eq $fence[0] -and $marker.Length -ge $fence.Length) { $fence = '' }
                $masked = ''
            } elseif ($fence -ne '') { $masked = '' }
            # Link-like syntax inside examples/code spans is a body mention, not a Markdown link.
            $masked = [regex]::Replace($masked, '(`+).*?\1', ' ')
            [void]$linkText.AppendLine($masked)
        }
        $text = $linkText.ToString()
        foreach ($pattern in @($linkPattern, $definitionPattern)) {
            foreach ($match in [regex]::Matches($text, $pattern)) {
                $target = if ($match.Groups['angle'].Success) { $match.Groups['angle'].Value } else { $match.Groups['bare'].Value }
                $lineNumber = 1 + [regex]::Matches($text.Substring(0, $match.Index), '\n').Count
                Check-MarkdownLink $file $lineNumber $target
            }
        }
    }
}

if ($ignoreCandidates.Count -gt 0) {
    $ignored = Invoke-ReferenceGit 'check-ignore --no-index -z --stdin' (($ignoreCandidates -join "`0") + "`0")
    if ($ignored.Code -notin @(0, 1)) { throw "Cannot check ignored references: $($ignored.Error)" }
    $ignoredPaths = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($path in $ignored.Output.Split([char]0, [StringSplitOptions]::RemoveEmptyEntries)) { [void]$ignoredPaths.Add($path) }
    foreach ($reference in $pending) {
        if ($ignoredPaths.Contains($reference.Path)) {
            Add-ReferenceIssue $reference.Source $reference.Line $reference.Target 'target matches Git ignore rules'
        }
    }
}

if ($issues.Count -gt 0) {
    foreach ($issue in ($issues | Sort-Object Source, Line, Target, Reason)) {
        Write-Host ('{0}:{1}: {2}: {3}' -f $issue.Source, $issue.Line, $issue.Reason, $issue.Target)
    }
    throw "Tracked-reference check failed: $($issues.Count) violation(s)."
}
Write-Host "Tracked-reference check passed ($scanCount tracked Markdown/code/resource files)."
Write-Verbose ('Completed in {0:F3}s.' -f $scanTimer.Elapsed.TotalSeconds)
