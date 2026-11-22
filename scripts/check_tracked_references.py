"""Check tracked Markdown, code, and resource files for broken, untracked, or ignored references."""

# Human requester: wjdrb, 2026-10-24: fail pre-commit and CI when tracked
# Markdown or code/resource files depend on untracked or ignored local files.
# Human requester: wjdrb, 2026-10-29: exempt check_godot_project.py excluded
# project-data directory names while keeping URLs and inner paths validated.
# Human requester: tinker495 (wjdrb), 2026-11-12: reject parent-directory
# traversal (..) in non-res:// Markdown links.
# Human requester: KyuSeok Jung (wjdrb), 2026-11-22: migrate the tracked-reference
# validator from scripts/check-tracked-references.ps1 to stdlib Python under the
# Python-first mechanical verification convention with full behavioral parity.

import argparse
from bisect import bisect_right
import html
import os
from pathlib import Path
import re
import subprocess
import sys
import time
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parents[1]

EXTENSIONS = {
    ".md", ".cs", ".gd", ".tscn", ".tres", ".import",
    ".json", ".cfg", ".ps1", ".csproj", ".py", ".sh", ".props",
}

INFRASTRUCTURE_LOWER = {
    "dev.ps1",
    "scripts/dev-workflow.ps1",
    "directory.build.props",
    "sts2pathdiscovery.props",
    "sts2wd.csproj",
    "export_presets.cfg",
    "local.props.example",
    "scripts/check_tracked_references.py",
}

EXTERNAL_EVIDENCE_FILENAMES = {"sts2.dll", "BaseLib.dll", "SlayTheSpire2.pck"}

# Regexes preserving exact PowerShell case-sensitivity semantics.
TEMPLATE_RE = re.compile(r"[*?]|<[^>]*>|\{[^}]*\}|\.\.\.|%s|\$")
LOCAL_ABSOLUTE_RE = re.compile(r"^(?:[A-Za-z]:[\\/]|/(?:Users|home)/|[Ff][Ii][Ll][Ee]:/)")
MARKDOWN_SCHEME_RE = re.compile(r"^(?:(?!res:)[A-Za-z][A-Za-z0-9+.-]*:|//)", re.IGNORECASE)
MENTION_SCHEME_RE = re.compile(r"^(?:(?!res:|file:)[A-Za-z][A-Za-z0-9+.-]*:|//)", re.IGNORECASE)
DRIVE_PREFIX_RE = re.compile(r"^[A-Za-z]:/", re.IGNORECASE)
PARENT_TRAVERSAL_RE = re.compile(r"(?:^|/)\.\.(?:/|$)")
ESCAPED_MD_CHAR_RE = re.compile(r"\\([\\`*{}\[\]()#+.!_ >-])")
LINE_RANGE_SUFFIX_RE = re.compile(r":\d+(?:-\d+)?(?::\d+)?$")
INFRA_LOCAL_RE = re.compile(r"^(?:\./|/)?(?:\.tools(?:/|$)|artifacts(?:/|$)|local\.props$)", re.IGNORECASE)
TOOLS_SEGMENT_RE = re.compile(r"(?:^|/)\.tools(?:/|$)", re.IGNORECASE)
TOOLS_ALLOWED_RE = re.compile(
    r"^\.tools(?:/(?:dotnet(?:/dotnet\.exe)?|megadot(?:/.*)?|gh/bin/gh\.exe|nuget-packages|dotnet-home))?$",
    re.IGNORECASE,
)
RES_DRAFT_OR_ARTIFACT_RE = re.compile(r"^(?:drafts|artifacts)(?:/|$)", re.IGNORECASE)
DRAFTS_INNER_RE = re.compile(r"(?:^|/)drafts/.", re.IGNORECASE)
DRAFTS_DIR_RE = re.compile(r"(?:^|/)drafts$", re.IGNORECASE)
ARTIFACTS_MD_RE = re.compile(r"^(?:\./)?artifacts(?:/|$)", re.IGNORECASE)
ARTIFACTS_MD_ALLOWED_RE = re.compile(r"^(?:\./)?artifacts(?:/mods/Sts2WD(?:/.*)?|/\.gdignore)?$", re.IGNORECASE)
REPRODUCTION_ARTIFACTS_RE = re.compile(r"^artifacts/attack-(?:before(?:/frames)?|after)$", re.IGNORECASE)
IMPORT_CTEX_RE = re.compile(r"^\.godot/imported/[^/]+\.ctex$", re.IGNORECASE)
ROOT_OR_REL_PREFIX_RE = re.compile(r"^(?:/|\.\.?/)")
REPO_RES_PREFIX_RE = re.compile(r"^(?:Sts2WD|Sts2WDCode|docs|tests)(?:/|$)", re.IGNORECASE)
REL_DOT_PREFIX_RE = re.compile(r"^\.\.?/")

TSCN_PROP_KEY_RE = re.compile(r"^[\w/]+[ \t]*=[ \t]*", re.MULTILINE)
TSCN_NODEPATH_RE = re.compile(r'NodePath\("[^"\r\n]*"\)')

PATH_UNIT = r"(?:<[^<>\r\n]*>|\{[^{}\r\n]*\}|[^\s`\"'()\[\],;=<>|])"
TOKEN_RE = re.compile(r"(?<![\w.:/\\-])(?=" + PATH_UNIT + r"*(?:[/\\]|\.[A-Za-z_]))" + PATH_UNIT + r"+")
QUOTED_RE = re.compile(r"""(?P<quote>["']|`+)(?P<path>(?:(?!(?P=quote))[^\r\n])+)(?P=quote)""")
QUOTED_SPACE_RE = re.compile(r"\s")
QUOTED_PATH_HINT_RE = re.compile(
    r"^(?:res://|file:/|[A-Za-z]:[\\/]|/(?:Users|home)/|\.?\.?/|[\w.-]+/)",
    re.IGNORECASE,
)
QUOTED_EXT_HINT_RE = re.compile(r"\.[A-Za-z_][\w.-]*$", re.IGNORECASE)
QUOTED_FLAG_OR_PIPE_RE = re.compile(r"\s(?:-[A-Za-z]|[|+])", re.IGNORECASE)
TOKEN_PATH_FILTER_RE = re.compile(r"[/\\]|^[\w.-]*\.[A-Za-z_][\w.-]*$", re.IGNORECASE)
RAW_LOCAL_RE = re.compile(r"(?<![\w.:/\\])(?:[A-Za-z]:[\\/]|/(?:Users|home)/|(?i:file:/))[^\s`\"'<>]*")
IMPORT_SOURCE_RE = re.compile(r'^[ \t]*source_file[ \t]*=[ \t]*"([^"]+)"', re.MULTILINE)

FENCE_RE = re.compile(r"^\s{0,3}(`{3,}|~{3,})")
INLINE_CODE_RE = re.compile(r"(`+).*?\1")
DEFINITION_RE = re.compile(
    r"^[ \t]{0,3}\[(?:\\.|[^\]\\])+\]:[ \t]*(?:\r?\n[ \t]*)?(?:<(?P<angle>[^>\r\n]+)>|(?P<bare>\S+))",
    re.MULTILINE,
)
LINK_TITLE_TAIL_RE = re.compile(r"""(?:\s+(?:"[^"]*"|'[^']*'|\([^)]*\)))?\s*\)""")


def configure_utf8_stdio():
    for stream in (sys.stdout, sys.stderr):
        reconfigure = getattr(stream, "reconfigure", None)
        if reconfigure is not None:
            reconfigure(encoding="utf-8", errors="replace")


def invoke_git(root, arguments, input_bytes=b""):
    proc = subprocess.run(
        ["git"] + arguments,
        cwd=root,
        input=input_bytes,
        capture_output=True,
        check=False,
    )
    return proc.returncode, proc.stdout, proc.stderr.decode("utf-8", errors="replace")


def build_line_starts(text):
    starts = [0]
    offset = text.find("\n")
    while offset >= 0:
        starts.append(offset + 1)
        offset = text.find("\n", offset + 1)
    return starts


def get_line_number(starts, offset):
    return bisect_right(starts, offset)


def is_escaped(text, index):
    backslashes = 0
    pos = index - 1
    while pos >= 0 and text[pos] == "\\":
        backslashes += 1
        pos -= 1
    return (backslashes % 2) == 1


def mask_markdown_for_links(content):
    """Mask fenced code blocks and inline code spans while preserving newlines."""
    masked_lines = []
    fence = ""
    for line in content.split("\n"):
        masked = line
        match = FENCE_RE.match(line)
        if match:
            marker = match.group(1)
            if not fence:
                fence = marker
            elif marker[0] == fence[0] and len(marker) >= len(fence):
                fence = ""
            masked = ""
        elif fence:
            masked = ""
        else:
            masked = INLINE_CODE_RE.sub(" ", masked)
        masked_lines.append(masked)
    return "\n".join(masked_lines)


def parse_inline_link_destination(text, start):
    """Parse Markdown inline link destination starting right after '('."""
    n = len(text)
    pos = start
    while pos < n and text[pos].isspace():
        pos += 1
    if pos >= n:
        return None

    if text[pos] == "<":
        end = pos + 1
        while end < n and text[end] not in ">\r\n":
            end += 1
        if end >= n or text[end] != ">":
            return None
        target = text[pos + 1 : end]
        after = end + 1
    else:
        end = pos
        paren_depth = 0
        while end < n:
            ch = text[end]
            if ch == "\\" and end + 1 < n:
                end += 2
                continue
            if ch.isspace():
                break
            if ch == "(":
                paren_depth += 1
                end += 1
                continue
            if ch == ")":
                if paren_depth == 0:
                    break
                paren_depth -= 1
                end += 1
                continue
            end += 1
        if paren_depth != 0 or end == pos:
            return None
        target = text[pos:end]
        after = end

    tail_match = LINK_TITLE_TAIL_RE.match(text, after)
    if tail_match is None:
        return None
    return target, tail_match.end()


def iter_markdown_links(masked_text, starts):
    """Linear-time stack-based scanner for Markdown inline links and images."""
    n = len(masked_text)
    bracket_stack = []
    i = 0
    while i < n:
        ch = masked_text[i]
        if ch == "\\" and i + 1 < n:
            i += 2
            continue
        if ch == "[":
            bracket_stack.append(i)
            i += 1
            continue
        if ch == "]" and bracket_stack:
            open_pos = bracket_stack.pop()
            if i + 1 < n and masked_text[i + 1] == "(":
                parsed = parse_inline_link_destination(masked_text, i + 2)
                if parsed is not None:
                    target, end_pos = parsed
                    link_start = open_pos - 1 if (open_pos > 0 and masked_text[open_pos - 1] == "!" and not is_escaped(masked_text, open_pos - 1)) else open_pos
                    yield get_line_number(starts, link_start), target
                    i = end_pos
                    continue
            i += 1
            continue
        i += 1

    for match in DEFINITION_RE.finditer(masked_text):
        target = match.group("angle") if match.group("angle") is not None else match.group("bare")
        yield get_line_number(starts, match.start()), target


def test_path_template(target):
    return TEMPLATE_RE.search(target) is not None


def test_local_absolute_path(target):
    return LOCAL_ABSOLUTE_RE.match(target) is not None


def check_repository(root, verbose=False):
    start_time = time.monotonic()
    root_str = os.path.abspath(str(root))
    root_prefix = root_str.rstrip("\\/") + os.sep
    root_prefix_lower = root_prefix.lower()

    code, out_bytes, err_text = invoke_git(root, ["ls-files", "--cached", "-z", "--deduplicate"])
    if code != 0:
        raise RuntimeError(f"Cannot list tracked files: {err_text.strip()}")

    tracked = {""}
    tracked_files = set()
    files = []
    repository_directories = {"Sts2WD", "Sts2WDCode", "docs", "tests", "scripts", ".agents", ".github"}

    for raw in out_bytes.split(b"\0"):
        if not raw:
            continue
        rel = raw.decode("utf-8")
        full = os.path.join(root_str, rel)
        if not os.path.isfile(full):
            continue
        if rel in tracked:
            continue
        tracked.add(rel)
        tracked_files.add(rel)
        files.append(rel)
        if "/" in rel:
            repository_directories.add(rel.split("/", 1)[0])
        parent = rel
        while "/" in parent:
            parent = parent.rsplit("/", 1)[0]
            tracked.add(parent)

    issues = []
    issue_keys = set()
    ignore_candidates = set()
    pending = []

    def add_issue(source, line, target, reason):
        key = (source, line, target, reason)
        if key not in issue_keys:
            issue_keys.add(key)
            issues.append(key)

    def resolve_reference_paths(source, target, root_only=False, is_link=False):
        target_path = target.replace("\\", "/")
        if "\x00" in target_path:
            return []
        rooted = root_only or target_path.startswith("/")
        target_path = target_path.lstrip("/")
        if not target_path and rooted:
            return [""]
        bases = [""]
        if not rooted:
            source_dir = source.rsplit("/", 1)[0] if "/" in source else ""
            bases = [source_dir]
            if not is_link and source_dir != "" and not REL_DOT_PREFIX_RE.match(target_path):
                bases.append("")
        resolved_list = []
        for base in bases:
            try:
                if os.path.isabs(target_path):
                    continue
                joined = os.path.join(root_str, base, target_path) if base else os.path.join(root_str, target_path)
                absolute = os.path.abspath(joined)
            except (OSError, ValueError):
                continue
            if absolute.lower() == root_str.lower():
                resolved_list.append("")
                continue
            if absolute.lower().startswith(root_prefix_lower):
                rel_out = absolute[len(root_prefix):].replace("\\", "/").rstrip("/")
                resolved_list.append(rel_out)
        return resolved_list

    def check_markdown_link(source, line, target):
        target_path = html.unescape(target)
        target_path = ESCAPED_MD_CHAR_RE.sub(r"\1", target_path)
        target_path = unquote(target_path)
        if target_path == "" or target_path.startswith("#"):
            return
        if test_local_absolute_path(target_path):
            add_issue(source, line, target, "local absolute path")
            return
        if MARKDOWN_SCHEME_RE.match(target_path):
            return
        target_path = re.split(r"[#?]", target_path, maxsplit=1)[0]
        is_resource = target_path.startswith("res://")
        if is_resource:
            target_path = target_path[6:]
        if not is_resource and PARENT_TRAVERSAL_RE.search(target_path.replace("\\", "/")):
            add_issue(source, line, target, "Markdown repository links must not traverse parent directories (..)")
            return
        for resolved in resolve_reference_paths(source, target_path, root_only=is_resource, is_link=True):
            if resolved in tracked:
                return
        add_issue(source, line, target, "Markdown target is not a tracked file or directory")

    def check_path_mention(source, line, target, is_markdown):
        target_path = unquote(html.unescape(target)).replace("\\", "/")
        target_path = LINE_RANGE_SUFFIX_RE.sub("", target_path)
        if target_path == "" or target_path.startswith("#"):
            return
        if MENTION_SCHEME_RE.match(target_path) and not DRIVE_PREFIX_RE.match(target_path):
            return
        source_lower = source.lower()
        if source_lower == "scripts/check_tracked_references.py" and not target_path.startswith("res://"):
            return
        if not is_markdown and source_lower in INFRASTRUCTURE_LOWER and INFRA_LOCAL_RE.match(target_path):
            return
        if test_local_absolute_path(target_path):
            add_issue(source, line, target, "local absolute path")
            return
        is_resource = target_path.startswith("res://")
        if is_resource:
            target_path = target_path[6:]

        if is_markdown and TOOLS_SEGMENT_RE.search(target_path):
            idx = target_path.lower().find(".tools")
            tool_path = target_path[idx:].rstrip("/")
            if TOOLS_ALLOWED_RE.match(tool_path):
                return
            add_issue(source, line, target, "personal tool or decompiler cache")
            return
        if is_resource and RES_DRAFT_OR_ARTIFACT_RE.match(target_path):
            add_issue(source, line, target, "local draft or artifact dependency")
            return
        if test_path_template(target_path):
            return
        if DRAFTS_INNER_RE.search(target_path):
            add_issue(source, line, target, "local draft or artifact dependency")
            return
        if is_markdown and ARTIFACTS_MD_RE.match(target_path):
            if ARTIFACTS_MD_ALLOWED_RE.match(target_path.rstrip("/")):
                return
            if source_lower == "docs/characters/realta/animation/reproduction.md" and REPRODUCTION_ARTIFACTS_RE.match(target_path):
                return
            add_issue(source, line, target, "ephemeral artifact reference")
            return
        if is_markdown and DRAFTS_DIR_RE.search(target_path.rstrip("/")):
            return
        if not is_markdown and source.endswith(".import") and IMPORT_CTEX_RE.match(target_path):
            return
        if (
            not is_markdown
            and not is_resource
            and source == ".agents/skills/godot-nodes-scenes/scripts/check_godot_project.py"
            and target_path in {".godot", ".godot/"}
        ):
            return
        if (
            is_markdown
            and source.startswith(".agents/skills/")
            and not is_resource
            and not ROOT_OR_REL_PREFIX_RE.match(target_path)
            and target_path.split("/", 1)[0] not in repository_directories
        ):
            return

        targets = resolve_reference_paths(source, target_path, root_only=is_resource, is_link=False)
        for resolved in targets:
            if resolved in tracked:
                return
        if not is_markdown and is_resource and REPO_RES_PREFIX_RE.match(target_path):
            add_issue(source, line, target, "repository resource is not tracked")
            return
        for resolved in targets:
            absolute = os.path.join(root_str, resolved) if resolved else root_str
            try:
                if os.path.isfile(absolute) or os.path.isdir(absolute):
                    add_issue(source, line, target, "workspace target exists but is not tracked")
                    return
            except (OSError, ValueError):
                continue
        if is_markdown and target in EXTERNAL_EVIDENCE_FILENAMES:
            return
        if is_markdown and not is_resource:
            for resolved in targets:
                ignore_path = resolved + "/" if target_path.endswith("/") else resolved
                ignore_candidates.add(ignore_path)
                pending.append((source, line, target, ignore_path))

    scan_count = 0
    for rel in files:
        ext = os.path.splitext(rel)[1].lower()
        if ext not in EXTENSIONS and rel.lower() != "local.props.example":
            continue
        scan_count += 1
        if verbose:
            print(f"{time.monotonic() - start_time:.3f}s: {rel}")

        is_markdown = ext == ".md"
        is_import = rel.endswith(".import")
        full = os.path.join(root_str, rel)
        with open(full, "r", encoding="utf-8-sig", newline=None) as handle:
            content = handle.read()

        scan_text = content
        if rel.endswith(".tscn"):
            scan_text = TSCN_PROP_KEY_RE.sub("", scan_text)
            scan_text = TSCN_NODEPATH_RE.sub("", scan_text)

        starts = build_line_starts(scan_text)
        raw_starts = starts if scan_text is content else build_line_starts(content)
        mention_keys = set()
        whole_paths = []

        for match in QUOTED_RE.finditer(scan_text):
            token = match.group("path")
            if (
                QUOTED_SPACE_RE.search(token)
                and (QUOTED_PATH_HINT_RE.match(token) or QUOTED_EXT_HINT_RE.search(token))
                and not QUOTED_FLAG_OR_PIPE_RE.search(token)
            ):
                whole_paths.append((match.start(), match.end()))
                line_no = get_line_number(starts, match.start())
                key = (line_no, token)
                if key not in mention_keys:
                    mention_keys.add(key)
                    check_path_mention(rel, line_no, token, is_markdown)

        whole_idx = 0
        for match in TOKEN_RE.finditer(scan_text):
            m_start = match.start()
            while whole_idx < len(whole_paths) and m_start >= whole_paths[whole_idx][1]:
                whole_idx += 1
            if whole_idx < len(whole_paths) and m_start >= whole_paths[whole_idx][0]:
                continue
            token = match.group(0).strip("!:")
            if not token.endswith("..."):
                token = token.rstrip(".")
            if TOKEN_PATH_FILTER_RE.search(token):
                line_no = get_line_number(starts, m_start)
                key = (line_no, token)
                if key not in mention_keys:
                    mention_keys.add(key)
                    check_path_mention(rel, line_no, token, is_markdown)

        for match in RAW_LOCAL_RE.finditer(content):
            token = match.group(0)
            line_no = get_line_number(raw_starts, match.start())
            key = (line_no, token)
            if key not in mention_keys:
                mention_keys.add(key)
                check_path_mention(rel, line_no, token, is_markdown)

        if is_import:
            has_import_source = False
            for match in IMPORT_SOURCE_RE.finditer(content):
                has_import_source = True
                source_target = match.group(1)
                if not source_target.startswith("res://") or source_target[6:] not in tracked_files:
                    line_no = get_line_number(raw_starts, match.start())
                    add_issue(rel, line_no, source_target, "import source_file is not a tracked file")
            if not has_import_source:
                add_issue(rel, 1, "source_file", "import source_file is missing")
            sibling = rel[: -len(".import")]
            if sibling not in tracked_files:
                add_issue(rel, 1, sibling, "import sibling is not a tracked file")

        if is_markdown:
            masked = mask_markdown_for_links(content)
            masked_starts = build_line_starts(masked)
            for line_no, link_target in iter_markdown_links(masked, masked_starts):
                check_markdown_link(rel, line_no, link_target)

    if ignore_candidates:
        payload = ("\0".join(sorted(ignore_candidates)) + "\0").encode("utf-8")
        ig_code, ig_out, ig_err = invoke_git(root, ["check-ignore", "--no-index", "-z", "--stdin"], payload)
        if ig_code not in (0, 1):
            raise RuntimeError(f"Cannot check ignored references: {ig_err.strip()}")
        ignored_paths = {raw.decode("utf-8") for raw in ig_out.split(b"\0") if raw}
        for source, line_no, target, ignore_path in pending:
            if ignore_path in ignored_paths:
                add_issue(source, line_no, target, "target matches Git ignore rules")

    issues.sort(key=lambda item: (item[0], item[1], item[2], item[3]))
    return issues, scan_count, time.monotonic() - start_time


def main():
    configure_utf8_stdio()
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=ROOT, help="repository root directory")
    parser.add_argument("--verbose", action="store_true", help="print per-file scan timing")
    args = parser.parse_args()
    root = args.root.resolve()

    try:
        issues, scan_count, elapsed = check_repository(root, verbose=args.verbose)
    except (OSError, RuntimeError, UnicodeDecodeError) as error:
        print(f"Tracked-reference check failed: {error}", file=sys.stderr)
        return 1

    if issues:
        for source, line_no, target, reason in issues:
            print(f"{source}:{line_no}: {reason}: {target}")
        print(f"Tracked-reference check failed: {len(issues)} violation(s).", file=sys.stderr)
        return 1

    print(f"Tracked-reference check passed ({scan_count} tracked Markdown/code/resource files).")
    if args.verbose:
        print(f"Completed in {elapsed:.3f}s.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
