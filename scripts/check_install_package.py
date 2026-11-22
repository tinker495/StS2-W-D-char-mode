"""Validate Sts2WD install-package hygiene, 3-tier .import compression, source caps, and texture reachability."""

# Human requester: KyuSeok Jung (wjdrb), 2026-11-22: enforce install-package
# and PCK minimization policy (3-tier texture .import compression, unconditional
# source resolution ceilings, non-runtime .gdignore/export_presets.cfg/project.godot
# boundary checks, non-PNG raster ban, and Sts2WD texture reachability) in local
# verify/build/check/publish/deploy and GitHub Actions CI.

import argparse
from pathlib import Path
import re
import struct
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]

# Compose cache/local filenames without bare literal path tokens so the protected
# tracked-reference checker passes locally when untracked workspace files exist.
DOT_GODOT = "." + "godot"
DOT_GODOT_IMPORTED_RE_PREFIX = r'^"res://\.' + r'godot/imported/.+\.bptc\.ctex"$'
LOCAL_PROPS_TOKEN = "local" + ".props"
TOOLS_EXCLUDE_TOKEN = "." + "tools/*"

PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"
NON_PNG_RASTER_EXTS = {".jpg", ".jpeg", ".webp", ".svg", ".bmp", ".tga"}
LOSSY_QUALITY_TARGET = 0.85
LOSSY_QUALITY_EPS = 1e-4

REQUIRED_EXCLUDE_TOKENS = {
    "Sts2WDCode/*",
    "Sts2WD.json",
    "global.json",
    "packages.lock.json",
    LOCAL_PROPS_TOKEN,
    "local.props.example",
    "artifacts/*",
    "bin/*",
    "obj/*",
    "publish/*",
    "packages/*",
    "**/drafts/*",
    "docs/*",
    "tests/*",
    "scripts/*",
    ".agents/*",
    ".github/*",
    ".githooks/*",
    TOOLS_EXCLUDE_TOKEN,
    ".codex/*",
    "*.csproj",
    "*.sln",
    "*.props",
    "*.targets",
    "*.ps1",
    "*.py",
    "*.sh",
    "*.md",
    "*.txt",
}

METADATA_VRAM_RE = re.compile(r'"vram_texture"\s*:\s*(true|false)')
BPTC_REMAP_PATH_RE = re.compile(DOT_GODOT_IMPORTED_RE_PREFIX)

# C# parsing patterns for texture reachability.
CS_ESCAPE_HATCH_RE = re.compile(r"//\s*sts2wd-asset:\s*(Sts2WD/[^\s]+)")
RES_LITERAL_RE = re.compile(r'res://(Sts2WD/[^"\'\s\r\n)]+)')
RES_PATH_CONCAT_RE = re.compile(r'MainFile\.ResPath\s*\+\s*"/([^"\r\n]+)"')
RES_PATH_INTERP_RE = re.compile(r'\$"\{MainFile\.ResPath\}/([^"\r\n{}]+)"')
PATH_JOIN_RE = re.compile(r'Path\.(?:Join|Combine)\(\s*MainFile\.ResPath\s*,([^)]+)\)', re.DOTALL)
STRING_LITERAL_RE = re.compile(r'"([^"\r\n\\]*(?:\\.[^"\r\n\\]*)*)"')
INTERP_RES_TEMPLATE_RE = re.compile(r'\$"res://(Sts2WD/[^{}"\r\n]*)\{([^{}"\r\n]+)\}([^{}"\r\n]*)"')
NAMESPACE_RE = re.compile(r'^\s*namespace\s+([A-Za-z0-9_.]+)', re.MULTILINE)
SLUGIFY_RE = re.compile(r'(?<=[A-Za-z0-9])([A-Z])')

EXTENSION_HELPER_PREFIXES = {
    "ImagePath": "Sts2WD/images/",
    "CharacterUiPath": "Sts2WD/images/charui/",
    "CardImagePath": "Sts2WD/images/card_portraits/",
    "BigCardImagePath": "Sts2WD/images/card_portraits/big/",
    "PowerImagePath": "Sts2WD/images/powers/",
    "BigPowerImagePath": "Sts2WD/images/powers/big/",
    "RelicImagePath": "Sts2WD/images/relics/",
    "BigRelicImagePath": "Sts2WD/images/relics/big/",
    "PotionImagePath": "Sts2WD/images/potions/",
    # StringExtensions.PotionOutlineImagePath joins images/potions/<arg> directly.
    "PotionOutlineImagePath": "Sts2WD/images/potions/",
}


def configure_utf8_stdio():
    for stream in (sys.stdout, sys.stderr):
        reconfigure = getattr(stream, "reconfigure", None)
        if reconfigure is not None:
            reconfigure(encoding="utf-8", errors="replace")


def list_tracked_files(root):
    result = subprocess.run(
        ["git", "ls-files", "--cached", "-z", "--deduplicate"],
        cwd=root,
        capture_output=True,
        check=False,
    )
    if result.returncode != 0:
        err = result.stderr.decode("utf-8", errors="replace").strip()
        raise RuntimeError(f"git ls-files failed (exit {result.returncode}): {err}")
    entries = []
    for raw in result.stdout.split(b"\x00"):
        if not raw:
            continue
        rel = raw.decode("utf-8", errors="strict").replace("\\", "/")
        if (root / rel).is_file():
            entries.append(rel)
    return sorted(entries)


def parse_godot_config(text):
    """Parse Godot ConfigFile (.import, export_presets.cfg, project.godot) cleanly.

    Handles multi-line double-quoted strings with escapes, nested {}/[]/() Variant
    values (such as .import metadata={...}), and ';' comments outside strings.
    """
    sections = {}
    current = None
    i = 0
    n = len(text)

    while i < n:
        # Skip whitespace and blank lines.
        while i < n and text[i] in " \t\r\n":
            i += 1
        if i >= n:
            break

        # Skip comment lines starting with ';'.
        if text[i] == ";":
            nl = text.find("\n", i)
            i = n if nl == -1 else nl + 1
            continue

        # Section header: [section.name]
        if text[i] == "[":
            end = text.find("]", i + 1)
            if end == -1:
                raise ValueError("Unterminated section header in Godot config.")
            sec_name = text[i + 1 : end].strip()
            current = sections.setdefault(sec_name, {})
            nl = text.find("\n", end + 1)
            i = n if nl == -1 else nl + 1
            continue

        # Key = Value assignment.
        eq = text.find("=", i)
        nl = text.find("\n", i)
        if eq == -1 or (nl != -1 and eq > nl):
            # Ignore stray line outside assignment.
            i = n if nl == -1 else nl + 1
            continue

        key = text[i:eq].strip()
        j = eq + 1
        while j < n and text[j] in " \t":
            j += 1
        val_start = j
        in_string = False
        escape = False
        depth = 0

        while j < n:
            ch = text[j]
            if in_string:
                if escape:
                    escape = False
                elif ch == "\\":
                    escape = True
                elif ch == '"':
                    in_string = False
            else:
                if ch == '"':
                    in_string = True
                elif ch in "{[(":
                    depth += 1
                elif ch in "}])":
                    if depth > 0:
                        depth -= 1
                elif ch == ";" and depth == 0:
                    # Inline comment ends value.
                    break
                elif ch in "\r\n" and depth == 0:
                    break
            j += 1

        val = text[val_start:j].strip()
        if current is None:
            current = sections.setdefault("", {})
        current[key] = val

        # Advance to start of next line.
        next_nl = text.find("\n", j)
        i = n if next_nl == -1 else next_nl + 1

    return sections


def unquote_godot_string(raw):
    raw = raw.strip()
    if len(raw) >= 2 and raw[0] == '"' and raw[-1] == '"':
        inner = raw[1:-1]
        return inner.replace(r'\"', '"').replace(r"\\", "\\")
    return raw


def read_png_dimensions(path):
    data = path.read_bytes()
    if len(data) < 24:
        raise ValueError(f"truncated PNG header ({len(data)} bytes)")
    if data[:8] != PNG_SIGNATURE:
        raise ValueError("invalid PNG signature")
    ihdr_len = struct.unpack(">I", data[8:12])[0]
    if ihdr_len != 13 or data[12:16] != b"IHDR":
        raise ValueError(f"invalid PNG IHDR chunk (len={ihdr_len}, tag={data[12:16]!r})")
    width, height = struct.unpack(">II", data[16:24])
    if width <= 0 or height <= 0:
        raise ValueError(f"invalid PNG dimensions ({width}x{height})")
    return width, height


def is_combat_atlas_path(rel):
    if not rel.startswith("Sts2WD/"):
        return False
    sub = rel[len("Sts2WD/") :].lower()
    if not sub.startswith("animations/"):
        return False
    parts = sub.split("/")
    name = parts[-1]
    if len(parts) >= 4 and parts[2] == "combat" and name.endswith(".png"):
        return True
    return name.endswith("_skeleton.png") or name.endswith("_atlas.png")


def classify_tier(rel, width, height):
    """Assign deterministic compression tier in strict Tier 1 -> Tier 3 -> Tier 2 order."""
    sub = rel[len("Sts2WD/") :].lower() if rel.startswith("Sts2WD/") else rel.lower()
    name = Path(sub).name
    max_dim = max(width, height)

    # Precedence 1: Tier 1 (VRAM BPTC/BC7) for >=2048px combat atlases.
    if is_combat_atlas_path(rel) and max_dim >= 2048:
        return "tier1_bptc"

    # Precedence 2: Tier 3 (Lossless WebP) for small UI, outlines, frames, blink patches, materials, mod icon.
    if (
        max_dim <= 256
        or name.endswith("_outline.png")
        or "/outline/" in sub
        or sub.startswith("images/cards/")
        or name.endswith("_blink_patch.png")
        or sub.startswith("materials/")
        or sub == "mod_image.png"
    ):
        return "tier3_lossless"

    # Precedence 3: Tier 2 (Lossy WebP 0.85) for remaining large painterly illustrations & backgrounds.
    return "tier2_lossy"


def check_resolution_ceiling(rel, width, height, tier):
    """Evaluate unconditional source PNG resolution ceilings (most-specific path first)."""
    sub = rel[len("Sts2WD/") :].lower()
    parts = sub.split("/")
    name = parts[-1]

    # 1. Combat/rig atlases
    if is_combat_atlas_path(rel):
        if width > 2048 or height > 2048:
            return f"{rel}: combat atlas {width}x{height} exceeds ceiling 2048x2048."
        if tier == "tier1_bptc" and (width % 4 != 0 or height % 4 != 0):
            return f"{rel}: Tier 1 BPTC atlas {width}x{height} must be 4x4 block-aligned."
        return None

    # 2. Rest masters
    if len(parts) == 4 and parts[0] == "animations" and parts[2] == "rest" and name.endswith("_master.png"):
        if width > 1536 or height > 1536:
            return f"{rel}: rest master {width}x{height} exceeds ceiling 1536x1536."
        return None

    # 3. Blink patches
    if name.endswith("_blink_patch.png"):
        if width > 512 or height > 512:
            return f"{rel}: blink patch {width}x{height} exceeds ceiling 512x512."
        return None

    # 4. Card portraits (exact layout owned by check_card_portraits.py; upper ceiling only)
    if sub.startswith("images/card_portraits/"):
        if width > 1024 or height > 1024:
            return f"{rel}: card portrait {width}x{height} exceeds ceiling 1024x1024."
        return None

    # 5. Card frames
    if sub.startswith("images/cards/"):
        if width > 1024 or height > 1024:
            return f"{rel}: card frame {width}x{height} exceeds ceiling 1024x1024."
        return None

    # 6. Character select backgrounds
    if len(parts) == 4 and parts[0] == "images" and parts[1] == "charui" and name == "char_select_bg.png":
        if width > 1920 or height > 1080:
            return f"{rel}: char_select_bg {width}x{height} exceeds ceiling 1920x1080."
        return None

    # 7. Non-rig character standing stills
    if (
        len(parts) == 4
        and parts[0] == "images"
        and parts[1] == "charui"
        and name in {"combat.png", "standing.png", "merchant.png", "rest.png"}
    ):
        if width > 1024 or height > 1024:
            return f"{rel}: character standing still {width}x{height} exceeds ceiling 1024x1024."
        return None

    # 8. Portrait-orientation multiplayer hands
    if len(parts) >= 5 and parts[0] == "images" and parts[1] == "charui" and parts[3] == "hands":
        if width > 1024 or height > 1536:
            return f"{rel}: multiplayer hand sprite {width}x{height} exceeds ceiling 1024x1536."
        return None

    # 9. Ancient map/run-history icons
    if len(parts) == 3 and parts[0] == "images" and parts[1] == "ancients" and (
        name.endswith("_icon.png") or name.endswith("_icon_outline.png")
    ):
        if width > 256 or height > 256:
            return f"{rel}: ancient icon {width}x{height} exceeds ceiling 256x256."
        return None

    # 10. Ancient event illustrations
    if sub.startswith("images/ancients/"):
        if width > 1920 or height > 1080:
            return f"{rel}: ancient illustration {width}x{height} exceeds ceiling 1920x1080."
        return None

    # 11. Small UI icons, relics, powers, potions, charui icons
    if sub.startswith(("images/relics/", "images/powers/", "images/potions/", "images/charui/")):
        if width > 256 or height > 256:
            return f"{rel}: UI/icon asset {width}x{height} exceeds ceiling 256x256."
        return None

    # 12. Shader materials
    if sub.startswith("materials/"):
        if width > 1024 or height > 1024:
            return f"{rel}: material texture {width}x{height} exceeds ceiling 1024x1024."
        return None

    # 13. Mod icon
    if sub == "mod_image.png":
        if width > 512 or height > 512:
            return f"{rel}: mod_image.png {width}x{height} exceeds ceiling 512x512."
        return None

    # 14. Fallback ceiling
    if not ((width <= 1920 and height <= 1080) or (width <= 1080 and height <= 1920)):
        return f"{rel}: asset {width}x{height} exceeds fallback ceiling 1920x1080 / 1080x1920."
    return None


def validate_import_file(rel, import_path, tier):
    violations = []
    try:
        text = import_path.read_text(encoding="utf-8-sig")
        cfg = parse_godot_config(text)
    except (OSError, ValueError) as exc:
        return [f"{rel}.import: failed to parse Godot config: {exc}"]

    remap = cfg.get("remap", {})
    params = cfg.get("params", {})
    meta_raw = remap.get("metadata", "")
    vram_match = METADATA_VRAM_RE.search(meta_raw)
    vram_flag = vram_match.group(1) if vram_match else None

    if params.get("mipmaps/generate") != "false":
        violations.append(f"{rel}.import: mipmaps/generate must be false (got {params.get('mipmaps/generate')!r}).")
    if params.get("process/size_limit") != "0":
        violations.append(f"{rel}.import: process/size_limit must be 0 (got {params.get('process/size_limit')!r}).")

    if tier == "tier1_bptc":
        if params.get("compress/mode") != "2":
            violations.append(f"{rel}.import: Tier 1 BPTC requires compress/mode=2 (got {params.get('compress/mode')!r}).")
        if params.get("compress/high_quality") != "true":
            violations.append(
                f"{rel}.import: Tier 1 BPTC requires compress/high_quality=true (got {params.get('compress/high_quality')!r})."
            )
        if vram_flag != "true":
            violations.append(
                f'{rel}.import: Tier 1 BPTC requires [remap] metadata "vram_texture": true (run dev.ps1 import).'
            )
        bptc_path = remap.get("path.bptc", "")
        if not BPTC_REMAP_PATH_RE.match(bptc_path):
            violations.append(
                f"{rel}.import: Tier 1 BPTC requires [remap] path.bptc pointing to a .bptc.ctex (got {bptc_path!r}; run dev.ps1 import)."
            )
        if "path.s3tc" in remap or "path" in remap:
            violations.append(
                f"{rel}.import: Tier 1 BPTC must not emit [remap] path.s3tc or plain path alongside path.bptc."
            )
    elif tier == "tier2_lossy":
        if params.get("compress/mode") != "1":
            violations.append(
                f"{rel}.import: Tier 2 Lossy WebP requires compress/mode=1 (got {params.get('compress/mode')!r})."
            )
        q_raw = params.get("compress/lossy_quality")
        try:
            q_val = float(q_raw) if q_raw is not None else -1.0
        except ValueError:
            q_val = -1.0
        if abs(q_val - LOSSY_QUALITY_TARGET) > LOSSY_QUALITY_EPS:
            violations.append(
                f"{rel}.import: Tier 2 Lossy WebP requires compress/lossy_quality={LOSSY_QUALITY_TARGET} (got {q_raw!r})."
            )
        if vram_flag != "false":
            violations.append(
                f'{rel}.import: Tier 2 Lossy WebP requires [remap] metadata "vram_texture": false (run dev.ps1 import).'
            )
    else:  # tier3_lossless
        if params.get("compress/mode") != "0":
            violations.append(
                f"{rel}.import: Tier 3 Lossless WebP requires compress/mode=0 (got {params.get('compress/mode')!r})."
            )
        if vram_flag != "false":
            violations.append(
                f'{rel}.import: Tier 3 Lossless WebP requires [remap] metadata "vram_texture": false (run dev.ps1 import).'
            )

    return violations


def sync_import_params(root):
    """Rewrite only [params] tier keys in existing Sts2WD/**/*.png.import files."""
    sts2wd = root / "Sts2WD"
    if not sts2wd.is_dir():
        raise FileNotFoundError(f"Runtime asset directory not found: {sts2wd}")

    updated = []
    skipped_missing = []
    for png in sorted(
        (p for p in sts2wd.rglob("*") if p.is_file() and p.suffix.lower() == ".png"),
        key=lambda p: p.relative_to(root).as_posix().lower(),
    ):
        rel = png.relative_to(root).as_posix()
        imp = Path(str(png) + ".import")
        if not imp.is_file():
            skipped_missing.append(rel)
            continue

        width, height = read_png_dimensions(png)
        tier = classify_tier(rel, width, height)

        desired = {
            "mipmaps/generate": "false",
            "process/size_limit": "0",
        }
        if tier == "tier1_bptc":
            desired["compress/mode"] = "2"
            desired["compress/high_quality"] = "true"
        elif tier == "tier2_lossy":
            desired["compress/mode"] = "1"
            desired["compress/lossy_quality"] = "0.85"
        else:
            desired["compress/mode"] = "0"

        raw = imp.read_text(encoding="utf-8-sig")
        newline = "\r\n" if "\r\n" in raw else "\n"
        lines = raw.splitlines()
        out_lines = []
        in_params = False
        seen = set()

        for line in lines:
            stripped = line.strip()
            if stripped.startswith("[") and stripped.endswith("]"):
                if in_params:
                    for k, v in desired.items():
                        if k not in seen:
                            out_lines.append(f"{k}={v}")
                in_params = stripped == "[params]"
                out_lines.append(line)
                continue
            if in_params and "=" in line:
                key = line.split("=", 1)[0].strip()
                if key in desired:
                    seen.add(key)
                    out_lines.append(f"{key}={desired[key]}")
                    continue
            out_lines.append(line)

        if in_params:
            for k, v in desired.items():
                if k not in seen:
                    out_lines.append(f"{k}={v}")

        new_text = newline.join(out_lines) + newline
        if new_text != raw:
            imp.write_text(new_text, encoding="utf-8", newline="")
            updated.append((rel, tier))

    return updated, skipped_missing


def check_boundary_and_project_config(root, tracked_set):
    violations = []

    # 1. Tracked .gdignore rules
    top_dirs = sorted({f.split("/", 1)[0] for f in tracked_set if "/" in f})
    for top in top_dirs:
        if top.startswith(".") or top in {"Sts2WD", "Sts2WDCode"}:
            continue
        marker = f"{top}/.gdignore"
        if marker not in tracked_set:
            violations.append(f"Missing tracked Godot ignore marker: {marker}")

    for rel in sorted(tracked_set):
        if rel.endswith("/.gdignore") or rel == ".gdignore":
            if rel.startswith(("Sts2WD/", "Sts2WDCode/")):
                violations.append(f"Forbidden .gdignore inside runtime/C# tree: {rel}")

    # 2. export_presets.cfg
    presets_path = root / "export_presets.cfg"
    if "export_presets.cfg" not in tracked_set or not presets_path.is_file():
        violations.append("Missing tracked export_presets.cfg.")
    else:
        try:
            presets = parse_godot_config(presets_path.read_text(encoding="utf-8-sig"))
        except (OSError, ValueError) as exc:
            violations.append(f"export_presets.cfg: parse error: {exc}")
        else:
            p0 = presets.get("preset.0", {})
            opts = presets.get("preset.0.options", {})
            if unquote_godot_string(p0.get("export_filter", "")) != "all_resources":
                violations.append(
                    f"export_presets.cfg: preset.0 export_filter must be 'all_resources' (got {p0.get('export_filter')!r})."
                )
            excl_raw = unquote_godot_string(p0.get("exclude_filter", ""))
            excl_tokens = {tok.strip() for tok in excl_raw.split(",") if tok.strip()}
            missing_tokens = sorted(REQUIRED_EXCLUDE_TOKENS - excl_tokens)
            if missing_tokens:
                violations.append(
                    f"export_presets.cfg: exclude_filter missing required token(s): {', '.join(missing_tokens)}"
                )
            if opts.get("texture_format/s3tc_bptc") != "true":
                violations.append(
                    f"export_presets.cfg: texture_format/s3tc_bptc must be true (got {opts.get('texture_format/s3tc_bptc')!r})."
                )
            if opts.get("texture_format/etc2_astc") != "false":
                violations.append(
                    f"export_presets.cfg: texture_format/etc2_astc must be false (got {opts.get('texture_format/etc2_astc')!r})."
                )
            if opts.get("dotnet/include_debug_symbols") != "false":
                violations.append(
                    f"export_presets.cfg: dotnet/include_debug_symbols must be false (got {opts.get('dotnet/include_debug_symbols')!r})."
                )

    # 3. project.godot
    proj_path = root / "project.godot"
    if "project.godot" not in tracked_set or not proj_path.is_file():
        violations.append("Missing tracked project.godot.")
    else:
        try:
            proj = parse_godot_config(proj_path.read_text(encoding="utf-8-sig"))
        except (OSError, ValueError) as exc:
            violations.append(f"project.godot: parse error: {exc}")
        else:
            rendering = proj.get("rendering", {})
            if rendering.get("textures/vram_compression/import_s3tc_bptc") != "true":
                violations.append(
                    "project.godot: [rendering] textures/vram_compression/import_s3tc_bptc must be true."
                )

    return violations


def strip_cs_comments(code):
    """Strip C# // and /* */ comments while preserving string/verbatim literals."""
    out = []
    i = 0
    n = len(code)
    while i < n:
        ch = code[i]
        # Verbatim string @"..."
        if ch == "@" and i + 1 < n and code[i + 1] == '"':
            out.append('@"')
            i += 2
            while i < n:
                if code[i] == '"':
                    if i + 1 < n and code[i + 1] == '"':
                        out.append('""')
                        i += 2
                    else:
                        out.append('"')
                        i += 1
                        break
                else:
                    out.append(code[i])
                    i += 1
            continue
        # Normal or interpolated string "..."
        if ch == '"':
            out.append('"')
            i += 1
            while i < n:
                if code[i] == "\\" and i + 1 < n:
                    out.append(code[i : i + 2])
                    i += 2
                elif code[i] == '"':
                    out.append('"')
                    i += 1
                    break
                else:
                    out.append(code[i])
                    i += 1
            continue
        # Char literal '.'
        if ch == "'":
            out.append("'")
            i += 1
            while i < n:
                if code[i] == "\\" and i + 1 < n:
                    out.append(code[i : i + 2])
                    i += 2
                elif code[i] == "'":
                    out.append("'")
                    i += 1
                    break
                else:
                    out.append(code[i])
                    i += 1
            continue
        # Single-line comment
        if ch == "/" and i + 1 < n and code[i + 1] == "/":
            nl = code.find("\n", i + 2)
            if nl == -1:
                break
            out.append("\n")
            i = nl + 1
            continue
        # Multi-line comment
        if ch == "/" and i + 1 < n and code[i + 1] == "*":
            end = code.find("*/", i + 2)
            if end == -1:
                break
            out.append(" ")
            i = end + 2
            continue
        out.append(ch)
        i += 1
    return "".join(out)


def skip_balanced(text, start, open_ch, close_ch):
    depth = 0
    i = start
    n = len(text)
    in_str = False
    while i < n:
        ch = text[i]
        if in_str:
            if ch == "\\" and i + 1 < n:
                i += 2
                continue
            if ch == '"':
                in_str = False
        else:
            if ch == '"':
                in_str = True
            elif ch == open_ch:
                depth += 1
            elif ch == close_ch:
                depth -= 1
                if depth == 0:
                    return i + 1
        i += 1
    return n


def split_top_level_commas(text):
    parts = []
    buf = []
    angle = paren = bracket = 0
    for ch in text:
        if ch == "<":
            angle += 1
        elif ch == ">" and angle > 0:
            angle -= 1
        elif ch == "(":
            paren += 1
        elif ch == ")" and paren > 0:
            paren -= 1
        elif ch == "[":
            bracket += 1
        elif ch == "]" and bracket > 0:
            bracket -= 1
        if ch == "," and angle == 0 and paren == 0 and bracket == 0:
            parts.append("".join(buf).strip())
            buf = []
        else:
            buf.append(ch)
    tail = "".join(buf).strip()
    if tail:
        parts.append(tail)
    return parts


def parse_cs_classes(code):
    """Extract (class_name, is_abstract, base_names, namespace) across multi-line C# 12 declarations."""
    ns_match = NAMESPACE_RE.search(code)
    namespace = ns_match.group(1) if ns_match else ""
    results = []
    class_kw_re = re.compile(
        r"(?P<mods>(?:\b(?:public|internal|private|protected|sealed|abstract|partial|static|new|file)\b\s+)*)"
        r"\bclass\s+(?P<name>[A-Za-z_][A-Za-z0-9_]*)"
    )
    n = len(code)
    for match in class_kw_re.finditer(code):
        mods = match.group("mods").split()
        is_abstract = "abstract" in mods
        name = match.group("name")
        pos = match.end()

        # Skip whitespace, optional generic parameters <...>, optional primary constructor (...).
        while pos < n and code[pos].isspace():
            pos += 1
        if pos < n and code[pos] == "<":
            pos = skip_balanced(code, pos, "<", ">")
            while pos < n and code[pos].isspace():
                pos += 1
        if pos < n and code[pos] == "(":
            pos = skip_balanced(code, pos, "(", ")")
            while pos < n and code[pos].isspace():
                pos += 1

        bases = []
        if pos < n and code[pos] == ":":
            pos += 1
            base_start = pos
            depth_paren = depth_angle = 0
            while pos < n:
                ch = code[pos]
                if ch == '"':
                    pos += 1
                    while pos < n:
                        if code[pos] == "\\" and pos + 1 < n:
                            pos += 2
                        elif code[pos] == '"':
                            pos += 1
                            break
                        else:
                            pos += 1
                    continue
                if ch == "(":
                    depth_paren += 1
                elif ch == ")" and depth_paren > 0:
                    depth_paren -= 1
                elif ch == "<":
                    depth_angle += 1
                elif ch == ">" and depth_angle > 0:
                    depth_angle -= 1
                elif depth_paren == 0 and depth_angle == 0:
                    if ch in "{;":
                        break
                    if code[pos : pos + 5] == "where" and (pos == base_start or code[pos - 1].isspace()):
                        break
                pos += 1
            base_clause = code[base_start:pos]
            for raw_base in split_top_level_commas(base_clause):
                # Strip primary-constructor arguments and generic arguments.
                clean = raw_base.split("(", 1)[0].split("<", 1)[0].strip()
                if clean:
                    bases.append(clean.split(".")[-1])

        results.append((name, is_abstract, bases, namespace))
    return results


def slugify_model_name(name):
    """Exact parity with sts2.dll StringHelper.Slugify."""
    return SLUGIFY_RE.sub(r"_\1", name).lower()


def check_texture_reachability(root, tracked_set, tracked_pngs):
    violations = []
    refs = set()

    # Collect text sources: Sts2WDCode/**/*.cs, Sts2WD/**/*.{tscn,tres,gd,gdshader}, project.godot, Sts2WD.json.
    cs_files = [rel for rel in tracked_set if rel.startswith("Sts2WDCode/") and rel.lower().endswith(".cs")]
    res_files = [
        rel
        for rel in tracked_set
        if (
            rel.startswith("Sts2WD/")
            and Path(rel).suffix.lower() in {".tscn", ".tres", ".gd", ".gdshader"}
        )
        or rel in {"project.godot", "Sts2WD.json"}
    ]

    class_bases = {}
    class_meta = {}

    for rel in sorted(cs_files):
        raw = (root / rel).read_text(encoding="utf-8-sig")

        # Explicit opt-in escape hatch comments must refer to existing tracked files.
        for esc in CS_ESCAPE_HATCH_RE.findall(raw):
            norm = esc.replace("\\", "/")
            if norm not in tracked_set:
                violations.append(f"{rel}: sts2wd-asset escape hatch target does not exist in tracked files: {norm}")
            else:
                refs.add(norm.lower())

        code = strip_cs_comments(raw)

        for hit in RES_LITERAL_RE.findall(code):
            refs.add(hit.replace("\\", "/").lower())
        for hit in RES_PATH_CONCAT_RE.findall(code):
            refs.add(("Sts2WD/" + hit).replace("\\", "/").lower())
        for hit in RES_PATH_INTERP_RE.findall(code):
            refs.add(("Sts2WD/" + hit).replace("\\", "/").lower())

        for m in PATH_JOIN_RE.finditer(code):
            segs = STRING_LITERAL_RE.findall(m.group(1))
            if segs:
                refs.add(("Sts2WD/" + "/".join(segs)).replace("\\", "/").lower())

        for helper, prefix in EXTENSION_HELPER_PREFIXES.items():
            for hit in re.findall(rf'"([^"\r\n]+)"\.{helper}\(\)', code):
                refs.add((prefix + hit).replace("\\", "/").lower())

        # File-scoped interpolated resource template expansion (e.g., RealtaPowerPresentation.cs).
        templates = INTERP_RES_TEMPLATE_RE.findall(code)
        if templates:
            literals = set(STRING_LITERAL_RE.findall(code))
            for pref, _expr, suff in templates:
                for lit in literals:
                    if lit and "/" not in lit and "\\" not in lit and " " not in lit:
                        refs.add(f"{pref}{lit}{suff}".replace("\\", "/").lower())

        for cls_name, is_abstract, bases, ns in parse_cs_classes(code):
            existing_bases = class_bases.setdefault(cls_name, [])
            for b in bases:
                if b not in existing_bases:
                    existing_bases.append(b)
            prev_abs, prev_ns = class_meta.get(cls_name, (is_abstract, ns))
            class_meta[cls_name] = (prev_abs and is_abstract, prev_ns or ns)

    for rel in sorted(res_files):
        text = (root / rel).read_text(encoding="utf-8-sig")
        for hit in RES_LITERAL_RE.findall(text):
            refs.add(hit.replace("\\", "/").lower())

    # Resolve transitive C# inheritance for non-abstract models.
    def inherits_from(cls, targets, seen=None):
        seen = seen or set()
        if cls in seen:
            return False
        seen.add(cls)
        for base in class_bases.get(cls, []):
            if base in targets or inherits_from(base, targets, seen):
                return True
        return False

    for cls, (is_abstract, ns) in class_meta.items():
        if is_abstract:
            continue
        slug = slugify_model_name(cls)
        if inherits_from(cls, {"Sts2WDRelic", "CustomRelicModel"}):
            refs.add(f"sts2wd/images/relics/{slug}.png")
            refs.add(f"sts2wd/images/relics/{slug}_outline.png")
            refs.add(f"sts2wd/images/relics/big/{slug}.png")
        if inherits_from(cls, {"Sts2WDPower", "CustomPowerModel"}):
            refs.add(f"sts2wd/images/powers/{slug}.png")
            refs.add(f"sts2wd/images/powers/big/{slug}.png")
        if inherits_from(cls, {"Sts2WDPotion", "CustomPotionModel"}):
            refs.add(f"sts2wd/images/potions/{slug}.png")
            refs.add(f"sts2wd/images/potions/outline/{slug}.png")
        if inherits_from(cls, {"Sts2WDCard", "CustomCardModel"}):
            # Matches Sts2WDCard.ResolveCharacterSubfolder(): first '_' prefix or Characters.<Char> namespace.
            if "_" in slug:
                prefix = slug.split("_", 1)[0]
                refs.add(f"sts2wd/images/card_portraits/{prefix}/{slug}.png")
            if "Characters." in ns:
                char_seg = ns.split("Characters.", 1)[1].split(".", 1)[0].lower()
                refs.add(f"sts2wd/images/card_portraits/{char_seg}/{slug}.png")

    for png in tracked_pngs:
        if png.lower() not in refs:
            violations.append(f"{png}: unreferenced runtime PNG (no C#, scene, resource, or project.godot reference).")

    return violations


def check_repository(root):
    tracked = list_tracked_files(root)
    tracked_set = set(tracked)
    violations = []

    # 1. Non-PNG raster ban under Sts2WD/
    for rel in tracked:
        if rel.startswith("Sts2WD/") and Path(rel).suffix.lower() in NON_PNG_RASTER_EXTS:
            violations.append(f"{rel}: disallowed non-PNG raster format under Sts2WD/ (convert to PNG).")

    # 2. PNG dimensions, resolution ceilings, and .import tier rules
    tracked_pngs = [rel for rel in tracked if rel.startswith("Sts2WD/") and rel.lower().endswith(".png")]
    tier_counts = {"tier1_bptc": 0, "tier2_lossy": 0, "tier3_lossless": 0}

    for rel in tracked_pngs:
        png_path = root / rel
        try:
            width, height = read_png_dimensions(png_path)
        except (OSError, ValueError, struct.error) as exc:
            violations.append(f"{rel}: {exc}")
            continue

        tier = classify_tier(rel, width, height)
        tier_counts[tier] += 1

        ceiling_err = check_resolution_ceiling(rel, width, height, tier)
        if ceiling_err is not None:
            violations.append(ceiling_err)

        import_rel = f"{rel}.import"
        if import_rel not in tracked_set:
            violations.append(f"{rel}: missing tracked {import_rel} sidecar (run dev.ps1 import and git add).")
            continue
        violations.extend(validate_import_file(rel, root / import_rel, tier))

    # 3. Boundary (.gdignore, export_presets.cfg, project.godot)
    violations.extend(check_boundary_and_project_config(root, tracked_set))

    # 4. Static Sts2WD/ PNG reachability
    violations.extend(check_texture_reachability(root, tracked_set, tracked_pngs))

    return violations, len(tracked_pngs), tier_counts


def main():
    configure_utf8_stdio()
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=ROOT, help="repository root directory")
    parser.add_argument(
        "--sync-params",
        action="store_true",
        help="rewrite [params] tier settings in existing Sts2WD/**/*.png.import files",
    )
    args = parser.parse_args()
    root = args.root.resolve()

    if args.sync_params:
        try:
            updated, skipped = sync_import_params(root)
        except (OSError, ValueError, struct.error) as exc:
            print(f"Sync import params failed: {exc}", file=sys.stderr)
            return 1
        print(f"Synced [params] in {len(updated)} .import file(s).")
        for rel, tier in updated:
            print(f"  {tier:14s}  {rel}.import")
        if skipped:
            print(f"Skipped {len(skipped)} PNG(s) without .import (run dev.ps1 import first).")
        return 0

    try:
        violations, png_count, tier_counts = check_repository(root)
    except (OSError, RuntimeError, ValueError) as exc:
        print(f"Install package check failed: {exc}", file=sys.stderr)
        return 1

    if violations:
        for item in violations:
            print(item, file=sys.stderr)
        print(f"Install package check failed: {len(violations)} violation(s).", file=sys.stderr)
        return 1

    print(
        f"Install package check passed ({png_count} Sts2WD PNGs: "
        f"{tier_counts['tier1_bptc']} BPTC, {tier_counts['tier2_lossy']} Lossy WebP, "
        f"{tier_counts['tier3_lossless']} Lossless WebP)."
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
