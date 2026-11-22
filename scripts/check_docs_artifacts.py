"""Check tracked docs/ files for applied duplicates, staging artifacts, and applied roughs."""

# Human requester: KyuSeok Jung (wjdrb), 2026-11-22: fail verify, pre-commit,
# and CI when tracked docs/ files duplicate Sts2WD/ runtime assets, duplicate
# tracked images inside docs/, retain staging candidate or overview files under
# docs/characters/, or retain applied topic roughs or orphaned Markdown indexes
# under docs/characters/<char>/refs/<topic>/.

import argparse
import hashlib
from pathlib import Path, PurePosixPath
import re
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
IMAGE_EXTS = {".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp", ".svg"}
OVERVIEW_PATTERN = re.compile(r"(?:^|[-_])overview(?:[-_]|\d|$)", re.IGNORECASE)


def configure_utf8_stdio():
    for stream in (sys.stdout, sys.stderr):
        reconfigure = getattr(stream, "reconfigure", None)
        if reconfigure is not None:
            reconfigure(encoding="utf-8", errors="replace")


def list_tracked_files(root):
    proc = subprocess.run(
        ["git", "ls-files", "-z", "--cached", "--deduplicate"],
        cwd=root,
        capture_output=True,
        check=False,
    )
    if proc.returncode != 0:
        err = proc.stderr.decode("utf-8", errors="replace").strip()
        raise RuntimeError(f"Cannot list tracked files: {err}")
    paths = []
    for raw in proc.stdout.split(b"\0"):
        if not raw:
            continue
        rel = raw.decode("utf-8")
        if (root / rel).is_file():
            paths.append(rel)
    return paths


def sha256_file(path):
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        while True:
            chunk = handle.read(65536)
            if not chunk:
                break
            digest.update(chunk)
    return digest.hexdigest()


def mapped_runtime_candidates(char, topic, stem):
    topic_lower = topic.lower()
    if topic_lower == "powers":
        return [
            f"Sts2WD/images/powers/{stem}.png",
            f"Sts2WD/images/powers/big/{stem}.png",
        ]
    if topic_lower == "relics":
        return [
            f"Sts2WD/images/relics/{stem}.png",
            f"Sts2WD/images/relics/big/{stem}.png",
        ]
    if topic_lower == "cards":
        return [f"Sts2WD/images/card_portraits/{char}/{stem}.png"]
    if topic_lower == "ui":
        return [f"Sts2WD/images/charui/{char}/{stem}.png"]
    if topic_lower == "ancients":
        return [f"Sts2WD/images/ancients/{stem}.png"]
    return [
        f"Sts2WD/images/{topic}/{stem}.png",
        f"Sts2WD/images/{topic}/big/{stem}.png",
        f"Sts2WD/images/{topic}/{char}/{stem}.png",
    ]


def check_repository(root):
    tracked = list_tracked_files(root)
    docs_files = [p for p in tracked if p.startswith("docs/")]
    runtime_files = [p for p in tracked if p.startswith("Sts2WD/")]

    runtime_by_hash = {}
    runtime_by_lower = {}
    for rel in runtime_files:
        runtime_by_lower[rel.lower()] = rel
        full = root / rel
        if full.stat().st_size > 0:
            runtime_by_hash.setdefault(sha256_file(full), []).append(rel)

    issues = set()
    docs_images_by_hash = {}
    topic_ref_subtrees = {}

    for rel in docs_files:
        full = root / rel
        size = full.stat().st_size
        posix = PurePosixPath(rel)
        suffix_lower = posix.suffix.lower()

        if size > 0:
            digest = sha256_file(full)
            if digest in runtime_by_hash:
                matches = ", ".join(sorted(runtime_by_hash[digest]))
                issues.add((rel, f"byte-identical to tracked runtime asset ({matches})"))
            if suffix_lower in IMAGE_EXTS:
                docs_images_by_hash.setdefault(digest, []).append(rel)

        parts = posix.parts
        if len(parts) >= 3 and parts[0] == "docs" and parts[1] == "characters":
            dir_segments = parts[2:-1]
            filename = parts[-1]
            stem = posix.stem
            base_stem = filename.split(".", 1)[0]

            if any(seg.lower().endswith("-candidates") for seg in dir_segments):
                issues.add((rel, "prohibited candidate directory under docs/characters/"))
            if stem.lower().endswith("-candidate") or base_stem.lower().endswith("-candidate"):
                issues.add((rel, "prohibited candidate filename under docs/characters/"))
            if suffix_lower in IMAGE_EXTS and OVERVIEW_PATTERN.search(stem):
                issues.add((rel, "prohibited overview image under docs/characters/"))

            if len(parts) >= 6 and parts[3] == "refs":
                char = parts[2]
                topic = parts[4]
                subtree_key = f"docs/characters/{char}/refs/{topic}"
                entry = topic_ref_subtrees.setdefault(subtree_key, {"md": [], "non_md": 0})
                if suffix_lower == ".md":
                    entry["md"].append(rel)
                else:
                    entry["non_md"] += 1
                    if suffix_lower in IMAGE_EXTS:
                        matched = []
                        for candidate in mapped_runtime_candidates(char, topic, stem):
                            hit = runtime_by_lower.get(candidate.lower())
                            if hit is not None:
                                matched.append(hit)
                        if matched:
                            matches = ", ".join(sorted(set(matched)))
                            issues.add((rel, f"topic rough already applied in runtime ({matches})"))

    for paths in docs_images_by_hash.values():
        if len(paths) > 1:
            ordered = sorted(paths)
            canonical = ordered[0]
            for duplicate in ordered[1:]:
                issues.add((duplicate, f"duplicate tracked image byte-identical to {canonical}"))

    for subtree_key, entry in sorted(topic_ref_subtrees.items()):
        if entry["md"] and entry["non_md"] == 0:
            for md_path in sorted(entry["md"]):
                issues.add(
                    (md_path, f"orphaned Markdown file in {subtree_key}/ with zero pending non-Markdown references")
                )

    return sorted(issues), len(docs_files)


def main():
    configure_utf8_stdio()
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=ROOT, help="repository root directory")
    args = parser.parse_args()
    root = args.root.resolve()

    try:
        issues, docs_count = check_repository(root)
    except (OSError, RuntimeError, UnicodeDecodeError) as error:
        print(f"Docs artifact check failed: {error}", file=sys.stderr)
        return 1

    if issues:
        for rel, reason in issues:
            print(f"{rel}: {reason}", file=sys.stderr)
        print(f"Docs artifact check failed: {len(issues)} violation(s).", file=sys.stderr)
        return 1

    print(f"Docs artifact check passed ({docs_count} tracked docs files).")
    return 0


if __name__ == "__main__":
    sys.exit(main())
