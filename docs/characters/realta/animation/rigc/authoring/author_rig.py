"""Author Realta's Spine rig from trimmed images registered on her standing canvas."""

from __future__ import annotations

import argparse
import json
import math
import re
from pathlib import Path

from PIL import Image
from garment import Garment, HEMS, PANELS, support_garment


FACE_STATE = re.compile(r"^(eye_r|eye_l|mouth)[-_](neutral|closed|hurt|attack|ponder|fallen)$")
EASINGS = {
    "ease": [0.36, 0.0, 0.64, 1.0],
    "push": [0.5, 0.05, 0.82, 0.56],
    "catch": [0.17, 0.58, 0.38, 1.0],
}


def save_json(path, document):
    path.write_text(json.dumps(document, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def rotate(x, y, angle):
    cosine = math.cos(math.radians(angle))
    sine = math.sin(math.radians(angle))
    return x * cosine - y * sine, x * sine + y * cosine


def shortest(degrees):
    return (degrees + 180) % 360 - 180


# The wood is continuous beneath the ribbons. The gold collar ends at this
# center; its cross section is perpendicular to the line between the palms.
WEAPON_SOCKET = (350.5, 575.0)
WEAPON_SLOPE = (433.5 - 395.0) / (1104.0 - 834.0)


def mirror_weapon_head(x, y):
    """Reflect in the original canvas about the longitudinal axis through the collar."""
    offset = ((x - WEAPON_SOCKET[0]) - WEAPON_SLOPE * (y - WEAPON_SOCKET[1])) / (1 + WEAPON_SLOPE ** 2)
    return x - 2 * offset, y + 2 * WEAPON_SLOPE * offset


def weapon_sections(texture, rect, pivot, shaft_angle):
    """Two rigid atlas quads meet under the knot. Only the original steel and gold turn."""
    x0, y0, width, height = rect
    x1, y1 = x0 + width, y0 + height
    cut_l = WEAPON_SOCKET[1] - WEAPON_SLOPE * (x0 - WEAPON_SOCKET[0])
    cut_r = WEAPON_SOCKET[1] - WEAPON_SLOPE * (x1 - WEAPON_SOCKET[0])
    # The wood extends a texel under the socket, like the real joint. Its exposed
    # shaft and entire brush remain the original projection for both hand grips.
    shaft = [(x0, cut_l - 1), (x1, cut_r - 1), (x1, y1), (x0, y1)]
    head = [(x0, y0), (x1, y0), (x1, cut_r), (x0, cut_l)]
    attachments = []
    for section, mirror in ((shaft, False), (head, True)):
        uvs, weights = [], []
        for x, y in section:
            uvs.extend([(x - x0) / width, (y - y0) / height])
            if mirror:
                x, y = mirror_weapon_head(x, y)
            lx, ly = rotate(x - pivot[0], pivot[1] - y, -shaft_angle)
            weights.append([{"bone": "weapon", "x": lx, "y": ly, "weight": 1}])
        attachments.append({"image": texture, "type": "mesh", "uvs": uvs, "weights": weights,
                            "triangles": [0, 1, 2, 0, 2, 3], "hull": 4})
    return attachments


def flexible_binding(item, bones):
    """Describe each flexible material in its painting's pixels, using rigc's segments lattice."""
    if "weights" in item:
        if not item["weights"]:
            return None
        return item["weights"], item.get("cell", 24), item.get("weightRadius", 15)
    x, y, width, height = item["rect"]
    name = item["name"]
    if name == "hat" and "hat_tip" in bones:
        # The felt crown folds left of the leather band. The whole broad brim and its
        # crossing in front of the forehead stay on "hat", including its outer tips.
        rails = [
            ("hat", (444, 611), (980, 446)), ("hat", (570, 502), (778, 413)),
            ("hat", (634, 321), (753, 406)), ("hat", (595, 406), (568, 467)),
            ("hat_tip", (431, 428), (489, 461)), ("hat_tip", (489, 461), (528, 397)),
        ]
        weights = [
            {"bone": binding, "from": [a[0] - x, a[1] - y], "to": [b[0] - x, b[1] - y]}
            for binding, a, b in rails
        ]
        return weights, 23, 14
    # Three short transverse bands keep the top of a material on its support and
    # gradually hand its lower part to the swinging bone. The clothing and its
    # bone can be diagonal on the screen; the bands follow the registered axis.
    if name in ("ribbon_long", "ribbon_short"):
        fractions, cell, radius = (0.0, 0.48, 0.92), 16, 13
    elif name in ("hair_r", "hair_l"):
        fractions, cell, radius = (0.05, 0.47, 0.9), 13, 11
    elif name in ("upper_arm_r", "upper_arm_l"):
        # Their cuff and pauldron cover both joints. Only the very proximal
        # hidden fabric takes a little of the chest's movement on a full reach.
        proximal = -1.68 if name == "upper_arm_r" else -0.48
        fractions, cell, radius = (proximal, 0.05, 0.80), 18, 32
    elif name in ("thigh_r", "thigh_l"):
        fractions, cell, radius = (-0.43, 0.04, 0.86), 19, 26
    else:
        return None
    bone = item["bone"]
    parent = bones[bone]["parent"]
    px, py = bones[bone]["pivot"]
    tx, ty = bones[bone]["tip"]
    dx, dy = tx - px, ty - py
    length = math.hypot(dx, dy)
    if not length:
        raise ValueError(f"Flexible part {name} needs a distinct root and tip")
    # Crossbars span the width of this part's bounding box along the normal.
    nx, ny = -dy / length, dx / length
    half = (abs(nx) * width + abs(ny) * height) / 2
    rails = []
    for binding, fraction in zip((parent, bone, bone), fractions):
        mx, my = px + fraction * dx - x, py + fraction * dy - y
        rails.append({
            "bone": binding,
            "from": [round(mx - nx * half, 5), round(my - ny * half, 5)],
            "to": [round(mx + nx * half, 5), round(my + ny * half, 5)],
        })
    return rails, cell, radius


from wrist_seams import CUFFS, cuff_attachment, register_cuffs

# Mirror the canonical open left glove (`hand_l.png`) across image X (`width_scale < 0`)
# with +23.71 deg compensation so `hand_r`'s open death pose places the thumb on the
# anatomical right-hand inner side. The palm-down ground pose uses its own ulnar-side sprite (`hand_r-ground.png`).
HAND_R_OPEN_SPEC = (-1.08, 1.08, 23.71)  # (width_scale, length_scale, extra_rot_deg)


def wrist_pinned_glove(source, width_scale, length_scale, extra_rot=0.0):
    """Scale and orient a vertical glove attachment while keeping its wrist pivot pinned at (0, 0)."""
    rot0 = source.get("rotation", 0.0)
    u_w, v_w = rotate(-source["x"], -source["y"], -rot0)
    rot = shortest(rot0 + extra_rot)
    dx, dy = rotate(u_w * width_scale, v_w * length_scale, rot)
    return {
        **source,
        "x": round(-dx, 7),
        "y": round(-dy, 7),
        "rotation": round(rot, 7),
        "scaleX": round(width_scale, 7),
        "scaleY": round(length_scale, 7),
    }


# Frame the axial turn around the wrist in the painted attachment. Bone scale
# would also flatten the independently moving weapon parented to this hand.
GLOVE_ROLL = (
    (1.074, "open_turn1", "open", .87), (1.095, "open_turn2", "open", .73),
    (1.115, "open_turn3", "open", .57), (1.134, "open_turn4", "open", .41),
    (1.155, "open_edge", "open", .28), (1.18, "ground_edge", "ground", .48),
    (1.22, "ground_turn1", "ground", .62), (1.25, "ground_turn2", "ground", .74),
    (1.28, "ground_turn3", "ground", .85), (1.31, "ground_turn4", "ground", .94),
)


class CanvasRig:
    """Input uses painted canvas coordinates; derived frames are Spine's y-up world."""

    def __init__(self, layout):
        self.width, self.height = layout["canvas"]
        self.parts = layout["parts"]
        self.garment = Garment(self.parts)
        self.bones = {}
        self.rotation = {}
        for row in layout["bones"]:
            name = row["name"]
            parent = row.get("parent")
            if name in self.bones or (parent and parent not in self.bones):
                raise ValueError(f"Duplicate bone or parent declared after its child: {name}")
            if name in PANELS:
                # All handles compress in garment-width / garment-length space.
                # Their three original images still have different crop origins.
                wx, wy = self.bones["pelvis"]["pivot"]
                row = {**row, "pivot": [wx, wy], "tip": [wx, wy + 480]}
            x, y = row["pivot"]
            tip_x, tip_y = row.get("tip") or [x, y]
            if parent and (x, y) != (tip_x, tip_y):
                angle = math.degrees(math.atan2(y - tip_y, tip_x - x))
            else:
                angle = self.rotation.get(parent, 0.0)
            self.bones[name] = row
            self.rotation[name] = angle
        for name in HEMS:
            wx, wy = self.bones["pelvis"]["pivot"]
            self.bones[name] = {"name": name, "parent": "pelvis", "pivot": [wx,wy], "tip": [wx,wy+480]}
            self.rotation[name] = -90
        # Articulated pauldron hinge bones pivoting at each upper-medial collar strap.
        for name, pivot in (("pauldron_r", [638, 676]), ("pauldron_l", [778, 688])):
            self.bones[name] = {"name": name, "parent": "chest", "pivot": pivot}
            self.rotation[name] = self.rotation["chest"]
        sigil_pivot = [1060, 720]
        self.bones["sigil_root"] = {"name": "sigil_root", "parent": "root", "pivot": sigil_pivot}
        self.rotation["sigil_root"] = self.rotation["root"]
        for layer in ("sigil_outer", "sigil_inner", "sigil_core"):
            self.bones[layer] = {"name": layer, "parent": "sigil_root", "pivot": sigil_pivot}
            self.rotation[layer] = self.rotation["sigil_root"]
        thrust_pivot = [1800, 868]
        for layer in ("thrust_streak", "thrust_flash"):
            self.bones[layer] = {"name": layer, "parent": "root", "pivot": thrust_pivot}
            self.rotation[layer] = self.rotation["root"]

        register_cuffs(self)

    def bone_entries(self):
        entries = []
        for name, item in self.bones.items():
            x, y = item["pivot"]
            parent = item.get("parent")
            if parent:
                px, py = self.bones[parent]["pivot"]
                parent_angle = self.rotation[parent]
                entry = {"name": name, "parent": parent}
                local = rotate(x - px, py - y, -parent_angle)
            else:
                parent_angle = 0
                entry = {"name": name}
                local = (x - self.width / 2, self.height - y)
            entry.update(x=round(local[0], 7), y=round(local[1], 7))
            angle = round(shortest(self.rotation[name] - parent_angle), 7)
            if angle:
                entry["rotation"] = angle
            if item.get("tip"):
                length = math.dist(item["pivot"], item["tip"])
                if length:
                    entry["length"] = round(length, 7)
            entries.append(entry)
        return entries

    def build(self, images):
        self.garment.register_material(images)
        slots = {}
        skin = {}
        lattice_slots = set()
        max_lattice_triangles = 0
        for item in self.parts:
            name = item["name"]
            bone = name if name in ("pauldron_r", "pauldron_l") else item["bone"]
            if bone not in self.bones:
                raise ValueError(f"Unknown bone {bone} for {name}")
            face = FACE_STATE.fullmatch(name)
            slot = item.get("slot") or (face[1] if face else name)
            state = item.get("state") or (face[2] if face else name)
            if slot not in slots:
                slots[slot] = {"name": slot, "bone": bone, "attachment": None}
                skin[slot] = {}
            if slots[slot]["bone"] != bone or state in skin[slot]:
                raise ValueError(f"Conflicting slot {slot} or duplicate attachment {state}")
            if state == "neutral" or (not face and not item.get("state")):
                slots[slot]["attachment"] = state
            x, y, w, h = item["rect"]
            texels = item.get("textureScale", 1)
            texture = Path(item["path"])
            if texture.is_absolute():
                texture = texture.relative_to(images.resolve())
            elif texture.parts[0] == "parts":
                texture = Path(*texture.parts[1:])
            with Image.open(images / texture) as im:
                if im.size != (round(w * texels), round(h * texels)):
                    raise ValueError(f"{name}: {im.size} differs from rect {item['rect']} at {texels}x")
            pix, piy = self.bones[bone]["pivot"]
            attachment = {"image": texture.as_posix()}
            flexible = flexible_binding(item, self.bones)
            if name in PANELS:
                attachment.update(self.garment.mesh(self, item["rect"]))
                lattice_slots.add(slot)
                max_lattice_triangles = max(max_lattice_triangles, len(attachment["triangles"]) // 3)
            elif flexible:
                if texels != 1:
                    raise ValueError(f"Weighted mesh {name} must be registered at canvas density")
                weights, cell, radius = flexible
                lattice_slots.add(slot)
                max_lattice_triangles = max(max_lattice_triangles, 2 * math.ceil(w/cell) * math.ceil(h/cell))
                attachment.update(type="mesh", generator={
                    "kind": "segments",
                    "anchor": [pix - x, piy - y],
                    "cell": cell,
                    "bones": weights,
                    "falloff": {"power": 4, "radius": radius, "maxBones": 3, "minWeight": 0.015},
                })
            else:
                cx, cy = rotate(x + w / 2 - pix, piy - y - h / 2, -self.rotation[bone])
                attachment.update(x=round(cx, 7), y=round(cy, 7))
                if self.rotation[bone]:
                    attachment["rotation"] = round(shortest(-self.rotation[bone]), 7)
                if texels != 1:
                    attachment.update(scaleX=1 / texels, scaleY=1 / texels)
            skin[slot][state] = attachment
        for side in CUFFS:
            name = "forearm_" + side
            item = next(part for part in self.parts if part["name"] == name)
            skin[name]["grasp"] = cuff_attachment(self, item, side)
            lattice_slots.add(name)
        closed_glove = skin["hand_r"]["hand_r"]
        skin["hand_r"]["hand_r-back"] = {**closed_glove, "image": "hand_r-back.png"}
        # Mirror `hand_l.png` on `hand_r` for the open release pose; rotate upright `hand_r-ground.png`
        # by 180 deg relative to setup `hand_r` (+178.5 deg) so it lands 0 deg horizontal after die's 180 deg wrist roll.
        open_source = skin["hand_l"]["hand_l"]
        open_w, open_len, open_rot = HAND_R_OPEN_SPEC
        skin["hand_r"]["open"] = wrist_pinned_glove(open_source, open_w, open_len, open_rot)
        raw_ground = skin["hand_r"]["ground"]
        ground_base = {
            **raw_ground,
            "x": round(-raw_ground["x"], 7),
            "y": round(-raw_ground["y"], 7),
            "rotation": round(shortest(raw_ground.get("rotation", 0.0) + 180.0), 7),
        }
        skin["hand_r"]["ground"] = ground_base
        for name, length, width in (("loosening", .60, 1.05), ("unfolding", .83, 1.02)):
            skin["hand_r"][name] = wrist_pinned_glove(open_source, open_w * width, open_len * length, open_rot)
        for _, name, source, width in GLOVE_ROLL:
            if source == "open":
                skin["hand_r"][name] = wrist_pinned_glove(open_source, open_w * width, open_len, open_rot)
            else:
                skin["hand_r"][name] = wrist_pinned_glove(ground_base, 1.0, width)
        # Both hands point from wrist to palm. Reflect across that local axis.
        # The far bronze cuff measures 53.5 px across its opening, the near one
        # 61 px. Fit the glove around its shaft contact to preserve the hold.
        grip_scale = 0.88
        palm_x = math.dist(self.bones["hand_r"]["pivot"], self.bones["weapon"]["pivot"])
        skin["hand_l"]["grip"] = {
            **closed_glove,
            "x": round(palm_x + (closed_glove["x"] - palm_x) * grip_scale, 7),
            "y": round(-closed_glove["y"] * grip_scale, 7),
            "rotation": -closed_glove["rotation"],
            "scaleX": grip_scale,
            "scaleY": -grip_scale,
        }
        weapon = next(part for part in self.parts if part["name"] == "weapon")
        shaft, head = weapon_sections(skin["weapon"]["weapon"]["image"], weapon["rect"],
                                      self.bones["weapon"]["pivot"], self.rotation["weapon"])
        if any(slot["attachment"] is None for slot in slots.values()):
            raise ValueError("A feature slot lacks a neutral setup drawing")
        slots["weapon_head"] = {"name": "weapon_head", "bone": "weapon", "attachment": None}
        skin["weapon"]["shaft"] = shaft
        skin["weapon_head"] = {"flip": head}
        lattice_slots.update(("weapon", "weapon_head"))
        for layer, blend in (("sigil_outer", "normal"), ("sigil_inner", "normal"), ("sigil_core", "additive")):
            slots[layer] = {"name": layer, "bone": layer, "attachment": None, "blend": blend}
            skin[layer] = {layer: {"image": f"{layer}.png", "x": 0, "y": 0}}
        # Offset thrust_streak behind its bone pivot so scaling stretches the wake backward from the blade tip.
        for layer, blend, x_off in (("thrust_streak", "normal", -140), ("thrust_flash", "additive", 0)):
            slots[layer] = {"name": layer, "bone": layer, "attachment": None, "blend": blend}
            skin[layer] = {layer: {"image": f"{layer}.png", "x": x_off, "y": 0}}
        rig = {
            "spec": "rigc-rig/1", "name": "realta", "images": "parts",
            "skeleton": {"x": -self.width / 2, "y": 0, "width": self.width, "height": self.height},
            "bones": self.bone_entries(), "slots": list(slots.values()), "skins": {"default": skin},
        }
        if lattice_slots:
            rig["invariants"] = {"meshSlots": len(lattice_slots), "meshTriangles": max_lattice_triangles}
        return rig


def channel(name, prop, entries, *, slot=False):
    """Keys are authored in seconds, with explicit outgoing curves for interpolated values."""
    keys = []
    for i, entry in enumerate(entries):
        t, v = entry[:2]
        if not slot and isinstance(v, (float, int)):
            v = [v]
        key = {"t": t, "v": v}
        if prop != "attachment" and i < len(entries) - 1:
            key["ease"] = entry[2] if len(entry) == 3 else "ease"
        keys.append(key)
    return {"slot" if slot else "bone": name, "property": prop, "keys": keys}


def weapon_flip_tracks(flip_time, return_time):
    """The descending sweep turns the painted head once, then the recovery turns it back."""
    return [
        channel("weapon", "attachment", [(0, "weapon"), (flip_time, "shaft"), (return_time, "weapon")], slot=True),
        channel("weapon_head", "attachment", [(0, None), (flip_time, "flip"), (return_time, None)], slot=True),
    ]


def joint_probe(canvas):
    # The separated extrema show the raised arm on its own, then the bent
    # leg on its own, then the prop against the coat without a compensating camera.
    poses = {
        "upper_arm_r": [-22, -8, -16], "forearm_r": [-30, -9, -24],
        "hand_r": [8, 4, 13], "weapon": [24, 7, -34],
        "upper_arm_l": [18, 0, 12], "forearm_l": [24, 0, 17],
        "thigh_r": [0, 18, 0], "shin_r": [0, -36, 0], "foot_r": [0, 10, 0],
        "thigh_l": [0, 0, 14], "shin_l": [0, 0, -28], "foot_l": [0, 0, 8],
        "chest": [2, -1, -4], "head": [-4, 4, 5],
        "hat": [-1, 1, -2], "hat_tip": [-3, 2, 4],
        "hair_r": [-3, 4, 3], "hair_l": [2, -3, -4],
        "coat_r": [1, -4, 2], "coat_l": [-1, 3, -4], "coat_back": [0, 2, -2],
        "ribbon": [-2, 3, 5], "ribbon_long": [-5, 3, 9], "ribbon_short": [-4, 4, 12],
    }
    missing = sorted(set(poses) - canvas.bones.keys())
    if missing:
        raise ValueError(f"Probe uses these anatomy roles: {missing}")
    from kinematics import Pose
    from sampling import bake, sample_times
    beats = (0, .5, 1, 1.5, 2)
    times = sample_times(2, 96, beats)
    samples = []
    for t in times:
        segment = min(3, int(t * 2))
        u = t * 2 - segment
        weight = u * u * (3 - 2 * u)
        pose = Pose(canvas)
        for name, positions in poses.items():
            angles = (0, *positions, 0)
            pose.angles[name] = angles[segment] + weight * (angles[segment+1] - angles[segment])
        samples.append(support_garment(pose))
    tracks = bake(times, samples, beats)
    return {"duration": 2, "loop": False, "tracks": tracks}


def motion(clips):
    return {"spec": "rigc-motion/1", "archetype": "realta", "cut": "realta", "easings": EASINGS, "animations": clips}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("layout", type=Path)
    parser.add_argument("--images", type=Path, required=True)
    parser.add_argument("--out", type=Path, required=True)
    args = parser.parse_args()
    layout = json.loads(args.layout.read_text(encoding="utf-8-sig"))
    canvas = CanvasRig(layout)
    args.out.mkdir(parents=True, exist_ok=True)
    save_json(args.out / "realta.rig.json", canvas.build(args.images))
    save_json(args.out / "probe.motion.json", motion({"joint_probe": joint_probe(canvas)}))
