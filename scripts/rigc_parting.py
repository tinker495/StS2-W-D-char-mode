from __future__ import annotations

from collections.abc import Callable, Mapping, Sequence
import json
from pathlib import Path
from tempfile import mkdtemp

import cv2
import numpy as np
from PIL import Image
from scipy import ndimage
from skimage.segmentation import watershed


def align_inpaint_plate(
    base_arr: np.ndarray,
    raw_path: Path,
    crop_box: tuple[int, int, int, int],
    exclude_mask_local: np.ndarray,
) -> np.ndarray:
    """Extract alpha from raw #FFFFFF distance before color-bias compensation so bias never creates gray haze on the background."""
    x0, y0, x1, y1 = crop_box
    cw, ch = x1 - x0, y1 - y0
    ref_rgba = base_arr[y0:y1, x0:x1]
    a = ref_rgba[:, :, 3:4].astype(np.float32) / 255.0
    ref_rgb = ref_rgba[:, :, :3].astype(np.float32) * a + 255.0 * (1.0 - a)
    ref_gray = cv2.cvtColor(np.clip(ref_rgb, 0, 255).astype(np.uint8), cv2.COLOR_RGB2GRAY)

    with Image.open(raw_path) as plate:
        im = np.array(plate.convert("RGB").resize((cw, ch), Image.Resampling.LANCZOS))
    g = cv2.cvtColor(im, cv2.COLOR_RGB2GRAY)

    lm_mask = ((ref_rgba[:, :, 3] > 25) & (~exclude_mask_local)).astype(np.uint8) * 255
    warp = np.eye(2, 3, dtype=np.float32)
    criteria = (cv2.TERM_CRITERIA_EPS | cv2.TERM_CRITERIA_COUNT, 200, 1e-6)
    cc, warp = cv2.findTransformECC(ref_gray, g, warp, cv2.MOTION_AFFINE, criteria, lm_mask)
    al_raw = cv2.warpAffine(
        im,
        warp,
        (cw, ch),
        flags=cv2.INTER_LANCZOS4 + cv2.WARP_INVERSE_MAP,
        borderValue=(255, 255, 255),
    ).astype(np.float32)
    print(f"Aligned {raw_path.name}: ECC={cc:.5f}, dx={warp[0,2]:+.2f}, dy={warp[1,2]:+.2f}")

    dist_white = np.linalg.norm(255.0 - al_raw, axis=2)
    alpha_f = np.clip((dist_white - 18.0) / 34.0, 0.0, 1.0)
    alpha_u8 = np.clip(np.round(alpha_f * 255.0), 0, 255).astype(np.uint8)
    alpha_u8[alpha_u8 < 16] = 0

    al = al_raw.copy()
    valid = (ref_rgba[:, :, 3] >= 220) & (~exclude_mask_local) & (np.abs(al_raw - ref_rgb).max(axis=2) < 30.0)
    if valid.sum() >= 50:
        w = valid.astype(np.float32)
        diff = (ref_rgb - al_raw) * w[:, :, None]
        den = ndimage.gaussian_filter(w, sigma=18.0) + 1e-5
        bias = np.empty_like(al_raw)
        for c in range(3):
            bias[:, :, c] = ndimage.gaussian_filter(diff[:, :, c], sigma=18.0) / den
        al = np.clip(al_raw + bias, 0.0, 255.0)

    fringe = (alpha_f > 0.04) & (alpha_f < 0.96) & (alpha_u8 > 0)
    rgb_out = al.copy()
    if fringe.any():
        core = alpha_f >= 0.72
        if core.any():
            _, nn = ndimage.distance_transform_edt(~core, return_indices=True)
            nearest = al[nn[0], nn[1]]
            a3 = np.clip(alpha_f[:, :, None], 0.25, 1.0)
            unmix = np.clip((al - (1.0 - alpha_f[:, :, None]) * 255.0) / a3, 0.0, 255.0)
            conf = np.clip((alpha_f - 0.15) / 0.65, 0.0, 1.0)[:, :, None]
            rgb_out[fringe] = (conf * unmix + (1.0 - conf) * nearest)[fringe]

    full = np.zeros(base_arr.shape, dtype=np.uint8)
    full[y0:y1, x0:x1, :3] = np.clip(np.round(rgb_out), 0, 255).astype(np.uint8)
    full[y0:y1, x0:x1, 3] = alpha_u8
    full[full[:, :, 3] == 0, :3] = 0
    return full


def refine_visible_ownership(
    base_arr: np.ndarray,
    coarse_masks: Mapping[str, np.ndarray],
    part_order: Sequence[str],
) -> tuple[dict[str, np.ndarray], np.ndarray]:
    """Parts later in part_order take priority where coarse masks overlap."""
    fg = base_arr[:, :, 3] > 0
    lab = cv2.cvtColor(base_arr[:, :, :3], cv2.COLOR_RGB2LAB).astype(np.float32)
    gx = cv2.Sobel(lab, cv2.CV_32F, 1, 0, ksize=3)
    gy = cv2.Sobel(lab, cv2.CV_32F, 0, 1, ksize=3)
    grad = np.sqrt(np.sum(gx * gx + gy * gy, axis=2))
    luma = lab[:, :, 0]
    seam_potential = grad + np.clip(65.0 - luma, 0.0, 65.0) * 1.4

    coarse_id = np.zeros(fg.shape, dtype=np.int32)
    for idx, name in enumerate(part_order, start=1):
        m = coarse_masks[name] & fg
        coarse_id[m] = idx

    unassigned = fg & (coarse_id == 0)
    if unassigned.any():
        assigned = coarse_id > 0
        if not assigned.any():
            raise ValueError("Coarse masks must cover at least one foreground pixel.")
        _, nn = ndimage.distance_transform_edt(~assigned, return_indices=True)
        coarse_id[unassigned] = coarse_id[nn[0], nn[1]][unassigned]
        print(f"Assigned {int(unassigned.sum())} uncovered edge pixels to nearest neighbor part.")

    id_max = ndimage.maximum_filter(coarse_id, size=3)
    id_min = ndimage.minimum_filter(coarse_id, size=3)
    seam_1px = fg & (id_max != id_min) & (id_min > 0)

    seam_band = ndimage.binary_dilation(seam_1px, iterations=3) & fg
    markers = np.where(seam_band, 0, coarse_id)
    for idx in range(1, len(part_order) + 1):
        if (coarse_id == idx).any() and not (markers == idx).any():
            markers[coarse_id == idx] = idx

    ws = watershed(seam_potential, markers=markers, mask=fg)
    rem = fg & (ws == 0)
    if rem.any():
        ws[rem] = coarse_id[rem]

    owners = {name: (ws == idx) for idx, name in enumerate(part_order, start=1)}
    return owners, ws


def paint_anatomical_cap(
    part_rgba: np.ndarray,
    cap_region: np.ndarray,
    sample_region: np.ndarray,
    allowed_hidden: np.ndarray,
    smooth_sigma: float = 3.5,
    extra_sample_filter: np.ndarray | None = None,
) -> np.ndarray:
    out = part_rgba.copy()
    valid_src = (part_rgba[:, :, 3] >= 220) & sample_region
    if extra_sample_filter is not None:
        valid_src = valid_src & extra_sample_filter
    if not valid_src.any():
        return out
    eff_cap = cap_region & allowed_hidden
    target = eff_cap & (part_rgba[:, :, 3] < 240)
    if not target.any():
        return out
    _, nn = ndimage.distance_transform_edt(~valid_src, return_indices=True)
    nearest_rgb = part_rgba[nn[0], nn[1], :3].astype(np.float32)
    blurred = cv2.GaussianBlur(nearest_rgb, (0, 0), sigmaX=smooth_sigma, sigmaY=smooth_sigma)
    dist_in = ndimage.distance_transform_edt(eff_cap)
    cap_alpha = np.clip(dist_in * 180.0, 0, 255).astype(np.uint8)
    cap_alpha[dist_in >= 1.5] = 255

    write_m = target & (cap_alpha > out[:, :, 3])
    out[write_m, :3] = np.clip(np.round(blurred[write_m]), 0, 255).astype(np.uint8)
    out[write_m, 3] = cap_alpha[write_m]
    return out


def blend_inpaint_into_part(
    part_rgba: np.ndarray,
    inpaint_rgba: np.ndarray,
    fill_mask: np.ndarray,
    allowed_hidden: np.ndarray,
    color_filter: np.ndarray | None = None,
    min_inp_alpha: int = 16,
) -> np.ndarray:
    out = part_rgba.copy()
    existing_opaque = out[:, :, 3] >= 245
    inp_valid = inpaint_rgba[:, :, 3] >= min_inp_alpha
    if color_filter is not None:
        inp_valid = inp_valid & color_filter
    eff_fill = fill_mask & allowed_hidden
    target = eff_fill & (~existing_opaque) & inp_valid
    if not target.any():
        return out

    seam_ring = ndimage.binary_dilation(target, iterations=4) & existing_opaque & (inpaint_rgba[:, :, 3] >= 200)
    inp_rgb = inpaint_rgba[:, :, :3].astype(np.float32)
    if seam_ring.sum() >= 15:
        w = seam_ring.astype(np.float32)
        diff = (out[:, :, :3].astype(np.float32) - inp_rgb) * w[:, :, None]
        den = ndimage.gaussian_filter(w, sigma=14.0) + 1e-5
        bias = np.empty_like(inp_rgb)
        for c in range(3):
            bias[:, :, c] = ndimage.gaussian_filter(diff[:, :, c], sigma=14.0) / den
        inp_rgb = np.clip(inp_rgb + bias, 0.0, 255.0)

    dist_in = ndimage.distance_transform_edt(eff_fill)
    mask_a = np.where(dist_in >= 1.5, 255, np.clip(dist_in * 180.0, 0, 255)).astype(np.uint8)
    use_a = np.minimum(inpaint_rgba[:, :, 3], mask_a)

    write_m = target & (use_a > out[:, :, 3])
    out[write_m, :3] = np.clip(np.round(inp_rgb[write_m]), 0, 255).astype(np.uint8)
    out[write_m, 3] = use_a[write_m]
    return out


def write_debug_roi(
    out_dir: Path,
    box: tuple[int, int, int, int],
    images: Mapping[str, np.ndarray],
    classify_materials: Callable[[str, np.ndarray], Mapping[str, np.ndarray]],
) -> Path:
    x0, y0, x1, y1 = box
    if not (0 <= x0 < x1 and 0 <= y0 < y1):
        raise ValueError(f"Invalid ROI bounds: {box}")
    for name, rgba in images.items():
        if x1 > rgba.shape[1] or y1 > rgba.shape[0]:
            raise ValueError(f"ROI {box} extends outside {name}")

    out_dir.mkdir(parents=True, exist_ok=True)
    dump_dir = Path(mkdtemp(prefix=f"roi-{x0}-{y0}-{x1}-{y1}-", dir=out_dir))
    report = {}
    for name, rgba in images.items():
        crop = rgba[y0:y1, x0:x1]
        visible_pixels = int(np.count_nonzero(crop[:, :, 3]))
        if visible_pixels == 0:
            continue
        Image.fromarray(crop).save(dump_dir / f"{name}.png")
        counts = {}
        for material, mask in classify_materials(name, crop).items():
            count = int(np.count_nonzero(mask))
            counts[material] = count
            if count:
                Image.fromarray(mask.astype(np.uint8) * 255).save(dump_dir / f"{name}.{material}.png")
        report[name] = {"nonzero_alpha": visible_pixels, "materials": counts}
    (dump_dir / "materials.json").write_text(
        json.dumps({"box": box, "images": report}, indent=2) + "\n",
        encoding="utf-8",
    )
    return dump_dir
