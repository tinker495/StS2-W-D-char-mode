"""One skinned garment across the original back lining and two front-panel textures."""

import bisect
import math
from pathlib import Path

from PIL import Image


PANELS = ("coat_back", "coat_r", "coat_l")
HEMS = ("skirt_hem_r", "skirt_hem_l")


def smooth(value):
    u = max(0.0, min(1.0, value))
    return u * u * (3 - 2 * u)


def binding(x, y):
    # The waist stays under the belt. The lateral handles are shared across
    # the lining and front; the middle of the lining can still carry its own drag.
    hem = max(0.0, min(1.0, (y - 882) / 250))
    right = smooth((x - 614) / 266)
    center = 0.25 * 4 * right * (1 - right) * (1 - smooth((y - 1065) / 104))
    drop = smooth((y - 1169) / 175)
    return (
        ("pelvis", 1 - hem),
        ("coat_back", hem * center),
        ("coat_r", hem * (1 - center) * (1 - right) * (1 - drop)),
        ("coat_l", hem * (1 - center) * right * (1 - drop)),
        ("skirt_hem_r", hem * (1 - right) * drop),
        ("skirt_hem_l", hem * right * drop),
    )


class Garment:
    def __init__(self, parts):
        rects = [p["rect"] for p in parts if p["name"] in PANELS]

        def coordinates(axis):
            edges = {v for r in rects for v in (r[axis], r[axis] + r[axis + 2])}
            return sorted(edges | set(range(math.ceil(min(edges) / 40) * 40, max(edges), 40)))

        self.xs = coordinates(0)
        self.ys = coordinates(1)
        self.pattern = {p["name"]: p for p in parts}

    def witness(self, x, y):
        """Barycentric contributions of the very triangles used in all three meshes."""
        c = min(len(self.xs) - 2, max(0, bisect.bisect_right(self.xs, x) - 1))
        r = min(len(self.ys) - 2, max(0, bisect.bisect_right(self.ys, y) - 1))
        left, right = self.xs[c : c + 2]
        top, bottom = self.ys[r : r + 2]
        u, v = (x - left) / (right - left), (y - top) / (bottom - top)
        corners = (
            ((left, top, 1 - u), (right, top, u - v), (right, bottom, v))
            if u >= v
            else ((left, top, 1 - v), (right, bottom, u), (left, bottom, v - u))
        )
        record = {}
        for px, py, bary in corners:
            for name, w in binding(px, py):
                sx, sy, sw = record.get(name, (0, 0, 0))
                q = w * bary
                record[name] = (sx + q * px, sy + q * py, sw + q)
        return record

    def register_material(self, images):
        def spans(name, start, end, step):
            part = self.pattern[name]
            x, y, w, h = part["rect"]
            texture = Path(part["path"])
            if texture.parts[0] == "parts":
                texture = Path(*texture.parts[1:])
            with Image.open(images / texture) as source:
                solid = source.getchannel("A").point(lambda a: 255 if a >= 128 else 0)
                result = []
                for py in range(start, end, step):
                    if y <= py < y + h:
                        bounds = solid.crop((0, py - y, w, py - y + 1)).getbbox()
                        if bounds:
                            result.append((x + bounds[0], x + bounds[2] - 1, py))
                return result

        # The near front's cream band has two genuine painted edges. Track the
        # outward half; a bent thigh can emerge inside the front opening.
        self.right_edge = [
            self.witness(0.26 * inside + 0.74 * outside, y)
            for inside, outside, y in spans("coat_l", 925, 1336, 12)
        ]
        self.leg_edges = [
            (name, [(x, y) for left, right, y in spans(name, start, end, 17) for x in (left, right)])
            for name, start, end in [
                ("thigh_l", 994, 1195),
                ("shin_l", 1147, 1295),
                ("thigh_r", 994, 1200),
                ("shin_r", 1151, 1295),
            ]
        ]

    def keep_knees_under_edge(self, pose, pelvis, angle):
        from kinematics import delta, turn

        bones = pose.canvas.bones
        origin = bones["pelvis"]["pivot"]
        # The knee and hem handles share a waist pivot and neutral (downward)
        # axis, so their local transforms can be evaluated directly in that frame.
        matrices = {"pelvis": (1, 0, 0, 1, 0, 0)}
        for name in (*PANELS, *HEMS):
            sx, sy = pose.scales.get(name, (1, 1))
            a = math.radians(pose.angles.get(name, 0))
            cs, sn = math.cos(a), math.sin(a)
            m0, m1, m2, m3 = cs * sy, sn * sx, -sn * sy, cs * sx
            tx, ty = pose.moves.get(name, (0, 0))
            matrices[name] = (
                m0,
                m1,
                m2,
                m3,
                origin[0] + tx - m0 * origin[0] - m1 * origin[1],
                origin[1] + ty - m2 * origin[0] - m3 * origin[1],
            )
        side = []
        for point in self.right_edge:
            x = y = 0
            for name, (wx, wy, w) in point.items():
                a, b, c, d, tx, ty = matrices[name]
                x += a * wx + b * wy + tx * w
                y += c * wx + d * wy + ty * w
            side.append((x, y, point["coat_l"][2], point["skirt_hem_l"][2]))
        knee_shift = hem_shift = 0.0
        for name, points in self.leg_edges:
            for point in points:
                dx, dy = turn(delta(pose.transform(name, point), pelvis), -angle)
                px, py = origin[0] + dx, origin[1] + dy
                hits = []
                for (x0, y0, k0, h0), (x1, y1, k1, h1) in zip(side, side[1:]):
                    if min(y0, y1) <= py < max(y0, y1):
                        u = (py - y0) / (y1 - y0)
                        hits.append((x0 + (x1 - x0) * u, k0 + (k1 - k0) * u, h0 + (h1 - h0) * u))
                if hits:
                    edge, wk, wh = max(hits)
                    if wk + wh > 0.14 and px + 5 > edge:
                        # Move the edge over this leg without carrying the entire
                        # hem outward. At a given texel both handles contribute.
                        scale = (px + 5 - edge) / (wk * wk + 0.25 * wh * wh)
                        knee_shift = max(knee_shift, scale * wk)
                        hem_shift = max(hem_shift, scale * 0.25 * wh)
        for handle, extra in (("coat_l", knee_shift), ("skirt_hem_l", hem_shift)):
            dx, dy = pose.moves[handle]
            pose.moves[handle] = (dx + extra, dy)

    def mesh(self, canvas, rect):
        x, y, width, height = rect
        xs = [v for v in self.xs if x <= v <= x + width]
        ys = [v for v in self.ys if y <= v <= y + height]
        c1, r1 = len(xs) - 1, len(ys) - 1
        # Each crop is a rectangular subset of the same triangulation, including
        # crop boundaries. Thus a painted point on either side of a seam uses
        # exactly the same three vertices, barycentric coordinates and skinning.
        rim = (
            [(0, c) for c in range(c1 + 1)]
            + [(r, c1) for r in range(1, r1 + 1)]
            + [(r1, c) for c in reversed(range(c1))]
            + [(r, 0) for r in reversed(range(1, r1))]
        )
        verts = rim + [(r, c) for r in range(1, r1) for c in range(1, c1)]
        index = {key: i for i, key in enumerate(verts)}
        uvs, weights, triangles = [], [], []
        for r, c in verts:
            px, py = xs[c], ys[r]
            uvs.extend([(px - x) / width, (py - y) / height])
            bones = []
            for name, weight in binding(px, py):
                if weight <= 0:
                    continue
                bx, by = canvas.bones[name]["pivot"]
                angle = math.radians(canvas.rotation[name])
                cs, sn = math.cos(angle), math.sin(angle)
                dx, dy = px - bx, by - py
                bones.append(
                    {"bone": name, "x": dx * cs + dy * sn, "y": -dx * sn + dy * cs, "weight": weight}
                )
            weights.append(bones)
        for r in range(r1):
            for c in range(c1):
                a, b, d, e = index[r, c], index[r, c + 1], index[r + 1, c], index[r + 1, c + 1]
                triangles.extend((a, b, e, a, e, d))
        return {"type": "mesh", "uvs": uvs, "weights": weights, "triangles": triangles, "hull": len(rim)}


def support_garment(pose):
    """The cloth follows the lateral knee travel inside the waist, with a smaller hem lift."""
    from kinematics import delta, turn

    if getattr(pose, "garment_supported", False):
        return pose
    pose.garment_supported = True
    pelvis, angle = pose.frame("pelvis")
    bones = pose.canvas.bones
    origin = bones["pelvis"]["pivot"]
    room = pose.scales.get("coat_back", (1, 1))[1]
    if room < 1:
        compression = sum(pose.scales[n][1] for n in PANELS) / 3
        average = sum(pose.angles.get(n, 0) for n in PANELS) / 3
        for name in PANELS:
            pose.angles[name] = average
            pose.scales[name] = (1, compression)
    ankles = {}
    for side in ("l", "r"):
        travel = []
        for joint in ("shin_" + side, "foot_" + side):
            local = turn(delta(pose.frame(joint)[0], pelvis), -angle)
            travel.append(delta(local, delta(bones[joint]["pivot"], origin)))
        knee, ankle = travel
        ankles[side] = ankle[0]
        pose.moves["coat_" + side] = (
            room * (0.84 * knee[0] + 0.16 * ankle[0]),
            room * (0.73 * knee[1] + 0.13 * ankle[1]),
        )
        lower = "skirt_hem_" + side
        pose.moves[lower] = (
            room * (0.84 * ankle[0] + 0.10 * knee[0]),
            room * (0.24 * ankle[1] + 0.10 * knee[1]),
        )
        pose.angles[lower] = pose.angles.get("coat_" + side, 0)
        if room < 1:
            pose.scales[lower] = pose.scales["coat_" + side]
    # The trailing knee passes behind the near, wide front panel. Draping its
    # upper edge over that knee prevents an isolated armor tip; the lower edge
    # still falls to the back ankle and lets the boot leave below the fur.
    x, y = pose.moves["coat_r"]
    pose.moves["coat_r"] = (x + 88 * room * smooth((ankles["l"] - ankles["r"] - 78) / 150), y)
    pose.canvas.garment.keep_knees_under_edge(pose, pelvis, angle)
    return pose
