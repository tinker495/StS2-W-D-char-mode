"""Export a looping WebP from existing rigc render frames."""

import argparse
from collections.abc import Iterator
from io import BytesIO
import json
import math
from pathlib import Path
import re
import sys

from PIL import Image, ImageSequence


FRAME_NAME = re.compile(r"f([0-9]{4,})\.png")


def find_manifests(source: Path) -> list[Path]:
    if source.is_file():
        return [source]
    direct = source / "frames.json"
    if direct.is_file():
        return [direct]
    manifests = sorted(
        (path for path in source.glob("*/frames.json") if path.parent.name.isdecimal()),
        key=lambda path: int(path.parent.name),
    )
    if not manifests:
        raise ValueError(f"{source}: no rigc frame manifests")
    return manifests


def timed_frames(manifest: Path) -> Iterator[tuple[Path, int]]:
    report = json.loads(manifest.read_text(encoding="utf-8-sig"))
    if not isinstance(report, dict) or report.get("spec") != "rigc-frames/1" or not report.get("sets"):
        raise ValueError(f"{manifest}: expected rigc-frames/1 with rendered sets")

    for frame_set in report["sets"]:
        directory = manifest.parent / frame_set["dir"]
        fps = frame_set["fps"]
        if not math.isfinite(fps) or fps <= 0:
            raise ValueError(f"{manifest}: expected a positive frame rate for {directory.name}")
        frames = sorted(
            (int(match[1]), path) for path in directory.iterdir()
            if path.is_file() and (match := FRAME_NAME.fullmatch(path.name))
        )
        sampled = frame_set["sampled"]
        if not frames or len(frames) != frame_set["written"] or frames[-1][0] >= sampled:
            raise ValueError(f"{directory}: frame files do not match {manifest.name}")

        # A render may retain only some sample indices. Hold each pictured
        # sample until the next one and distribute millisecond rounding.
        ends = [index for index, _ in frames[1:]] + [sampled]
        for (index, path), end in zip(frames, ends, strict=True):
            delay = round(end * 1000 / fps) - round(index * 1000 / fps)
            if delay < 1:
                raise ValueError(f"{path}: frame interval is below one millisecond")
            yield path, delay


def render_preview(source: Path, output: Path, max_edge: int) -> None:
    images = []
    delays = []
    for manifest in find_manifests(source):
        for path, delay in timed_frames(manifest):
            with Image.open(path) as image:
                frame = image.convert("RGBA")
            frame.thumbnail((max_edge, max_edge), Image.Resampling.LANCZOS)
            if images and frame.size != images[0].size:
                raise ValueError(f"{path}: inconsistent frame size; export this render separately")
            images.append(frame)
            delays.append(delay)

    with BytesIO() as encoded:
        images[0].save(encoded, format="WEBP", save_all=True, append_images=images[1:],
                       duration=delays, loop=0, quality=90)
        with Image.open(encoded) as preview:
            # WebP can coalesce identical frames or emit a still for a fixed
            # pose. Compare total playback time when the result animates.
            if preview.is_animated:
                duration = 0
                for frame in ImageSequence.Iterator(preview):
                    frame.load()
                    duration += frame.info["duration"]
                if duration != sum(delays):
                    raise ValueError(f"preview duration changed from {sum(delays)} to {duration} ms")
            else:
                preview.load()
            frame_count = preview.n_frames

        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_bytes(encoded.getvalue())
    print(f"Wrote {output} ({frame_count} encoded frames from {len(images)} rendered samples)")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path, help="frames.json or the rigc package diagnostic directory")
    parser.add_argument("--out", required=True, type=Path, help="destination .webp")
    parser.add_argument("--max", type=int, default=720, dest="max_edge", help="maximum long edge (default: 720)")
    args = parser.parse_args()
    if args.out.suffix.lower() != ".webp" or args.max_edge < 1:
        parser.error("--out must end in .webp and --max must be positive")
    try:
        render_preview(args.source, args.out, args.max_edge)
    except (OSError, ValueError, KeyError, TypeError) as error:
        print(f"Rig preview: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
