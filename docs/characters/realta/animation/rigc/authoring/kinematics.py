"""Pose the cutout at its measured hinges and project canvas moves into Spine tracks."""

import math
from pathlib import Path

import cv2
import numpy as np
from PIL import Image

from author_rig import HAND_R_OPEN_SPEC, rotate, shortest


def add(a, b):
    return a[0] + b[0], a[1] + b[1]


def delta(a, b):
    return a[0] - b[0], a[1] - b[1]


def turn(vector, angle):
    """Rotate canvas-space coordinates through a Spine angle."""
    x, y = rotate(vector[0], -vector[1], angle)
    return x, -y


def heading(vector):
    return math.degrees(math.atan2(-vector[1], vector[0]))


class Pose:
    def __init__(self, canvas):
        self.canvas = canvas
        self.angles = {}
        self.moves = {}
        self.scales = {}

    def frame(self, name):
        entry = self.canvas.bones[name]
        at = entry["pivot"]
        move = self.moves.get(name, (0.0, 0.0))
        angle = self.angles.get(name, 0.0)
        parent = entry.get("parent")
        if not parent:
            return add(at, move), angle
        parent_at, parent_angle = self.frame(parent)
        relative = delta(at, self.canvas.bones[parent]["pivot"])
        return add(parent_at, turn(add(relative, move), parent_angle)), parent_angle + angle

    def transform(self, name, point):
        at, angle = self.frame(name)
        return add(at, turn(delta(point, self.canvas.bones[name]["pivot"]), angle))

    def orient(self, name, world_delta):
        parent = self.canvas.bones[name].get("parent")
        parent_angle = self.frame(parent)[1] if parent else 0.0
        self.angles[name] = shortest(world_delta - parent_angle)

    def place(self, name, point):
        entry = self.canvas.bones[name]
        parent = entry.get("parent")
        if not parent:
            self.moves[name] = delta(point, entry["pivot"])
            return
        parent_at, parent_angle = self.frame(parent)
        relative = turn(delta(point, parent_at), -parent_angle)
        self.moves[name] = delta(relative, delta(entry["pivot"], self.canvas.bones[parent]["pivot"]))

    def solve_two(self, upper, lower, end, target, *, bend=None):
        """Solve the observed bend side for a planted ankle or posed wrist."""
        shoulder = self.frame(upper)[0]
        data = self.canvas.bones
        v1 = delta(data[lower]["pivot"], data[upper]["pivot"])
        v2 = delta(data[end]["pivot"], data[lower]["pivot"])
        l1, l2 = math.hypot(*v1), math.hypot(*v2)
        toward = delta(target, shoulder)
        distance = min(l1 + l2, max(abs(l1 - l2), math.hypot(*toward)))
        cosine = (distance * distance - l1 * l1 - l2 * l2) / (2 * l1 * l2)
        rest_turn = shortest(heading(v2) - heading(v1))
        direction = math.copysign(1, rest_turn if bend is None else bend)
        elbow = direction * math.acos(min(1, max(-1, cosine)))
        first = heading(toward) - math.degrees(math.atan2(l2 * math.sin(elbow), l1 + l2 * math.cos(elbow)))
        self.orient(upper, first - heading(v1))
        self.orient(lower, first + math.degrees(elbow) - heading(v2))

    def hold(self, name, angle, point):
        self.orient(name, angle)
        self.place(name, point)

    def aimed_grip(self, hand_angle, grip, weapon_angle):
        bones = self.canvas.bones
        offset = turn(delta(bones["weapon"]["pivot"], bones["hand_r"]["pivot"]), hand_angle)
        self.solve_two("upper_arm_r", "forearm_r", "hand_r", delta(grip, offset))
        self.orient("hand_r", hand_angle)
        self.orient("weapon", weapon_angle)

    def track_translation(self, name):
        dx, dy = self.moves.get(name, (0.0, 0.0))
        parent = self.canvas.bones[name].get("parent")
        return rotate(dx, -dy, -self.canvas.rotation.get(parent, 0.0))


def opacity_hull(item, images):
    """Hull of the actual nontransparent texel corners in canvas coordinates."""
    file = Path(item["path"])
    if file.parts[0] == "parts":
        file = Path(*file.parts[1:])
    with Image.open(images / file) as im:
        alpha = np.array(im.getchannel("A"))
    contours = cv2.findContours((alpha > 0).astype("uint8"), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)[0]
    if not contours:
        raise ValueError(f'No part geometry for {item["name"]}')
    centers = cv2.convexHull(np.concatenate(contours))[:, 0, :]
    all_corners = np.concatenate([centers + d for d in [(0, 0), (1, 0), (1, 1), (0, 1)]])
    outline = cv2.convexHull(all_corners.astype("float32"))[:, 0, :]
    return outline / item.get("textureScale", 1) + np.array(item["rect"][:2])


class Ground:
    def __init__(self, canvas, images):
        self.items = []
        for item in canvas.parts:
            if item["name"].startswith(("eye_", "mouth")):
                continue
            self.items.append((item["name"], item["bone"], opacity_hull(item, images)))
        # Register derived hand_r-open hull from mirrored hand_l.png and align upright hand_r-ground
        # hull with its 180 deg setup-space attachment flip around hand_r's wrist pivot.
        hand_l_item = next(item for item in canvas.parts if item["name"] == "hand_l")
        hand_l_hull = next(pts for name, _, pts in self.items if name == "hand_l")
        lx0, ly0, lw, lh = hand_l_item["rect"]
        l_piv = canvas.bones["hand_l"]["pivot"]
        r_piv = canvas.bones["hand_r"]["pivot"]
        l_rot = canvas.rotation["hand_l"]
        r_rot = canvas.rotation["hand_r"]
        u_hull = hand_l_hull[:, 0] - (lx0 + lw / 2.0)
        v_hull = (ly0 + lh / 2.0) - hand_l_hull[:, 1]
        u_w = l_piv[0] - (lx0 + lw / 2.0)
        v_w = (ly0 + lh / 2.0) - l_piv[1]
        w_scale, l_scale, extra_rot = HAND_R_OPEN_SPEC
        ang = -l_rot + extra_rot
        pts = []
        for u, v in zip(u_hull, v_hull):
            bx, by = rotate((u - u_w) * w_scale, (v - v_w) * l_scale, ang)
            cx, cy = rotate(bx, by, r_rot)
            pts.append((r_piv[0] + cx, r_piv[1] - cy))
        self.items.append(("hand_r-open", "hand_r", np.array(pts, dtype=np.float32)))
        r_piv_arr = np.array(r_piv, dtype=np.float32)
        self.items = [
            (name, bone, (2.0 * r_piv_arr - hull) if name == "hand_r-ground" else hull)
            for name, bone, hull in self.items
        ]
        self.floor = max(float(points[:, 1].max()) for _, _, points in self.items)
        self.broom = next(points for name, _, points in self.items if name == "weapon")
        self.grip = canvas.bones["weapon"]["pivot"]

    def lowest_grip(self, angle):
        """Lowest y at which the grip can be held without sinking the solid prop."""
        points = self.broom - self.grip
        radians = math.radians(angle)
        dy = -math.sin(radians) * points[:, 0] + math.cos(radians) * points[:, 1]
        return self.floor - float(dy.max())

    def bottom(self, pose, only=None, omit=()):
        lowest = -math.inf
        for name, bone, outline in self.items:
            if (only is not None and name not in only) or name in omit:
                continue
            (_, at_y), angle = pose.frame(bone)
            points = outline - pose.canvas.bones[bone]["pivot"]
            if bone in pose.scales:
                basis = math.radians(pose.canvas.rotation[bone])
                cosine, sine = math.cos(basis), math.sin(basis)
                along = cosine * points[:, 0] - sine * points[:, 1]
                across = sine * points[:, 0] + cosine * points[:, 1]
                kx, ky = pose.scales[bone]
                points = np.column_stack((cosine*kx*along+sine*ky*across,
                                          -sine*kx*along+cosine*ky*across))
            radians = math.radians(angle)
            y = at_y - math.sin(radians) * points[:, 0] + math.cos(radians) * points[:, 1]
            lowest = max(lowest, float(y.max()))
        return lowest
