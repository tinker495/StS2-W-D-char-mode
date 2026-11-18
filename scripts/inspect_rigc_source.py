"""Measure a static rig source PNG before selecting pixels for comparison."""

import argparse
from io import BytesIO
from pathlib import Path
import sys

import numpy as np
from PIL import Image

from check_png_alpha import MAX_ALPHA
from normalize_png_alpha import rgba_samples


def inspect_source(path: Path, opaque_alpha: int) -> None:
    data = path.read_bytes()
    with Image.open(BytesIO(data)) as image:
        if image.format != "PNG" or image.is_animated:
            raise ValueError("expected a single-frame PNG painting")

    alpha = rgba_samples(data)[..., 3]
    height, width = alpha.shape
    maximum = int(np.iinfo(alpha.dtype).max)
    unit = maximum // 255
    low_cutoff = MAX_ALPHA * unit
    selected = int(np.count_nonzero(alpha >= opaque_alpha * unit))

    print(f"{path}: {width} x {height} ({maximum.bit_length()}-bit alpha, max sample {alpha.max()}/{maximum})")
    print(f"  transparent (alpha = 0): {np.count_nonzero(alpha == 0):,}")
    print(f"  low alpha (0 < alpha <= {MAX_ALPHA}): {np.count_nonzero((0 < alpha) & (alpha <= low_cutoff)):,}")
    print(f"  partial alpha ({MAX_ALPHA} < alpha < 255): {np.count_nonzero((low_cutoff < alpha) & (alpha < maximum)):,}")
    print(f"  full alpha (alpha = 255): {np.count_nonzero(alpha == maximum):,}")
    print(f"  comparison (alpha >= {opaque_alpha}): {selected:,} pixels", flush=True)
    if selected == 0:
        raise ValueError(f"no comparison pixels at alpha >= {opaque_alpha}; inspect the source and choose its cutoff")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("--opaque-alpha", type=int, default=255, metavar="1..255",
                        help="minimum alpha on the 8-bit scale used by the comparison (default: 255)")
    args = parser.parse_args()
    if not 1 <= args.opaque_alpha <= 255:
        parser.error("--opaque-alpha must be in 1..255")
    try:
        inspect_source(args.source, args.opaque_alpha)
    except (OSError, ValueError, ImportError) as error:
        print(f"{args.source}: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
