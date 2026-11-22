"""Check runtime card portrait PNGs for expected dimensions, PNG encoding, and full opacity."""

# Human requester: wjdrb, 2026-10-29, retrospective items 1 and 2:
# check the delivered card canvases and every pixel's opacity in local verify and CI.
# Human requester: KyuSeok Jung (wjdrb), 2026-11-22: migrate card portrait checks
# from asset-pipeline.ps1 check-cards (Windows System.Drawing) to cross-platform
# Python reusing scripts/normalize_png_alpha.py decoding.

import argparse
from io import BytesIO
import json
from pathlib import Path
import struct
import sys

import numpy as np
from PIL import Image, UnidentifiedImageError

SCRIPT_DIR = Path(__file__).resolve().parent
if str(SCRIPT_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPT_DIR))

from normalize_png_alpha import rgba_samples

ROOT = SCRIPT_DIR.parent
CARD_SIZES = {
    "Standard": (1000, 760),
    "Ancient": (606, 852),
    "Thumbnail": (250, 190),
}


def configure_utf8_stdio():
    for stream in (sys.stdout, sys.stderr):
        reconfigure = getattr(stream, "reconfigure", None)
        if reconfigure is not None:
            reconfigure(encoding="utf-8", errors="replace")


def load_layout_overrides(layouts_path):
    raw = json.loads(layouts_path.read_text(encoding="utf-8-sig"))
    if not isinstance(raw, dict) or not isinstance(raw.get("overrides"), dict):
        raise ValueError(f"{layouts_path}: expected JSON object with an 'overrides' mapping.")
    overrides = {}
    for rel_key, layout_name in raw["overrides"].items():
        if not isinstance(rel_key, str) or not isinstance(layout_name, str) or layout_name not in CARD_SIZES:
            raise ValueError(f"Unknown card layout '{layout_name}' for {rel_key}.")
        overrides[rel_key.replace("\\", "/").lower()] = layout_name
    return overrides


def validate_card_png(path, relative, expected_size):
    data = path.read_bytes()
    with Image.open(BytesIO(data)) as image:
        if image.format != "PNG":
            return f"{relative}: expected PNG encoding."
        if getattr(image, "is_animated", False):
            return f"{relative}: expected static PNG, not animated PNG."
        if image.size != expected_size:
            exp_w, exp_h = expected_size
            got_w, got_h = image.size
            return f"{relative}: expected {exp_w}x{exp_h}, got {got_w}x{got_h}."

    pixels = rgba_samples(data)
    max_alpha = int(np.iinfo(pixels.dtype).max)
    min_alpha = int(pixels[..., 3].min())
    if min_alpha != max_alpha:
        return f"{relative}: every card pixel must have alpha {max_alpha}; minimum alpha is {min_alpha}."
    return None


def check_repository(root):
    card_root = root / "Sts2WD" / "images" / "card_portraits"
    layouts_path = root / "scripts" / "card-portrait-layouts.json"
    if not card_root.is_dir():
        raise FileNotFoundError(f"Card portrait directory not found: {card_root}")
    if not layouts_path.is_file():
        raise FileNotFoundError(f"Card portrait layouts file not found: {layouts_path}")

    overrides = load_layout_overrides(layouts_path)
    png_files = sorted(
        (p for p in card_root.rglob("*") if p.is_file() and p.suffix.lower() == ".png"),
        key=lambda p: p.relative_to(card_root).as_posix().lower(),
    )
    if not png_files:
        raise ValueError(f"No card portrait PNGs found under {card_root}.")

    failures = []
    for path in png_files:
        relative = path.relative_to(card_root).as_posix()
        layout_name = overrides.get(relative.lower(), "Standard")
        expected_size = CARD_SIZES[layout_name]
        try:
            error = validate_card_png(path, relative, expected_size)
        except (OSError, ValueError, ImportError, struct.error, UnidentifiedImageError) as exc:
            error = f"{relative}: {exc}"
        if error is not None:
            failures.append(error)

    return failures, len(png_files)


def main():
    configure_utf8_stdio()
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=ROOT, help="repository root directory")
    args = parser.parse_args()
    root = args.root.resolve()

    try:
        failures, count = check_repository(root)
    except (OSError, ValueError, json.JSONDecodeError) as error:
        print(f"Card portrait check failed: {error}", file=sys.stderr)
        return 1

    if failures:
        for failure in failures:
            print(failure, file=sys.stderr)
        print(f"Card portrait check failed: {len(failures)} violation(s).", file=sys.stderr)
        return 1

    print(f"Card portrait check passed ({count} PNGs; exact dimensions and fully opaque pixels).")
    return 0


if __name__ == "__main__":
    sys.exit(main())
