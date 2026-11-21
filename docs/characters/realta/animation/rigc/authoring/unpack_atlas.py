"""Losslessly unpack Spine atlas regions back into individual part PNGs."""

from __future__ import annotations

import argparse
from fnmatch import fnmatch
from pathlib import Path

from PIL import Image


DEFAULT_SKIP = ("composites/*", "revive_*")
PAGE_KEYS = frozenset({"size", "format", "filter", "repeat", "pma", "scale"})


def parse_ints(value: str) -> list[int]:
    return [int(part.strip()) for part in value.split(",")]


def parse_rotate(value: str) -> int:
    raw = value.strip().lower()
    if raw in ("false", "0"):
        return 0
    if raw in ("true", "90"):
        return 90
    if raw in ("180", "270"):
        return int(raw)
    raise ValueError(f"Unsupported Spine atlas rotate value: {value!r}")


def parse_atlas(text: str) -> list[dict]:
    """Supports both Spine 3.x (xy/size/orig/offset) and 4.x (bounds/offsets) atlas formats."""
    regions: list[dict] = []
    current_page: str | None = None
    current: dict | None = None

    for raw_line in text.splitlines():
        line = raw_line.strip()
        if not line:
            current_page = None
            current = None
            continue

        if ":" not in line:
            if current_page is None:
                current_page = line
                current = None
            else:
                current = {
                    "page": current_page,
                    "name": line,
                    "rotate": 0,
                }
                regions.append(current)
            continue

        key, value = (part.strip() for part in line.split(":", 1))
        if current is None:
            if key not in PAGE_KEYS:
                raise ValueError(f"Unexpected page header field {key!r} on page {current_page!r}")
            continue

        if key == "bounds":
            x, y, w, h = parse_ints(value)
            current.update(x=x, y=y, w=w, h=h)
        elif key == "offsets":
            ox, oy, orig_w, orig_h = parse_ints(value)
            current.update(ox=ox, oy=oy, orig_w=orig_w, orig_h=orig_h)
        elif key == "xy":
            x, y = parse_ints(value)
            current.update(x=x, y=y)
        elif key == "size":
            w, h = parse_ints(value)
            current.update(w=w, h=h)
        elif key == "orig":
            orig_w, orig_h = parse_ints(value)
            current.update(orig_w=orig_w, orig_h=orig_h)
        elif key == "offset":
            ox, oy = parse_ints(value)
            current.update(ox=ox, oy=oy)
        elif key == "rotate":
            current["rotate"] = parse_rotate(value)

    for region in regions:
        for required in ("x", "y", "w", "h"):
            if required not in region:
                raise ValueError(f"Atlas region {region['name']!r} is missing {required!r}")
        region.setdefault("ox", 0)
        region.setdefault("oy", 0)
        region.setdefault("orig_w", region["w"])
        region.setdefault("orig_h", region["h"])

    return regions


def unpack_region(atlas_img: Image.Image, region: dict) -> Image.Image:
    x, y, w, h = region["x"], region["y"], region["w"], region["h"]
    rot = region["rotate"]

    if rot == 0:
        crop = atlas_img.crop((x, y, x + w, y + h))
    elif rot == 90:
        crop = atlas_img.crop((x, y, x + h, y + w)).transpose(Image.Transpose.ROTATE_90)
    elif rot == 180:
        crop = atlas_img.crop((x, y, x + w, y + h)).transpose(Image.Transpose.ROTATE_180)
    elif rot == 270:
        crop = atlas_img.crop((x, y, x + h, y + w)).transpose(Image.Transpose.ROTATE_270)
    else:
        raise ValueError(f"Unsupported rotation {rot} for region {region['name']!r}")

    ox, oy = region["ox"], region["oy"]
    orig_w, orig_h = region["orig_w"], region["orig_h"]
    if (ox, oy, orig_w, orig_h) == (0, 0, w, h):
        return crop

    canvas = Image.new("RGBA", (orig_w, orig_h), (0, 0, 0, 0))
    top = orig_h - oy - h
    canvas.paste(crop, (ox, top))
    return canvas


def should_skip(name: str, patterns: tuple[str, ...]) -> bool:
    return any(fnmatch(name, pat) for pat in patterns)


def unpack_atlas(
    atlas_path: Path,
    png_path: Path,
    out_dir: Path,
    skip_patterns: tuple[str, ...] = DEFAULT_SKIP,
) -> list[Path]:
    regions = parse_atlas(atlas_path.read_text(encoding="utf-8"))
    out_dir.mkdir(parents=True, exist_ok=True)
    written: list[Path] = []

    with Image.open(png_path) as atlas_img:
        rgba = atlas_img.convert("RGBA")
        for region in regions:
            name = region["name"]
            if should_skip(name, skip_patterns):
                continue
            part = unpack_region(rgba, region)
            dest = out_dir / f"{name}.png"
            dest.parent.mkdir(parents=True, exist_ok=True)
            part.save(dest, format="PNG")
            written.append(dest)

    return written


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--atlas", type=Path, required=True, help="Path to Spine .atlas / atlas.txt file")
    parser.add_argument("--png", type=Path, required=True, help="Path to packed atlas PNG image")
    parser.add_argument("--out", type=Path, required=True, help="Output directory for unpacked part PNGs")
    parser.add_argument(
        "--skip",
        action="append",
        default=None,
        help="Glob pattern of region names to skip (default: composites/*, revive_*)",
    )
    args = parser.parse_args()
    patterns = tuple(args.skip) if args.skip is not None else DEFAULT_SKIP
    written = unpack_atlas(args.atlas, args.png, args.out, patterns)
    print(f"Unpacked {len(written)} parts from {args.png.name} into {args.out}")


if __name__ == "__main__":
    main()
