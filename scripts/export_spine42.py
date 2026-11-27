"""Convert compiled rig-c (Spine 4.3) outputs into StS2 native Spine 4.2 (.spjson, .spatlas, 4px-aligned PNG)."""

from __future__ import annotations

import argparse
import copy
import json
from pathlib import Path
import re

import numpy as np
from PIL import Image

TARGET_SPINE_VERSION = "4.2.43"
SIZE_LINE_RE = re.compile(r"^(\s*size:\s*)(\d+)\s*,\s*(\d+)(\s*)$")


def _convert_ik_constraint(entry: dict, order: int) -> dict:
    out = {k: copy.deepcopy(v) for k, v in entry.items() if k not in ("type", "scaleY")}
    out["order"] = order
    scale_y = entry.get("scaleY")
    if scale_y is not None and str(scale_y).lower() == "uniform":
        out["uniform"] = True
    return out


def _convert_transform_constraint(entry: dict, order: int) -> dict:
    if "properties" in entry:
        raise ValueError(
            f"Transform constraint {entry.get('name')!r} uses Spine 4.3 cross-property 'properties' mapping, "
            "which has no direct Spine 4.2 equivalent."
        )
    out = {}
    for k, v in entry.items():
        if k == "type":
            continue
        if k == "source":
            out["target"] = v
        elif k in ("localSource", "localTarget"):
            if v:
                out["local"] = True
        elif k == "additive":
            if v:
                out["relative"] = True
        else:
            out[k] = copy.deepcopy(v)
    out["order"] = order
    return out


def _convert_path_constraint(entry: dict, order: int) -> dict:
    out = {}
    for k, v in entry.items():
        if k == "type":
            continue
        if k == "slot":
            out["target"] = v
        else:
            out[k] = copy.deepcopy(v)
    out["order"] = order
    return out


def _convert_physics_constraint(entry: dict, order: int) -> dict:
    out = {k: copy.deepcopy(v) for k, v in entry.items() if k != "type"}
    out["order"] = order
    return out


def convert_skeleton_43_to_42(skel_43: dict) -> dict:
    """Deterministically downgrade a rig-c Spine 4.3 skeleton dict to Spine 4.2.43."""
    skel = copy.deepcopy(skel_43)
    header = dict(skel.get("skeleton") or {})
    declared = str(header.get("spine", ""))
    if not (declared == "4.3" or declared.startswith("4.3.") or declared.startswith("4.2.")):
        raise ValueError(f"Expected Spine 4.3.x or 4.2.x input skeleton, got {declared!r}")
    header["spine"] = TARGET_SPINE_VERSION
    header.pop("images", None)
    header.pop("audio", None)
    skel["skeleton"] = header

    constraints = skel.pop("constraints", None)
    if constraints:
        ik_list: list[dict] = []
        tf_list: list[dict] = []
        path_list: list[dict] = []
        phys_list: list[dict] = []
        for idx, raw in enumerate(constraints):
            ctype = raw.get("type")
            if ctype == "ik":
                ik_list.append(_convert_ik_constraint(raw, idx))
            elif ctype == "transform":
                tf_list.append(_convert_transform_constraint(raw, idx))
            elif ctype == "path":
                path_list.append(_convert_path_constraint(raw, idx))
            elif ctype == "physics":
                phys_list.append(_convert_physics_constraint(raw, idx))
            elif ctype == "slider":
                raise ValueError(
                    f"Constraint {raw.get('name')!r} uses Spine 4.3 'slider', which is unsupported in Spine 4.2."
                )
            else:
                raise ValueError(f"Unknown Spine 4.3 constraint type {ctype!r} on {raw.get('name')!r}")
        if ik_list:
            skel["ik"] = ik_list
        if tf_list:
            skel["transform"] = tf_list
        if path_list:
            skel["path"] = path_list
        if phys_list:
            skel["physics"] = phys_list

    for aname, anim in skel.get("animations", {}).items():
        if "slider" in anim:
            raise ValueError(f"Animation {aname!r} contains Spine 4.3 'slider' timelines.")

    # Preserve canonical Spine top-level key ordering.
    ordered_keys = [
        "skeleton",
        "bones",
        "slots",
        "ik",
        "transform",
        "path",
        "physics",
        "skins",
        "events",
        "animations",
    ]
    result = {}
    for key in ordered_keys:
        if key in skel:
            result[key] = skel[key]
    for key, val in skel.items():
        if key not in result:
            result[key] = val
    return result


def rewrite_atlas_text(atlas_text: str, page_filename: str, new_size: tuple[int, int] | None = None) -> str:
    """Rewrite the first atlas page header filename and optional padded dimensions."""
    lines = atlas_text.replace("\r\n", "\n").replace("\r", "\n").split("\n")
    replaced_page = False
    replaced_size = False
    out_lines: list[str] = []

    for line in lines:
        stripped = line.strip()
        if not replaced_page and stripped and ":" not in stripped:
            out_lines.append(page_filename)
            replaced_page = True
            continue
        if replaced_page and not replaced_size and new_size is not None:
            m = SIZE_LINE_RE.match(line)
            if m:
                out_lines.append(f"{m.group(1)}{new_size[0]}, {new_size[1]}{m.group(4)}")
                replaced_size = True
                continue
        out_lines.append(line)

    if not replaced_page:
        raise ValueError("Atlas text contains no page image header.")
    normalized = "\n".join(out_lines)
    if not normalized.endswith("\n"):
        normalized += "\n"
    return normalized


def copy_and_align_png(src_png: Path, dst_png: Path, align_block_4: bool = True) -> tuple[int, int, bool]:
    """Copy atlas PNG, normalizing low-alpha pixels (<= 8 -> RGBA 0) and padding to 4x4 alignment if needed."""
    with Image.open(src_png) as img:
        rgba = img.convert("RGBA")
        w, h = rgba.size
        target_w = ((w + 3) // 4) * 4 if align_block_4 else w
        target_h = ((h + 3) // 4) * 4 if align_block_4 else h
        arr = np.array(rgba, dtype=np.uint8)
        low_alpha = (arr[:, :, 3] <= 8) & np.any(arr != 0, axis=2)
        had_low_alpha = bool(np.any(low_alpha))
        if had_low_alpha:
            arr[low_alpha] = 0
        dst_png.parent.mkdir(parents=True, exist_ok=True)
        if (target_w, target_h) == (w, h):
            if img.mode == "RGBA" and not had_low_alpha:
                dst_png.write_bytes(src_png.read_bytes())
            else:
                Image.fromarray(arr, mode="RGBA").save(dst_png, format="PNG")
            return w, h, False
        padded = np.zeros((target_h, target_w, 4), dtype=np.uint8)
        padded[:h, :w] = arr
        Image.fromarray(padded, mode="RGBA").save(dst_png, format="PNG")
        return target_w, target_h, True


def export_spine42(
    build_dir: Path,
    out_dir: Path,
    stem: str,
    res_dir: str,
    sync_atlas_txt: Path | None = None,
) -> dict:
    """Export rig-c build/ outputs to <out_dir>/<stem>.{spjson,spatlas,png}."""
    src_json = build_dir / "skeleton.json"
    src_atlas = build_dir / "skeleton.atlas"
    src_png = build_dir / "skeleton.png"
    for req in (src_json, src_atlas, src_png):
        if not req.is_file():
            raise FileNotFoundError(f"Missing compiled rig-c artifact: {req}")

    out_dir.mkdir(parents=True, exist_ok=True)
    png_name = f"{stem}.png"
    dst_png = out_dir / png_name
    dst_spatlas = out_dir / f"{stem}.spatlas"
    dst_spjson = out_dir / f"{stem}.spjson"

    w, h, was_padded = copy_and_align_png(src_png, dst_png, align_block_4=True)
    raw_atlas = src_atlas.read_text(encoding="utf-8")
    if was_padded:
        raise ValueError(
            f"Atlas {src_png} ({w}x{h} after pad) changed size; pack at 4px-aligned dimensions so UVs stay exact."
        )
    rewritten_atlas = rewrite_atlas_text(raw_atlas, png_name, None)

    res_prefix = res_dir.rstrip("/")
    if not res_prefix.startswith("res://"):
        res_prefix = "res://" + res_prefix.lstrip("/")

    spatlas_payload = {
        "atlas_data": rewritten_atlas,
        "normal_texture_prefix": "n",
        "source_path": f"{res_prefix}/{stem}.atlas",
        "specular_texture_prefix": "s",
    }
    dst_spatlas.write_text(
        json.dumps(spatlas_payload, ensure_ascii=False, separators=(",", ":")) + "\n",
        encoding="utf-8",
        newline="\n",
    )

    skel_43 = json.loads(src_json.read_text(encoding="utf-8"))
    skel_42 = convert_skeleton_43_to_42(skel_43)
    dst_spjson.write_text(
        json.dumps(skel_42, ensure_ascii=False, separators=(",", ":")) + "\n",
        encoding="utf-8",
        newline="\n",
    )

    if sync_atlas_txt is not None:
        sync_atlas_txt.parent.mkdir(parents=True, exist_ok=True)
        normalized_src_atlas = raw_atlas.replace("\r\n", "\n").replace("\r", "\n")
        if not normalized_src_atlas.endswith("\n"):
            normalized_src_atlas += "\n"
        sync_atlas_txt.write_text(normalized_src_atlas, encoding="utf-8", newline="\n")

    return {
        "spjson": str(dst_spjson),
        "spatlas": str(dst_spatlas),
        "png": str(dst_png),
        "dimensions": [w, h],
        "animations": sorted(skel_42.get("animations", {}).keys()),
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--build-dir", type=Path, required=True, help="Directory containing skeleton.{json,atlas,png}")
    parser.add_argument("--out-dir", type=Path, required=True, help="Target runtime directory under Sts2WD/animations/")
    parser.add_argument("--stem", type=str, required=True, help="Output filename stem (e.g. realta_skeleton)")
    parser.add_argument(
        "--res-dir",
        type=str,
        required=True,
        help="Godot res:// directory matching --out-dir (e.g. res://Sts2WD/animations/realta/combat)",
    )
    parser.add_argument(
        "--sync-atlas-txt",
        type=Path,
        default=None,
        help="Optional tracked atlas.txt path to sync from skeleton.atlas",
    )
    args = parser.parse_args()
    info = export_spine42(
        build_dir=args.build_dir.resolve(),
        out_dir=args.out_dir.resolve(),
        stem=args.stem,
        res_dir=args.res_dir,
        sync_atlas_txt=args.sync_atlas_txt.resolve() if args.sync_atlas_txt else None,
    )
    print(
        f"Exported Spine 4.2 rig '{args.stem}' ({info['dimensions'][0]}x{info['dimensions'][1]}, "
        f"{len(info['animations'])} clips: {', '.join(info['animations'])}) -> {args.out_dir}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
