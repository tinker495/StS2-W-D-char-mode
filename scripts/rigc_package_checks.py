"""Read native rigc diagnostics, verify loose parts, and assemble a peak-frame sheet."""

import argparse
from collections import Counter, defaultdict
import json
from pathlib import Path
import sys

import numpy as np
from PIL import Image, ImageChops, ImageDraw, ImageStat
from scipy.ndimage import label

from check_png_alpha import count_invalid


def load_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def load_report(path, spec):
    report = load_json(path)
    if report.get("spec") != spec:
        raise ValueError(f"{path}: expected {spec}")
    return report


def frame_path(directory, index):
    return directory / f"f{index:04d}.png"


def peak_motion_frame(directory, count):
    def preview(index):
        with Image.open(frame_path(directory, index)) as image:
            image = image.convert("RGB")
        image.thumbnail((128, 128))
        return image

    first = preview(0)
    return max(
        range(count),
        key=lambda index: sum(ImageStat.Stat(ImageChops.difference(first, preview(index))).sum),
    )


def inspect_seams(directory, animation, count, peaks, errors):
    report = load_report(directory / "seam.json", "rigc-seam/1")
    if not report["pairs"]:
        raise ValueError(f"{animation}: no requested seam pairs in the render report")
    readings = {}
    for pair in report["pairs"]:
        name = f'{pair["a"]}:{pair["b"]}'
        contact = pair.get("jointContact", {}).get("frames", [])
        indices = {frame["index"] for frame in contact}
        if len(indices) != len(contact) or any(not 0 <= index < count for index in indices):
            raise ValueError(f"{animation}: {name} has invalid contact frame indices")
        statuses = Counter(frame["status"] for frame in contact)
        separated = [frame["index"] for frame in contact if frame["status"] == "separated"]
        # A missing contact sample requires the same visual follow-up as an unavailable one.
        manual = count - statuses["joined"] - statuses["separated"]
        readings[name] = Counter(separated=len(separated), joined=statuses["joined"], manual=manual)
        print(f"{animation}: {name} contact: {len(separated)} separated, "
              f"{statuses['joined']} joined, {manual} require visual inspection")
        if separated:
            errors.append(f"{animation}: {name} separated at frames {separated}")
            peaks[separated[0]].append(f"{name} separated")
        if manual:
            unknown = [frame for frame in contact if frame["status"] not in ("joined", "separated")]
            unavailable = sorted({frame.get("reason") or "unavailable contact" for frame in unknown})
            if len(contact) < count:
                unavailable.append("missing contact samples")
            reason = "; ".join(unavailable)
            print(f"WARNING {animation}: {name} requires visual inspection ({reason})")
            resolved = indices - {frame["index"] for frame in unknown}
            first_manual = next(index for index in range(count) if index not in resolved)
            peaks[first_manual].append(f"{name} requires visual inspection")
        for metric in ("gapOpen", "ghost"):
            worst = pair[metric]["worst"]
            if worst is not None:
                peaks[worst["index"]].append(f"{name} {metric}")
    return readings


def inspect_render(run, checkpoints, check_seams, errors):
    manifest = load_report(run / "frames.json", "rigc-frames/1")
    [clip] = manifest["sets"]
    animation = clip["animation"]
    size = (manifest["viewport"]["pixelWidth"], manifest["viewport"]["pixelHeight"])
    fps, count = clip["fps"], clip["written"]
    if max(size) != 1024 or fps != 12 or count < 1 or clip["stride"] != 1:
        raise ValueError(f"{animation}: expected a complete 12 fps render at 1024 px, got {size}")
    directory = run / clip["dir"]
    peaks = defaultdict(list)
    for checkpoint in checkpoints:
        if checkpoint.get("animation") == animation and "time" in checkpoint:
            index = min(count - 1, int(checkpoint["time"] * fps + 0.5))
            peaks[index].append(checkpoint["id"])
    if not peaks:
        peaks[peak_motion_frame(directory, count)].append("largest change from first frame")

    folds = load_report(directory / "folds.json", "rigc-folds/1")
    folded = [mesh for mesh in folds["meshes"] if mesh["worst"] is not None or mesh["frames"]]
    print(f'{animation}: {len(folded)}/{len(folds["meshes"])} meshes folded, {count} frames')
    for mesh in folded:
        name = f'{mesh["slot"]}/{mesh["attachment"]}'
        errors.append(f"{animation}: folded mesh {name}, frames {mesh['frames']}")
        if mesh["worst"] is not None:
            peaks[mesh["worst"]["index"]].append(f"{name} fold")
    seams = inspect_seams(directory, animation, count, peaks, errors) if check_seams else {}

    tiles = [
        (frame_path(directory, index), size, f"{animation} | frame {index} | {index / fps:.3f}s", ", ".join(reasons))
        for index, reasons in sorted(peaks.items())
    ]
    return tiles, len(folded), seams


def write_sheet(path, tiles):
    columns = min(3, len(tiles))
    rows = (len(tiles) + columns - 1) // columns
    tile_width, tile_height = 1036, 1070
    sheet = Image.new("RGB", (columns * tile_width, rows * tile_height), "#20242b")
    draw = ImageDraw.Draw(sheet)
    for index, (frame, size, title, reason) in enumerate(tiles):
        x, y = index % columns * tile_width, index // columns * tile_height
        with Image.open(frame) as image:
            if image.size != size:
                raise ValueError(f"{frame}: expected {size}, got {image.size}")
            sheet.paste(image, (x + (tile_width - image.width) // 2, y + 40))
        draw.text((x + 8, y + 6), title, fill="white")
        draw.text((x + 8, y + 22), reason, fill="#c4ccdc")
    sheet.save(path)


def inspect_parts(directory, material_leaks, errors):
    parts = sorted(path for path in directory.rglob("*") if path.is_file() and path.suffix.lower() == ".png")
    if not parts:
        raise ValueError(f"{directory}: no loose PNGs")
    masks = defaultdict(list)
    for rule in material_leaks:
        part, separator, mask = rule.partition("=")
        if not separator or not part or not mask:
            raise ValueError(f"Expected part.png=mask.png for material leak check, got {rule!r}")
        masks[Path(part).as_posix()].append(Path(mask))

    dirty_pixels = 0
    warnings = 0
    for part in parts:
        name = part.relative_to(directory).as_posix()
        dirty = count_invalid(part.read_bytes())
        if dirty:
            errors.append(f"{name}: {dirty} low-alpha pixels must be transparent black")
            dirty_pixels += dirty

        with Image.open(part) as image:
            alpha = np.asarray(image.convert("RGBA"))[:, :, 3]

        components, count = label(alpha >= 128)  # SciPy's default is 4-connected in 2D.
        if count > 1:
            sizes = sorted(np.bincount(components.ravel())[1:], reverse=True)
            warnings += 1
            print(f"WARNING {name}: {count - 1} secondary components at alpha >= 128 "
                  f"({sum(sizes[1:])} px total, largest {sizes[1]} px)")

        for mask in masks.pop(name, []):
            with Image.open(mask) as image:
                forbidden = np.asarray(image.convert("L")) > 0
            if alpha.shape != forbidden.shape:
                errors.append(f"{name}: material mask {mask} dimensions differ from the part")
                continue
            leaked = np.count_nonzero((alpha > 0) & forbidden)
            print(f"{name}: {leaked} pixels in forbidden material mask {mask}")
            if leaked:
                errors.append(f"{name}: {leaked} material leak pixels in {mask}")
    for name in sorted(masks):
        errors.append(f"Unknown loose part for material mask: {name}")
    print(f"Loose PNGs: {len(parts)}, transparent-black violations: {dirty_pixels}, "
          f"PNGs with secondary components: {warnings}")


def inspect_renders(package, output, check_seams, errors):
    checkpoints = load_json(package / "review.json").get("checkpoints", [])
    tiles = []
    folded_total = 0
    seam_totals = defaultdict(Counter)
    runs = sorted(path for path in output.iterdir() if path.is_dir())
    for run in runs:
        clip_tiles, folded, seams = inspect_render(run, checkpoints, check_seams, errors)
        tiles.extend(clip_tiles)
        folded_total += folded
        for name, counts in seams.items():
            seam_totals[name].update(counts)
    if not tiles:
        raise ValueError("No rendered animation frames to inspect")

    sheet = output.resolve() / "peak-frames.png"
    write_sheet(sheet, tiles)
    print(f"Mesh folds: {folded_total} folded meshes across {len(runs)} clips")
    for name, counts in sorted(seam_totals.items()):
        print(f"Seam {name}: {counts['separated']} separated, {counts['joined']} joined, "
              f"{counts['manual']} require visual inspection")
    print(f"Peak-frame closeups ({len(tiles)} native-size frames): {sheet}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("package", type=Path)
    parser.add_argument("output", nargs="?", type=Path)
    stage = parser.add_mutually_exclusive_group()
    stage.add_argument("--parts-only", action="store_true", help="scan loose parts before rendering")
    stage.add_argument("--renders-only", action="store_true", help="read renders after the loose-part scan")
    parser.add_argument("--seams", action="store_true")
    parser.add_argument("--material-leak", action="append", default=[], metavar="PART=MASK")
    args = parser.parse_args()
    if args.parts_only and (args.output is not None or args.seams):
        parser.error("--parts-only takes the package and optional material masks")
    if not args.parts_only and args.output is None:
        parser.error("output is required to inspect renders")
    if args.renders_only and args.material_leak:
        parser.error("--material-leak requires loose-part inspection")

    errors = []
    if not args.renders_only:
        inspect_parts(args.package / "parts", args.material_leak, errors)
    if not args.parts_only:
        inspect_renders(args.package, args.output, args.seams, errors)
    for error in errors:
        print(f"ERROR {error}", file=sys.stderr)
    return int(bool(errors))


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (OSError, ValueError, KeyError, ImportError) as error:
        sys.exit(f"Rig diagnostics: {error}")
