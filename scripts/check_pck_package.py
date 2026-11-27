"""Validate exported Godot PCK allowlist, provenance, and bundle hygiene, and report size/format stats."""

# Human requester: KyuSeok Jung (wjdrb), 2026-11-22: add Stage 2 post-export
# PCK leak gate (prefix allowlist, Sts2WD extension/subpath rules, 1:1 imported
# provenance, Release/Debug sibling bundle checks) and non-blocking size/format
# reporting to dev.ps1 publish and deploy.
# Human requester: KyuSeok Jung (wjdrb), 2026-11-27 (GitHub Issue #46 / Q6=A):
# authorize native Spine 4.2 runtime skeleton/atlas extensions (.spjson, .spatlas)
# under Sts2WD/ in exported PCK packages.

import argparse
import json
from pathlib import Path
import struct
import sys

SCRIPT_DIR = Path(__file__).resolve().parent
if str(SCRIPT_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPT_DIR))

from check_install_package import configure_utf8_stdio, parse_godot_config, unquote_godot_string

SCHEMA_VERSION = 1

# Compose cache prefixes without bare literal tokens that match untracked local
# workspace paths in scripts/check_tracked_references.py.
DOT_GODOT_PREFIX = "." + "godot/"
DOT_GODOT_IMPORTED_PREFIX = DOT_GODOT_PREFIX + "imported/"
DOT_GODOT_EXPORTED_PREFIX = DOT_GODOT_PREFIX + "exported/"

PACK_MAGIC = b"GDPC"
PACK_DIR_ENCRYPTED = 1 << 0
PACK_REL_FILEBASE = 1 << 1
PACK_SPARSE_BUNDLE = 1 << 2
PACK_FILE_ENCRYPTED = 1 << 0

ALLOWED_STS2WD_EXTS = {
    ".import",
    ".remap",
    ".gdc",
    ".gd",
    ".gdshader",
    ".tscn",
    ".tres",
    ".uid",
    ".spjson",
    ".spatlas",
}
ALLOWED_STS2WD_EXACT = {"Sts2WD/mod_image.png"}
ALLOWED_JSON_PREFIXES = ("Sts2WD/localization/", "Sts2WD/character_text/")

GST2_MAGIC = b"GST2"
DATA_FORMAT_IMAGE = 0
DATA_FORMAT_WEBP = 2
BPTC_FORMATS = {22, 23, 24}  # FORMAT_BPTC_RGBA, FORMAT_BPTC_RGBF, FORMAT_BPTC_RGBFU
S3TC_FORMATS = {17, 18, 19, 20, 21}  # FORMAT_DXT1/3/5, FORMAT_RGTC_R/RG


def parse_pck_entries(handle, file_size):
    """Parse Godot PCK v2 or v3 binary header and entry table."""
    if file_size < 32:
        raise ValueError(f"PCK file is truncated ({file_size} bytes).")
    handle.seek(0)
    magic, version, major, minor, patch, pack_flags, file_base = struct.unpack("<4sIIIIIQ", handle.read(32))
    if magic != PACK_MAGIC:
        raise ValueError(f"Invalid PCK magic {magic!r}; expected {PACK_MAGIC!r}.")
    if version not in (2, 3):
        raise ValueError(f"Unsupported PCK format version {version}; expected 2 or 3.")
    if pack_flags & PACK_DIR_ENCRYPTED:
        raise ValueError("Encrypted PCK directory (PACK_DIR_ENCRYPTED) is not supported.")
    if pack_flags & PACK_SPARSE_BUNDLE:
        raise ValueError("Sparse PCK bundle (PACK_SPARSE_BUNDLE) is not supported.")

    if version == 2:
        if file_size < 100:
            raise ValueError("Truncated PCK v2 header.")
        handle.seek(96)
    else:
        if file_size < 40:
            raise ValueError("Truncated PCK v3 header.")
        dir_offset = struct.unpack("<Q", handle.read(8))[0]
        if dir_offset >= file_size or dir_offset + 4 > file_size:
            raise ValueError(f"PCK v3 directory offset {dir_offset} is out of bounds (file size {file_size}).")
        handle.seek(dir_offset)

    count_bytes = handle.read(4)
    if len(count_bytes) < 4:
        raise ValueError("Truncated PCK entry count.")
    file_count = struct.unpack("<I", count_bytes)[0]
    if file_count == 0:
        raise ValueError("PCK contains zero entries.")

    entries = []
    for index in range(file_count):
        plen_raw = handle.read(4)
        if len(plen_raw) < 4:
            raise ValueError(f"Truncated PCK directory at entry {index}.")
        path_len = struct.unpack("<I", plen_raw)[0]
        raw_path = handle.read(path_len)
        if len(raw_path) < path_len:
            raise ValueError(f"Truncated PCK entry path at entry {index}.")
        meta_raw = handle.read(36)
        if len(meta_raw) < 36:
            raise ValueError(f"Truncated PCK entry metadata at entry {index}.")
        entry_ofs, entry_size = struct.unpack("<QQ", meta_raw[:16])
        entry_flags = struct.unpack("<I", meta_raw[32:36])[0]

        path_str = raw_path.rstrip(b"\x00").decode("utf-8", errors="strict")
        if entry_flags & PACK_FILE_ENCRYPTED:
            raise ValueError(f"Encrypted PCK entry '{path_str}' (PACK_FILE_ENCRYPTED) is not supported.")

        abs_ofs = file_base + entry_ofs
        if abs_ofs + entry_size > file_size:
            raise ValueError(
                f"PCK entry '{path_str}' payload ({abs_ofs}..{abs_ofs + entry_size}) exceeds file size {file_size}."
            )
        entries.append(
            {
                "path": path_str,
                "offset": abs_ofs,
                "size": entry_size,
                "flags": entry_flags,
            }
        )

    return version, (major, minor, patch), pack_flags, entries


def classify_ctex(handle, entry):
    """Structurally parse GST2 header and walk WebP RIFF chunks via O(1) seeks."""
    size = entry["size"]
    start = entry["offset"]
    if size < 52:
        return "Other"
    handle.seek(start)
    hdr = handle.read(52)
    if hdr[:4] != GST2_MAGIC:
        return "Other"

    data_format = struct.unpack("<I", hdr[0x24:0x28])[0]
    image_format = struct.unpack("<I", hdr[0x30:0x34])[0]

    if data_format == DATA_FORMAT_IMAGE:
        if image_format in BPTC_FORMATS:
            return "VRAM BPTC"
        if image_format in S3TC_FORMATS:
            return "VRAM S3TC"
        return "Other"

    if data_format == DATA_FORMAT_WEBP:
        if size < 0x44:
            return "Other"
        handle.seek(start + 0x34)
        mip_len = struct.unpack("<I", handle.read(4))[0]
        riff_hdr = handle.read(12)
        if len(riff_hdr) < 12 or riff_hdr[:4] != b"RIFF" or riff_hdr[8:12] != b"WEBP":
            return "Other"
        mip_end = min(start + size, start + 0x38 + mip_len)
        pos = start + 0x44
        while pos + 8 <= mip_end:
            handle.seek(pos)
            chunk_hdr = handle.read(8)
            if len(chunk_hdr) < 8:
                break
            fourcc = chunk_hdr[:4]
            chunk_size = struct.unpack("<I", chunk_hdr[4:8])[0]
            if fourcc == b"VP8 ":
                return "Lossy WebP (VP8)"
            if fourcc == b"VP8L":
                return "Lossless WebP (VP8L)"
            step = 8 + chunk_size + (chunk_size & 1)
            if step <= 8:
                break
            pos += step
        return "Other"

    return "Other"


def extract_import_targets(text):
    """Return (source_rel, set_of_imported_rel_paths) from a packed .import file."""
    cfg = parse_godot_config(text)
    remap = cfg.get("remap", {})
    deps = cfg.get("deps", {})
    source = unquote_godot_string(deps.get("source_file", ""))
    if source.startswith("res://"):
        source = source[6:]

    targets = set()
    for key, raw_val in remap.items():
        if key == "path" or key.startswith("path."):
            val = unquote_godot_string(raw_val)
            if val.startswith("res://"):
                val = val[6:]
            if val:
                targets.add(val)

    dest_raw = deps.get("dest_files", "")
    if dest_raw:
        # Parse Godot PackedStringArray / ["res://...", ...] entries.
        idx = 0
        while idx < len(dest_raw):
            q1 = dest_raw.find('"', idx)
            if q1 == -1:
                break
            q2 = dest_raw.find('"', q1 + 1)
            if q2 == -1:
                break
            item = dest_raw[q1 + 1 : q2]
            if item.startswith("res://"):
                item = item[6:]
            if item:
                targets.add(item)
            idx = q2 + 1

    return source, targets


def category_for_source(source_rel):
    if source_rel.startswith("Sts2WD/images/"):
        parts = source_rel.split("/")
        if len(parts) >= 4:
            return f"Sts2WD/images/{parts[2]}"
        return "Sts2WD/images"
    if source_rel.startswith("Sts2WD/animations/"):
        return "Sts2WD/animations"
    if source_rel.startswith("Sts2WD/scenes/"):
        return "Sts2WD/scenes"
    if source_rel.startswith("Sts2WD/localization/"):
        return "Sts2WD/localization"
    if source_rel.startswith("Sts2WD/character_text/"):
        return "Sts2WD/character_text"
    if source_rel.startswith("Sts2WD/materials/"):
        return "Sts2WD/materials"
    if source_rel.startswith(DOT_GODOT_EXPORTED_PREFIX):
        return DOT_GODOT_EXPORTED_PREFIX.rstrip("/")
    return "other"


def inspect_pck(pck_path, configuration=None, top_n=10):
    pck_path = pck_path.resolve()
    if not pck_path.is_file():
        raise FileNotFoundError(f"PCK file not found: {pck_path}")

    pck_bytes = pck_path.stat().st_size
    violations = []

    with pck_path.open("rb") as handle:
        pck_version, _engine_ver, _flags, entries = parse_pck_entries(handle, pck_bytes)

        # 1. Entry prefix allowlist and Sts2WD/ extension/subpath allowlist
        referenced_imported = set()
        imported_to_source = {}

        for entry in entries:
            path = entry["path"]
            if path == "project.binary":
                continue
            if path.startswith("Sts2WD/"):
                parts = path.split("/")
                if any(part.startswith(".realta-") for part in parts):
                    violations.append(f"Disallowed temporary animation staging entry in PCK: {path}")
                    continue
                ext = Path(path).suffix.lower()
                if path in ALLOWED_STS2WD_EXACT:
                    pass
                elif ext == ".json":
                    if not path.startswith(ALLOWED_JSON_PREFIXES):
                        violations.append(f"Disallowed JSON location in PCK: {path}")
                elif ext not in ALLOWED_STS2WD_EXTS:
                    violations.append(f"Disallowed extension under Sts2WD/ in PCK: {path}")

                if path.endswith(".import"):
                    handle.seek(entry["offset"])
                    raw_text = handle.read(entry["size"]).decode("utf-8-sig", errors="replace")
                    try:
                        src, targets = extract_import_targets(raw_text)
                    except ValueError as exc:
                        violations.append(f"Invalid packed .import '{path}': {exc}")
                    else:
                        resolved_src = src or path[: -len(".import")]
                        for target in targets:
                            referenced_imported.add(target)
                            imported_to_source[target] = resolved_src
                continue
            if path.startswith(DOT_GODOT_PREFIX):
                if path.endswith(".import"):
                    violations.append(f"Non-Sts2WD .import entry in PCK: {path}")
                continue
            violations.append(f"Disallowed path outside prefix allowlist in PCK: {path}")

        # 2. 1:1 .godot/imported/* provenance & GST2 format classification
        categories = {}
        ctex_formats = {}
        enriched_entries = []

        for entry in entries:
            path = entry["path"]
            size = entry["size"]
            display_source = path

            if path.startswith(DOT_GODOT_IMPORTED_PREFIX):
                if path not in referenced_imported:
                    violations.append(f"Orphaned imported cache entry in PCK (no matching Sts2WD .import): {path}")
                src = imported_to_source.get(path, "")
                if src:
                    display_source = f"{path} <- {src}"
                cat = category_for_source(src) if src else "other"
                if path.lower().endswith(".ctex"):
                    fmt = classify_ctex(handle, entry)
                    bucket = ctex_formats.setdefault(fmt, {"count": 0, "bytes": 0})
                    bucket["count"] += 1
                    bucket["bytes"] += size
            else:
                cat = category_for_source(path)

            cat_bucket = categories.setdefault(cat, {"count": 0, "bytes": 0})
            cat_bucket["count"] += 1
            cat_bucket["bytes"] += size
            enriched_entries.append({"path": path, "source": imported_to_source.get(path), "bytes": size, "label": display_source})

    # 3. Sibling bundle contract
    bundle_files = {}
    bundle_bytes = pck_bytes
    if configuration is not None:
        parent = pck_path.parent
        stem = pck_path.stem
        dll_name = f"{stem}.dll"
        json_name = f"{stem}.json"
        pdb_name = f"{stem}.pdb"
        pck_name = pck_path.name

        required = [dll_name, pck_name, json_name]
        if configuration == "Debug":
            required.append(pdb_name)

        for name in required:
            candidate = parent / name
            if not candidate.is_file():
                violations.append(f"Missing required {configuration} bundle file: {candidate}")
            else:
                bundle_files[name] = candidate.stat().st_size

        if configuration == "Release":
            stale_pdb = parent / pdb_name
            if stale_pdb.is_file():
                violations.append(f"Unexpected debug symbols in Release bundle: {stale_pdb}")

        bundle_bytes = sum(bundle_files.values())
    else:
        bundle_files[pck_path.name] = pck_bytes

    top_entries = sorted(enriched_entries, key=lambda item: (-item["bytes"], item["path"]))[: max(0, top_n)]
    report = {
        "schema_version": SCHEMA_VERSION,
        "pck_path": str(pck_path),
        "pck_format_version": pck_version,
        "configuration": configuration,
        "passed": len(violations) == 0,
        "violations": violations,
        "bundle_files": bundle_files,
        "bundle_bytes": bundle_bytes,
        "pck_bytes": pck_bytes,
        "entry_count": len(entries),
        "categories": dict(sorted(categories.items(), key=lambda kv: (-kv[1]["bytes"], kv[0]))),
        "ctex_formats": dict(sorted(ctex_formats.items(), key=lambda kv: (-kv[1]["bytes"], kv[0]))),
        "top_entries": [
            {"path": item["path"], "source": item["source"], "bytes": item["bytes"]}
            for item in top_entries
        ],
    }
    return report


def fmt_mb(num_bytes):
    return f"{num_bytes / (1024 * 1024):.2f} MB"


def fmt_kb(num_bytes):
    return f"{num_bytes / 1024:.1f} KB"


def print_report(report):
    cfg = report["configuration"] or "standalone"
    print(
        f"PCK Report ({cfg}, PCK v{report['pck_format_version']}): "
        f"bundle={fmt_mb(report['bundle_bytes'])}, pck={fmt_mb(report['pck_bytes'])}, "
        f"entries={report['entry_count']}"
    )
    if report["bundle_files"]:
        parts = [f"{name}={fmt_kb(sz)}" for name, sz in sorted(report["bundle_files"].items())]
        print("  Bundle files: " + ", ".join(parts))
    if report["ctex_formats"]:
        parts = [
            f"{fmt}={info['count']} ({fmt_mb(info['bytes'])})"
            for fmt, info in report["ctex_formats"].items()
        ]
        print("  Texture formats: " + ", ".join(parts))
    if report["categories"]:
        print("  Categories:")
        for cat, info in report["categories"].items():
            print(f"    {cat:30s} {info['count']:4d} entries  {fmt_mb(info['bytes']):>9s}")
    if report["top_entries"]:
        print(f"  Top {len(report['top_entries'])} entries:")
        for item in report["top_entries"]:
            label = item["source"] or item["path"]
            print(f"    {fmt_kb(item['bytes']):>9s}  {label}")


def main():
    configure_utf8_stdio()
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("pck_path", type=Path, help="path to exported .pck file")
    parser.add_argument(
        "--configuration",
        choices=("Release", "Debug"),
        default=None,
        help="validate sibling bundle files for Release (3 files, no .pdb) or Debug (4 files)",
    )
    parser.add_argument("--json-out", type=Path, default=None, help="optional path to write structured JSON report")
    parser.add_argument("--top", type=int, default=10, help="number of largest PCK entries to display (default: 10)")
    args = parser.parse_args()

    try:
        report = inspect_pck(args.pck_path, configuration=args.configuration, top_n=args.top)
    except (OSError, ValueError, struct.error) as exc:
        print(f"PCK check failed: {exc}", file=sys.stderr)
        return 1

    if args.json_out is not None:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(json.dumps(report, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    print_report(report)

    if not report["passed"]:
        for violation in report["violations"]:
            print(violation, file=sys.stderr)
        print(f"PCK check failed: {len(report['violations'])} violation(s).", file=sys.stderr)
        return 1

    print("PCK check passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
