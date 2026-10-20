"""Build complete-frame review sheets and playback GIFs from native captures.

All inputs are explicit arguments. An optional baseline is another capture made
with the same tool/camera; no earlier draft, official capture or local cache is
required. Outputs aid human review and are not image acceptance oracles.
"""
import argparse
import json
from pathlib import Path

from PIL import Image, ImageDraw


def read_capture(folder):
    metadata = json.loads((folder / "capture.json").read_text(encoding="utf-8"))
    paths = [folder / f"{index:04d}.png" for index in range(metadata["frame_count"])]
    missing = [str(path) for path in paths if not path.is_file()]
    if missing:
        raise ValueError(f"Missing native frames: {missing[:3]}")
    return metadata, paths


def composite(path, size, background):
    result = Image.new("RGB", tuple(size), background)
    if path is not None:
        with Image.open(path) as source:
            image = source.convert("RGBA")
            if image.size != tuple(size):
                raise ValueError(f"Frame size disagrees with capture metadata: {path}")
            result.paste(image, (0, 0), image.getchannel("A"))
    return result


def build_review(folder, destination, baseline):
    metadata, frames = read_capture(folder)
    previous = []
    if baseline is not None:
        before, previous = read_capture(baseline)
        for field in ("variant", "clip", "fps", "size", "origin", "scale", "renderer"):
            if before[field] != metadata[field]:
                raise ValueError(f"Incompatible comparison {field}: {baseline} / {folder}")
    destination.mkdir(parents=True, exist_ok=False)
    width, height = metadata["size"]
    fps = metadata["fps"]
    dark = (34, 39, 48)
    playback = [composite(path, metadata["size"], dark) for path in frames]
    # GIF durations use centiseconds. Distribute rounding across the sequence.
    durations = [10 * (round((n + 1) * 100 / fps) - round(n * 100 / fps)) for n in range(len(frames))]
    playback[0].save(destination / "playback.gif", save_all=True, append_images=playback[1:],
                     duration=durations, loop=0, optimize=False)
    for image in playback:
        image.close()
    samples_per_page = 3 if baseline is not None else 6
    columns = 2 if baseline is not None else 3
    count = max(len(frames), len(previous))
    for theme, color in (("dark", dark), ("light", (235, 235, 235))):
        for start in range(0, count, samples_per_page):
            items = []
            for index in range(start, min(start + samples_per_page, count)):
                groups = [("before", previous), ("after", frames)] if baseline is not None else [("current", frames)]
                for label, sequence in groups:
                    path = sequence[index] if index < len(sequence) else None
                    suffix = "" if path is not None else " (no frame)"
                    items.append((path, f"{label} {index:04d}  t={index / fps:.4f}s{suffix}"))
            rows = (len(items) + columns - 1) // columns
            sheet = Image.new("RGB", (columns * width, rows * (height + 24)), color)
            drawing = ImageDraw.Draw(sheet)
            for cell, (path, label) in enumerate(items):
                x, y = (cell % columns) * width, (cell // columns) * (height + 24)
                drawing.text((x + 8, y + 5), label, fill="white" if theme == "dark" else "black")
                sheet.paste(composite(path, metadata["size"], color), (x, y + 24))
            sheet.save(destination / f"{theme}-{start:04d}.png")
            sheet.close()
    print(f"{metadata['variant']}/{metadata['clip']}: {count} frame positions, sheets and GIF -> {destination}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("frames", type=Path, help="Capture root containing variant/clip/capture.json")
    parser.add_argument("output", type=Path, help="New review directory")
    parser.add_argument("--baseline", type=Path, help="Optional previous capture root")
    args = parser.parse_args()
    captures = sorted(args.frames.glob("*/*/capture.json"))
    if not captures:
        parser.error("No native captures found")
    if args.output.exists():
        parser.error("Use a fresh review output directory")
    for manifest in captures:
        relative = manifest.parent.relative_to(args.frames)
        baseline = args.baseline / relative if args.baseline is not None else None
        build_review(manifest.parent, args.output / relative, baseline)
    print("REVIEW_COMPLETE")


if __name__ == "__main__":
    main()
