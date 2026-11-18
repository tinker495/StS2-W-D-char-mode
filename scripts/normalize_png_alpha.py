"""Preview low-alpha PNG repairs. Pass --apply to write verified changes."""

import argparse
from io import BytesIO
import os
from pathlib import Path
import shutil
import struct
import subprocess
import sys
from tempfile import NamedTemporaryFile
import zlib

import numpy as np
from PIL import Image

from check_png_alpha import MAX_ALPHA, ROOT, count_invalid, png_chunk, rgba8_samples, stored_frames


def chunks(data):
    offset = 8
    while offset < len(data):
        length, kind = struct.unpack_from(">I4s", data, offset)
        end = offset + 12 + length
        if end > len(data):
            raise ValueError("truncated PNG chunk")
        yield kind, data[offset + 8:end - 4]
        offset = end


def rgba_samples(data):
    """Decode a static PNG in straight RGBA, retaining 16-bit precision."""
    with Image.open(BytesIO(data)) as image:
        depth, kind = data[24:26]
        key = image.info.get("transparency")
        if depth != 16:
            return rgba8_samples(image, depth, kind)

        import cv2

        pixels = cv2.imdecode(np.frombuffer(data, np.uint8), cv2.IMREAD_UNCHANGED)
        if pixels is None or pixels.dtype != np.uint16:
            raise ValueError("could not decode 16-bit PNG samples")
        if kind in (4, 6):
            return pixels[..., [2, 1, 0, 3]]
        if kind == 0:
            gray = pixels if pixels.ndim == 2 else pixels[..., 0]
            rgb = np.repeat(gray[..., None], 3, axis=2)
        else:
            rgb = pixels[..., 2::-1]
        alpha = np.full(rgb.shape[:2], 65535, dtype=np.uint16)
        if key is not None:
            alpha[np.all(rgb == key, axis=2)] = 0
        return np.dstack((rgb, alpha))


def low_alpha(pixels):
    return pixels[..., 3] <= MAX_ALPHA * (np.iinfo(pixels.dtype).max // 255)


def tracked_pngs():
    names = subprocess.check_output(
        ["git", "ls-files", "-z", "--cached", "--deduplicate", "--", ":(icase)*.png"], cwd=ROOT
    )
    paths = [ROOT / name.decode("utf-8") for name in names.split(b"\0") if name]
    return [path for path in paths if path.exists()]


def normalized_samples(frame):
    pixels = rgba_samples(frame)
    pixels[low_alpha(pixels)] = 0
    return pixels


def compressed_frame(frame, gray, interlaced):
    pixels = normalized_samples(frame)
    if gray:
        pixels = pixels[..., [0, 3]]
    if pixels.dtype == np.uint8 and not interlaced:
        encoded = BytesIO()
        Image.fromarray(pixels).save(encoded, format="PNG")
        return b"".join(body for kind, body in chunks(encoded.getvalue()) if kind == b"IDAT")

    # Store the native-depth channels in PNG byte order. Each Adam7 pass has its
    # own scanlines; PNG's Sub filter subtracts the preceding pixel bytewise.
    passes = ((0, 0, 1, 1),)
    if interlaced:
        passes = ((0, 0, 8, 8), (4, 0, 8, 8), (0, 4, 4, 8), (2, 0, 4, 4),
                  (0, 2, 2, 4), (1, 0, 2, 2), (0, 1, 1, 2))
    compressor = zlib.compressobj()
    compressed = bytearray()
    for x, y, dx, dy in passes:
        samples = pixels[y::dy, x::dx]
        if not samples.size:
            continue
        raw = np.ascontiguousarray(samples, dtype=pixels.dtype.newbyteorder(">")).view(np.uint8)
        filtered = raw.copy()
        filtered[:, 1:] -= raw[:, :-1]
        rows = np.empty((raw.shape[0], 1 + raw.shape[1] * raw.shape[2]), dtype=np.uint8)
        rows[:, 0] = 1
        rows[:, 1:] = filtered.reshape(raw.shape[0], -1)
        compressed.extend(compressor.compress(rows))
    return bytes(compressed) + compressor.flush()


def normalize(data):
    depth, color, _, _, interlaced = data[24:29]
    if color == 3:
        # Indexed PNGs/APNGs share one palette. Keep frame data and indices intact.
        alphas = next(body for kind, body in chunks(data) if kind == b"tRNS")
        clear = [i for i, alpha in enumerate(alphas) if alpha <= MAX_ALPHA]
        result = bytearray(data[:8])
        for kind, body in chunks(data):
            if kind in (b"PLTE", b"tRNS"):
                body = bytearray(body)
                for i in clear:
                    if kind == b"PLTE":
                        offset = 3 * i
                        body[offset:offset + 3] = bytes(3)
                    else:
                        body[i] = 0
            result.extend(png_chunk(kind, body))
        return result

    # A tRNS color key can collide with an opaque black pixel after repair.
    # Giving keyed pixels an explicit alpha channel preserves both pixels.
    gray = color in (0, 4)
    output_depth = max(depth, 8)
    frames = iter(stored_frames(data))
    first_data = True
    sequence = 0
    result = bytearray(data[:8])
    for kind, body in chunks(data):
        if kind == b"IHDR":
            body = body[:8] + bytes((output_depth, 4 if gray else 6)) + body[10:]
        elif kind == b"tRNS":
            continue
        elif kind == b"sBIT" and color in (0, 2):
            body += bytes([output_depth])
        elif kind == b"bKGD" and depth < 8:
            sample = int.from_bytes(body, "big") * 255 // ((1 << depth) - 1)
            body = struct.pack(">H", sample)
        elif kind == b"fcTL":
            body = struct.pack(">I", sequence) + body[4:]
            sequence += 1
            first_data = True
        elif kind in (b"IDAT", b"fdAT"):
            if not first_data:
                continue
            body = compressed_frame(next(frames), gray, interlaced)
            first_data = False
            if kind == b"fdAT":
                body = struct.pack(">I", sequence) + body
                sequence += 1
        result.extend(png_chunk(kind, body))
    return result


def replace_verified(path, original):
    path = path.resolve(strict=True)
    temporary = None
    try:
        with NamedTemporaryFile(dir=path.parent, delete=False, suffix=".tmp") as output:
            temporary = Path(output.name)
            output.write(normalize(original))
        written = temporary.read_bytes()
        for before, after in zip(stored_frames(original), stored_frames(written), strict=True):
            if not np.array_equal(normalized_samples(before), rgba_samples(after)):
                raise ValueError("pixel verification failed")
        if count_invalid(written):
            raise ValueError("normalized PNG failed the repository check")
        shutil.copymode(path, temporary)
        if path.read_bytes() != original:
            raise ValueError("source changed during normalization")
        os.replace(temporary, path)
    finally:
        if temporary is not None:
            temporary.unlink(missing_ok=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apply", action="store_true", help="atomically replace each verified PNG")
    parser.add_argument("files", nargs="*", type=Path, help="default: all Git-tracked PNGs")
    args = parser.parse_args()
    paths = args.files or tracked_pngs()
    changed = total = signed = 0
    for path in paths:
        try:
            data = path.read_bytes()
            count = count_invalid(data)
            if not count:
                continue
            manifest = any(kind == b"caBX" and b"c2pa" in body for kind, body in chunks(data))
            note = " [C2PA manifest; signed content changes]" if manifest else ""
            print(f"{path}: {count} pixels{note}", flush=True)
            if args.apply:
                replace_verified(path, data)
            changed += 1
            total += count
            signed += int(manifest)
        except (OSError, ValueError, ImportError, struct.error) as error:
            sys.exit(f"{path}: {error}")
    mode = "Applied" if args.apply else "Preview"
    print(f"{mode}: {changed}/{len(paths)} PNGs, {total} pixels, {signed} affected C2PA manifests.")


if __name__ == "__main__":
    main()
