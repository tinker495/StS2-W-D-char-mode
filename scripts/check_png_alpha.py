"""Check stored PNG pixels against the repository's transparent-black standard."""

import argparse
from io import BytesIO
from pathlib import Path
import struct
import subprocess
import sys
import zlib

import numpy as np
from PIL import Image


# Requested by wjdrb, 2026-11-19: normalize alpha 0 through 8 (inclusive)
# throughout the repository and catch residual RGB as well as faint alpha.
MAX_ALPHA = 8
ROOT = Path(__file__).resolve().parents[1]


def png_chunk(kind, data):
    return (struct.pack(">I", len(data)) + kind + data
            + struct.pack(">I", zlib.crc32(kind + data)))


def stored_frames(data):
    """Expose APNG source pixels before compositing can hide transparent RGB."""
    header = data[16:29]
    palette = b""
    frame_header = header
    payload = []
    offset = 8
    while offset < len(data):
        length, kind = struct.unpack_from(">I4s", data, offset)
        body = data[offset + 8:offset + 8 + length]
        offset += length + 12
        if kind in (b"fcTL", b"IEND") and payload:
            yield (data[:8] + png_chunk(b"IHDR", frame_header) + palette
                   + b"".join(payload) + png_chunk(b"IEND", b""))
            payload.clear()
        if kind == b"fcTL":
            frame_header = body[4:12] + header[8:]
        elif kind in (b"PLTE", b"tRNS"):
            palette += png_chunk(kind, body)
        elif kind == b"IDAT":
            payload.append(png_chunk(b"IDAT", body))
        elif kind == b"fdAT":
            payload.append(png_chunk(b"IDAT", body[4:]))


def rgba8_samples(image, depth, color_type):
    pixels = np.array(image.convert("RGBA"))
    key = image.info.get("transparency")
    if color_type == 0 and depth in (1, 2, 4) and key is not None:
        # Requested by wjdrb, 2026-11-19: match the native tRNS sample to
        # Pillow's expanded gray channel for both validation and normalization.
        gray_key = key * 255 // ((1 << depth) - 1)
        pixels[..., 3] = np.where(pixels[..., 0] == gray_key, 0, 255)
    return pixels


def count_invalid(data):
    with Image.open(BytesIO(data)) as image:
        if image.format != "PNG":
            raise ValueError("expected a PNG image")
        if image.is_animated:
            return sum(count_invalid(frame) for frame in stored_frames(data))
        if "A" not in image.getbands() and "transparency" not in image.info:
            return 0
        if data[24] == 16:
            # Pillow reduces multichannel 16-bit PNGs to eight bits on decode.
            # Inspect the original samples, including tRNS color-keyed images.
            import cv2

            pixels = cv2.imdecode(np.frombuffer(data, np.uint8), cv2.IMREAD_UNCHANGED)
            if pixels is None or pixels.dtype != np.uint16:
                raise ValueError("could not decode 16-bit PNG samples")
            if data[25] in (0, 2):
                key = image.info["transparency"]
                if not np.any(key):
                    return 0
                if data[25] == 0:
                    gray = pixels if pixels.ndim == 2 else pixels[..., 0]
                    return np.count_nonzero(gray == key)
                return np.count_nonzero(np.all(pixels[..., :3] == key[::-1], axis=2))
            cutoff = MAX_ALPHA * 257
        else:
            pixels = rgba8_samples(image, data[24], data[25])
            cutoff = MAX_ALPHA
        alpha = pixels[..., 3]
        return np.count_nonzero((alpha <= cutoff) & np.any(pixels != 0, axis=2))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("files", nargs="*", type=Path, help="default: all Git-tracked PNGs")
    args = parser.parse_args()
    paths = args.files
    if not paths:
        names = subprocess.check_output(
            ["git", "ls-files", "-z", "--cached", "--deduplicate", "--", ":(icase)*.png"], cwd=ROOT
        )
        paths = [ROOT / name.decode("utf-8") for name in names.split(b"\0") if name]
        paths = [path for path in paths if path.exists()]
    failed = False
    for path in paths:
        try:
            count = count_invalid(path.read_bytes())
        except (OSError, ValueError, ImportError, struct.error) as error:
            print(f"{path}: {error}", file=sys.stderr)
            failed = True
            continue
        if count:
            print(f"{path}: {count} low-alpha pixels must be transparent black", file=sys.stderr)
            failed = True
    if not failed:
        print(f"PNG alpha check passed ({len(paths)} files).")
    return int(failed)


if __name__ == "__main__":
    sys.exit(main())
