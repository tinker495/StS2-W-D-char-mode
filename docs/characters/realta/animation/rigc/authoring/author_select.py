"""Author Realta's character-select Spine overlay rig (single 'animation' loop for NSpineAutoPlayer).

Pairs with the full-frame 1920x1080 char_select_bg.png background TextureRect so only
the swaying red glaive ribbon, narrow blade-bevel shimmer, blink overlay, and arcane motes
are packed into a single minimal 512x512 Spine atlas with zero border seams.
"""

from __future__ import annotations

import argparse
import json
import math
from pathlib import Path

import cv2
import numpy as np
from PIL import Image

WIDTH = 1920
HEIGHT = 1080
DURATION = 8.0
BLINK_START = 3.10
BLINK_END = 3.19
BLINK_OFFSET = (1287, 334)
# Tight blade-bevel-only shimmer strip (excludes gloved fingers and wide outer background wisps).
GLOW_RECT = (836, 16, 116, 288)
MOTE_SIZE = 24
MOTE_COUNT = 5

RIBBON_UPPER = [
    (1005.0, 632.0), (947.0, 728.0), (861.0, 800.0), (817.0, 830.0),
    (789.0, 806.0), (750.0, 801.0), (830.0, 749.0), (913.0, 718.0),
]
RIBBON_LOWER = [
    (1016.0, 645.0), (988.0, 756.0), (970.0, 837.0), (914.0, 944.0),
    (898.0, 967.0), (867.0, 930.0), (804.0, 915.0), (895.0, 849.0), (931.0, 774.0),
]

CLOTH_PATCHES = {
    "ribbon_patch": {
        "rect": (742, 624, 280, 352),
        "bone": "ribbon_sway",
        "pivot": (900.0, 800.0),
        "polys": [(RIBBON_UPPER, 14.0, 28.0), (RIBBON_LOWER, 14.0, 28.0)],
        "cell": 20,
    },
}

EASINGS = {
    "ease": [0.36, 0.0, 0.64, 1.0],
}


def smoothstep(edge0: float, edge1: float, x: float) -> float:
    t = max(0.0, min(1.0, (x - edge0) / (edge1 - edge0)))
    return t * t * (3.0 - 2.0 * t)


def smoothstep_array(e0: float, e1: float, arr: np.ndarray) -> np.ndarray:
    t = np.clip((arr - e0) / (e1 - e0), 0.0, 1.0)
    return t * t * (3.0 - 2.0 * t)


def point_in_poly(px: float, py: float, poly: list[tuple[float, float]]) -> bool:
    inside = False
    n = len(poly)
    j = n - 1
    for i in range(n):
        xi, yi = poly[i]
        xj, yj = poly[j]
        if ((yi > py) != (yj > py)) and (px < (xj - xi) * (py - yi) / (yj - yi + 1e-12) + xi):
            inside = not inside
        j = i
    return inside


def dist_to_segment(px: float, py: float, ax: float, ay: float, bx: float, by: float) -> float:
    vx, vy = bx - ax, by - ay
    wx, wy = px - ax, py - ay
    denom = vx * vx + vy * vy
    if denom <= 1e-12:
        return math.hypot(px - ax, py - ay)
    t = max(0.0, min(1.0, (wx * vx + wy * vy) / denom))
    cx, cy = ax + t * vx, ay + t * vy
    return math.hypot(px - cx, py - cy)


def cloth_weight(px: float, py: float, poly: list[tuple[float, float]], inset: float, falloff: float) -> float:
    if not point_in_poly(px, py, poly):
        return 0.0
    d = min(
        dist_to_segment(px, py, poly[i][0], poly[i][1], poly[(i + 1) % len(poly)][0], poly[(i + 1) % len(poly)][1])
        for i in range(len(poly))
    )
    return smoothstep(inset, falloff, d)


def _clean_low_alpha(rgba: np.ndarray) -> np.ndarray:
    rgba[rgba[..., 3] <= 8] = 0
    return rgba


def extract_select_parts(master_png: Path, blink_png: Path, out_dir: Path) -> None:
    """Extract ribbon_patch, narrow blade_glow, blink_patch, and mote PNGs."""
    out_dir.mkdir(parents=True, exist_ok=True)
    with Image.open(master_png) as m_im:
        master_rgba = m_im.convert("RGBA")
        if master_rgba.size != (WIDTH, HEIGHT):
            raise ValueError(f"Expected {WIDTH}x{HEIGHT} select master, got {master_rgba.size}")
        master_np = np.asarray(master_rgba, dtype=np.uint8)

        # 1. Soft-feathered ribbon patch: alpha ramps 0 -> 1 inside the motionless 0..14px border ring.
        for pname, spec in CLOTH_PATCHES.items():
            rx, ry, rw, rh = spec["rect"]
            crop = master_np[ry : ry + rh, rx : rx + rw].copy()
            poly_mask = np.zeros((rh, rw), dtype=np.uint8)
            for poly, _inset, _falloff in spec["polys"]:
                pts = np.array([[round(px - rx), round(py - ry)] for px, py in poly], dtype=np.int32)
                cv2.fillPoly(poly_mask, [pts], 255)
            dist_in = cv2.distanceTransform(poly_mask, cv2.DIST_L2, 5)
            alpha_f = smoothstep_array(1.0, 13.0, dist_in)
            alpha_f[:2, :] = 0.0
            alpha_f[-2:, :] = 0.0
            alpha_f[:, :2] = 0.0
            alpha_f[:, -2:] = 0.0
            crop[..., 3] = np.clip(np.round(alpha_f * 255.0), 0, 255).astype(np.uint8)
            _clean_low_alpha(crop)
            Image.fromarray(crop).save(out_dir / f"{pname}.png", format="PNG")

        # 2. Compact blade-bevel shimmer strip (strictly along the bright steel bevel, zero finger highlights).
        arr = master_np.astype(np.float32) / 255.0
        gx, gy, gw, gh = GLOW_RECT
        sub = arr[gy : gy + gh, gx : gx + gw]
        yy, xx = np.mgrid[0:gh, 0:gw]
        # Normalized local coords with soft inward fade on all 4 sides and finger-side exclusion on lower-left.
        edge_fade = (
            smoothstep_array(2.0, 18.0, xx.astype(np.float32))
            * smoothstep_array(2.0, 18.0, (gw - 1 - xx).astype(np.float32))
            * smoothstep_array(2.0, 20.0, yy.astype(np.float32))
            * smoothstep_array(2.0, 24.0, (gh - 1 - yy).astype(np.float32))
        )
        finger_guard = smoothstep_array(0.0, 14.0, (xx - 18.0) + (gh - yy) * 0.18)
        bright_bevel = smoothstep_array(0.68, 0.90, 0.25 * sub[..., 0] + 0.40 * sub[..., 1] + 0.35 * sub[..., 2])
        cyan_core = smoothstep_array(0.14, 0.34, np.minimum(sub[..., 1] - sub[..., 0], sub[..., 2] - sub[..., 0]))
        raw_mask = np.clip(np.maximum(bright_bevel, cyan_core * 0.65) * edge_fade * finger_guard, 0.0, 1.0)
        soft_mask = cv2.GaussianBlur(raw_mask.astype(np.float32), (0, 0), 3.0)
        soft_mask[:2, :] = 0.0
        soft_mask[-2:, :] = 0.0
        soft_mask[:, :2] = 0.0
        soft_mask[:, -2:] = 0.0
        glow_rgba = np.zeros((gh, gw, 4), dtype=np.uint8)
        glow_rgba[..., 0] = np.clip(np.round((sub[..., 0] * 0.55 + 0.35) * 255.0), 0, 255).astype(np.uint8)
        glow_rgba[..., 1] = np.clip(np.round((sub[..., 1] * 0.45 + 0.52) * 255.0), 0, 255).astype(np.uint8)
        glow_rgba[..., 2] = 255
        glow_rgba[..., 3] = np.clip(np.round(soft_mask * 255.0), 0, 255).astype(np.uint8)
        _clean_low_alpha(glow_rgba)
        Image.fromarray(glow_rgba).save(out_dir / "blade_glow.png", format="PNG")

    # 3. Blink patch: 2px feathered border over motionless face.
    with Image.open(blink_png) as b_im:
        b_np = np.asarray(b_im.convert("RGBA"), dtype=np.uint8).copy()
        bh, bw = b_np.shape[:2]
        yy, xx = np.mgrid[0:bh, 0:bw]
        edge_d = np.minimum(np.minimum(xx, bw - 1 - xx), np.minimum(yy, bh - 1 - yy)).astype(np.float32)
        b_alpha = np.clip(
            np.round((b_np[..., 3].astype(np.float32) / 255.0) * smoothstep_array(0.0, 3.0, edge_d) * 255.0),
            0,
            255,
        ).astype(np.uint8)
        b_np[..., 3] = b_alpha
        _clean_low_alpha(b_np)
        Image.fromarray(b_np).save(out_dir / "blink_patch.png", format="PNG")

    # 4. Compact 24x24 arcane mote
    yy, xx = np.mgrid[0:MOTE_SIZE, 0:MOTE_SIZE]
    center = (MOTE_SIZE - 1) * 0.5
    dist = np.hypot(xx - center, yy - center)
    halo = np.clip(1.0 - smoothstep_array(4.2, 6.8, dist), 0.0, 1.0) * 0.16
    core = np.clip(1.0 - smoothstep_array(1.6, 3.0, dist), 0.0, 1.0)
    alpha = np.clip(halo + core, 0.0, 1.0)
    core_ratio = np.divide(core, np.maximum(alpha, 1e-6))
    mote_rgba = np.zeros((MOTE_SIZE, MOTE_SIZE, 4), dtype=np.uint8)
    mote_rgba[..., 0] = np.clip(np.round((0.30 * (1.0 - core_ratio) + 0.72 * core_ratio) * 255.0), 0, 255).astype(np.uint8)
    mote_rgba[..., 1] = np.clip(np.round((0.88 * (1.0 - core_ratio) + 0.98 * core_ratio) * 255.0), 0, 255).astype(np.uint8)
    mote_rgba[..., 2] = 255
    mote_rgba[..., 3] = np.clip(np.round(alpha * 255.0), 0, 255).astype(np.uint8)
    _clean_low_alpha(mote_rgba)
    Image.fromarray(mote_rgba).save(out_dir / "mote.png", format="PNG")


def sx(x: float) -> float:
    return round(x - WIDTH * 0.5, 6)


def sy(y: float) -> float:
    return round(HEIGHT - y, 6)


def build_patch_mesh(pname: str, spec: dict) -> dict:
    rx, ry, rw, rh = spec["rect"]
    cell = spec["cell"]
    bname = spec["bone"]
    bx, by = sx(spec["pivot"][0]), sy(spec["pivot"][1])
    cols = max(2, math.ceil(rw / cell))
    rows = max(2, math.ceil(rh / cell))
    xs = [rx + rw * ix / cols for ix in range(cols + 1)]
    ys = [ry + rh * iy / rows for iy in range(rows + 1)]

    n_cols = len(xs)
    n_rows = len(ys)
    uvs: list[float] = []
    weights: list[list[dict]] = []

    for y in ys:
        for x in xs:
            uvs.extend([round((x - rx) / rw, 7), round((y - ry) / rh, 7)])
            w_move = max(
                cloth_weight(x, y, poly, ins, fall)
                for poly, ins, fall in spec["polys"]
            )
            w_move = max(0.0, min(1.0, w_move))
            w_root = 1.0 - w_move
            wx, wy = sx(x), sy(y)
            v_binds = []
            if w_root > 1e-4:
                v_binds.append({"bone": "root", "x": wx, "y": wy, "weight": round(w_root, 6)})
            if w_move > 1e-4:
                v_binds.append({"bone": bname, "x": round(wx - bx, 6), "y": round(wy - by, 6), "weight": round(w_move, 6)})
            s = sum(b["weight"] for b in v_binds)
            v_binds[0]["weight"] = round(v_binds[0]["weight"] + (1.0 - s), 6)
            weights.append(v_binds)

    perim: list[int] = []
    for ix in range(n_cols):
        perim.append(ix)
    for iy in range(1, n_rows):
        perim.append(iy * n_cols + (n_cols - 1))
    for ix in range(n_cols - 2, -1, -1):
        perim.append((n_rows - 1) * n_cols + ix)
    for iy in range(n_rows - 2, 0, -1):
        perim.append(iy * n_cols)

    perim_set = set(perim)
    interior = [i for i in range(n_rows * n_cols) if i not in perim_set]
    new_order = perim + interior
    remap = {old_i: new_i for new_i, old_i in enumerate(new_order)}

    r_uvs: list[float] = []
    r_weights: list[list[dict]] = []
    for old_i in new_order:
        r_uvs.extend([uvs[2 * old_i], uvs[2 * old_i + 1]])
        r_weights.append(weights[old_i])

    tris: list[int] = []
    for iy in range(n_rows - 1):
        for ix in range(n_cols - 1):
            a = remap[iy * n_cols + ix]
            b = remap[iy * n_cols + ix + 1]
            c = remap[(iy + 1) * n_cols + ix + 1]
            d = remap[(iy + 1) * n_cols + ix]
            tris.extend([a, d, c, a, c, b])

    return {
        "image": f"{pname}.png",
        "type": "mesh",
        "uvs": r_uvs,
        "weights": r_weights,
        "triangles": tris,
        "hull": len(perim),
    }


def author_select(images_dir: Path, out_dir: Path) -> None:
    with Image.open(images_dir / "blink_patch.png") as bp:
        bw, bh = bp.size

    bones = [
        {"name": "root", "x": 0.0, "y": 0.0},
        {"name": "ribbon_sway", "parent": "root", "x": sx(900.0), "y": sy(800.0), "length": 60.0},
    ]
    for i in range(MOTE_COUNT):
        bones.append({"name": f"mote_{i}", "parent": "root", "x": sx(924.0), "y": sy(310.0)})

    gx, gy, gw, gh = GLOW_RECT
    bx, by = BLINK_OFFSET

    slots = []
    skin = {}
    max_tris = 0
    for pname, spec in CLOTH_PATCHES.items():
        slots.append({"name": pname, "bone": "root", "attachment": pname})
        mesh = build_patch_mesh(pname, spec)
        max_tris = max(max_tris, len(mesh["triangles"]) // 3)
        skin[pname] = {pname: mesh}

    slots.extend([
        {"name": "blade_glow", "bone": "root", "attachment": "blade_glow", "blend": "additive", "color": "ffffff0d"},
        {"name": "blink_patch", "bone": "root", "attachment": None},
    ])
    skin["blade_glow"] = {
        "blade_glow": {
            "image": "blade_glow.png",
            "x": sx(gx + gw * 0.5),
            "y": sy(gy + gh * 0.5),
        }
    }
    skin["blink_patch"] = {
        "blink_patch": {
            "image": "blink_patch.png",
            "x": sx(bx + bw * 0.5),
            "y": sy(by + bh * 0.5),
        }
    }
    for i in range(MOTE_COUNT):
        mname = f"mote_{i}"
        slots.append({"name": mname, "bone": mname, "attachment": "mote", "blend": "additive", "color": "ffffff00"})
        skin[mname] = {"mote": {"image": "mote.png", "x": 0.0, "y": 0.0}}

    rig = {
        "spec": "rigc-rig/1",
        "name": "realta_select",
        "images": "parts",
        "skeleton": {"x": -WIDTH * 0.5, "y": 0, "width": WIDTH, "height": HEIGHT},
        "bones": bones,
        "slots": slots,
        "skins": {"default": skin},
        "invariants": {"meshSlots": len(CLOTH_PATCHES), "meshTriangles": max_tris},
    }

    tracks = []
    steps = 32
    ribbon_keys, glow_keys = [], []
    for s in range(steps + 1):
        t = round(DURATION * s / steps, 5)
        phase = 2.0 * math.pi * s / steps
        r_val = math.sin(phase - 0.65) * 3.5
        g_val = 0.07 + 0.055 * math.sin(phase * 2.0 - 0.25)
        ease_kw = {"ease": "ease"} if s < steps else {}
        ribbon_keys.append({"t": t, "v": [round(-0.4 * r_val, 5), round(-r_val, 5)], **ease_kw})
        glow_keys.append({"t": t, "v": [round(g_val, 5)], **ease_kw})

    tracks.append({"bone": "ribbon_sway", "property": "translate", "keys": ribbon_keys})
    tracks.append({"slot": "blade_glow", "property": "alpha", "keys": glow_keys})
    tracks.append({
        "slot": "blink_patch",
        "property": "attachment",
        "keys": [
            {"t": 0.0, "v": None},
            {"t": BLINK_START, "v": "blink_patch"},
            {"t": BLINK_END, "v": None},
            {"t": DURATION, "v": None},
        ],
    })

    mote_base_x, mote_base_y = sx(924.0), sy(310.0)
    for i in range(MOTE_COUNT):
        offset_phase = float(i) / MOTE_COUNT
        wrap_times = []
        for k in range(3):
            wt = 4.0 * (k - offset_phase)
            if 0.0 < wt < DURATION:
                wrap_times.append(wt)
        sample_ts = {round(DURATION * j / 64.0, 5) for j in range(65)}
        for wt in wrap_times:
            sample_ts.discard(round(wt, 5))
            if wt - 0.002 > 0.0:
                sample_ts.add(round(wt - 0.002, 5))
            sample_ts.add(round(wt, 5))
            if wt + 0.002 < DURATION:
                sample_ts.add(round(wt + 0.002, 5))

        sorted_ts = sorted(sample_ts)
        pos_keys, alpha_keys = [], []
        for idx, t in enumerate(sorted_ts):
            is_pre_wrap = any(abs(t - (wt - 0.002)) < 1e-4 for wt in wrap_times)
            is_at_wrap = any(abs(t - wt) < 1e-4 for wt in wrap_times)
            if is_pre_wrap:
                age = 1.0
            elif is_at_wrap:
                age = 0.0
            else:
                age = ((t / 4.0) + offset_phase) % 1.0
            opacity = math.sin(math.pi * age) * 0.38
            px = sx(924.0 + 39.0 * math.sin(age * 2.0 * math.pi + i * 0.8)) - mote_base_x
            py = sy(310.0 - 250.0 * age) - mote_base_y
            next_is_wrap = idx + 1 < len(sorted_ts) and any(abs(sorted_ts[idx + 1] - wt) < 1e-4 for wt in wrap_times)
            pos_entry = {"t": t, "v": [round(px, 4), round(py, 4)]}
            alpha_entry = {"t": t, "v": [round(max(0.0, opacity), 5)]}
            if idx < len(sorted_ts) - 1 and next_is_wrap:
                pos_entry["ease"] = "stepped"
            pos_keys.append(pos_entry)
            alpha_keys.append(alpha_entry)

        tracks.append({"bone": f"mote_{i}", "property": "translate", "keys": pos_keys})
        tracks.append({"slot": f"mote_{i}", "property": "alpha", "keys": alpha_keys})

    motion_doc = {
        "spec": "rigc-motion/1",
        "archetype": "realta_select",
        "cut": "realta_select",
        "easings": EASINGS,
        "animations": {
            "animation": {
                "duration": DURATION,
                "loop": True,
                "tracks": tracks,
            }
        },
    }

    out_dir.mkdir(parents=True, exist_ok=True)
    (out_dir / "realta_select.rig.json").write_text(
        json.dumps(rig, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n"
    )
    (out_dir / "realta_select.motion.json").write_text(
        json.dumps(motion_doc, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n"
    )


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--images", type=Path, required=True, help="Directory with select overlay part PNGs")
    parser.add_argument("--out", type=Path, required=True, help="Output directory for rigc JSON specs")
    parser.add_argument("--seed-from-master", type=Path, default=None, help="Optional 1920x1080 master PNG to extract parts from")
    parser.add_argument("--seed-from-blink", type=Path, default=None, help="Optional blink patch PNG to extract when seeding")
    args = parser.parse_args()
    if args.seed_from_master and args.seed_from_blink:
        extract_select_parts(args.seed_from_master, args.seed_from_blink, args.images)
    author_select(args.images, args.out)


if __name__ == "__main__":
    main()
