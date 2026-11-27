"""Bake the seven independent performance clips for Realta's base-painting rig."""

from __future__ import annotations

from wrist_seams import seat_cuffs

import argparse
import json
import math
from pathlib import Path

from author_rig import CanvasRig, GLOVE_ROLL, channel, motion, save_json, shortest, weapon_flip_tracks
from garment import support_garment
from kinematics import Ground, Pose, add, delta, heading, turn
from sampling import Phrasing, bake, sample_times


FEATURES = ("eye_r", "eye_l", "mouth")
PROP = ("weapon", "ribbon", "ribbon_long", "ribbon_short")


def smooth(value):
    value = max(0.0, min(1.0, value))
    return value * value * (3 - 2 * value)


def mix(a, b, weight):
    return a + (b - a) * weight


def breath(t, period, lag=0):
    phase = 2 * math.pi / period
    return 0.5 * (math.cos(-lag * phase) - math.cos((t - lag) * phase))


def wave(t, period, lag=0):
    phase = 2 * math.pi / period
    return math.sin((t - lag) * phase) - math.sin(-lag * phase)


def face(eyes, mouth=None):
    mouth = eyes if mouth is None else mouth
    return [
        channel(name, "attachment", [(max(0, time - 0.000001), state) for time, state in keys], slot=True)
        for name, keys in zip(FEATURES, (eyes, eyes, mouth))
    ]


class Performance:
    def __init__(self, canvas, images):
        self.canvas = canvas
        self.bones = canvas.bones
        self.ground = Ground(canvas, images)
        lead_boot = next(hull for name, _, hull in self.ground.items if name == "boot_l")
        self.lead_sole = lead_boot - self.bones["foot_l"]["pivot"]
        self.lead_sole_depth = max(self.lead_sole[:, 1])
        self.rig = canvas.build(images)
        self.slots = [item["name"] for item in self.rig["slots"]]
        self.rig["events"] = {
            "attack_hit": {},
            "guard_brace": {},
            "cast_release": {},
            "weapon_release": {},
        }

    def planted(self, pose, right=0.0, left=0.0):
        for side, shift in (("r", right), ("l", left)):
            upper, lower, foot = (f"{prefix}_{side}" for prefix in ("thigh", "shin", "foot"))
            ankle = self.bones[foot]["pivot"]
            pose.solve_two(upper, lower, foot, (ankle[0] + shift, ankle[1]))
            pose.orient(foot, 0.0)

    def grip(self, pose, dx, dy, hand, shaft, slide=0.0, slide_x=None):
        base = self.bones["weapon"]["pivot"]
        shifted_wood = turn((0.111*slide if slide_x is None else slide_x,slide), shaft)
        wanted = add(base, (dx, dy))
        clearance = min(.75, .7*math.sqrt(abs(dx)+abs(dy)+abs(shaft)))
        safe_y = self.ground.lowest_grip(shaft) + shifted_wood[1] - clearance
        wanted = (wanted[0], min(safe_y, wanted[1]))
        pose.aimed_grip(hand, wanted, shaft)
        if slide:
            actual = pose.frame("weapon")[0]
            pose.place("weapon", delta(actual, shifted_wood))

    def free_hand(self, pose, dx, dy, turn_angle, bend=None):
        at = self.bones["hand_l"]["pivot"]
        pose.solve_two("upper_arm_l", "forearm_l", "hand_l", add(at, (dx, dy)), bend=bend)
        pose.orient("hand_l", turn_angle)

    def idle(self, t):
        p = Pose(self.canvas)
        b = breath(t, 2.6)
        p.moves["chest"] = (0.8 * b, -5.3 * b)
        p.angles.update(chest=0.62*b, head=-0.85*breath(t, 2.6, .10), hat=.36*breath(t, 2.6, .16),
                        hat_tip=-1.7*wave(t, 2.6, .26), hair_r=.9*wave(t, 2.6, .14),
                        hair_l=-1.2*wave(t, 2.6, .19), upper_arm_l=.43*b, forearm_l=-.8*b,
                        hand_l=1.5*breath(t, 2.6, .14), coat_r=.48*wave(t, 2.6, .21),
                        coat_l=-.44*wave(t, 2.6, .25), coat_back=.27*wave(t, 2.6, .31),
                        ribbon=.56*wave(t, 2.6, .21), ribbon_long=1.5*wave(t, 2.6, .31),
                        ribbon_short=1.12*wave(t, 2.6, .38))
        self.grip(p, -.8*b, -1.8*b, -.64*b, .95*b)
        return p

    def relaxed(self, t):
        p = Pose(self.canvas)
        beat = breath(t, 1.0)
        groove = wave(t, 2.0)
        phrase = wave(t, 4.0)
        slow = wave(t, 8.0)
        p.moves["pelvis"] = (-30 + 2.8*groove + 1.2*slow, 322 + 4.8*beat - 1.2*breath(t, 4.0))
        p.angles.update(
            pelvis=24.0 - 0.7*groove - 0.25*slow,
            chest=-43.5 + 1.8*groove + 1.0*beat,
            head=7.5 + 2.8*wave(t, 2.0, .06) + 1.4*breath(t, 1.0, .08) + 0.8*phrase,
            hat=1.4*wave(t, 2.0, .11) + 1.1*breath(t, 1.0, .10),
            hat_tip=-3.2*wave(t, 2.0, .18) - 2.2*breath(t, 1.0, .15),
            hair_r=2.5 + 2.0*wave(t, 2.0, .13) + 1.0*breath(t, 1.0, .12),
            hair_l=-2.8 - 2.2*wave(t, 2.0, .15) - 1.1*breath(t, 1.0, .14),
            coat_r=1.2 + 0.9*wave(t, 2.0, .16) + 0.7*breath(t, 1.0, .14),
            coat_l=-1.4 - 1.0*wave(t, 2.0, .18) - 0.8*breath(t, 1.0, .16),
            coat_back=-0.8 - 0.7*wave(t, 2.0, .20),
        )
        p.moves["chest"] = (0.8*groove, -2.0*beat)
        # Full low crouch on the balls of her feet (heels lifted slightly so ankles stay natural
        # and toes remain planted cleanly on the floor line).
        for side, shift_x, lift_y, pitch in (("r", 34, -18, -14.0), ("l", -22, -20, -16.0)):
            upper, lower, foot = (f"{prefix}_{side}" for prefix in ("thigh", "shin", "foot"))
            ankle = self.bones[foot]["pivot"]
            p.solve_two(upper, lower, foot, (ankle[0] + shift_x, ankle[1] + lift_y))
            p.orient(foot, pitch)
        for part in ("coat_back", "coat_r", "coat_l", "skirt_hem_r", "skirt_hem_l"):
            p.scales[part] = (0.67, 1.0)
        support_garment(p)
        # In a full low crouch the knees rise high toward the waist; moderate the upward
        # panel lift so the mid-coat never inverts past the waist row while keeping hem drape clean.
        for part, x_scale, y_scale, y_min in (
            ("coat_r", 0.85, 0.30, -28.0),
            ("coat_l", 0.68, 0.25, -30.0),
            ("skirt_hem_r", 0.90, 0.45, -45.0),
            ("skirt_hem_l", 0.85, 0.45, -45.0),
        ):
            mx, my = p.moves.get(part, (0.0, 0.0))
            p.moves[part] = (mx * x_scale, max(y_min, my * y_scale))

        # Right elbow rests near the raised right thigh so the fist cups under the chin.
        chin = p.transform("head", (722, 642))
        head_world = p.frame("head")[1]
        p.solve_two("upper_arm_r", "forearm_r", "hand_r", add(chin, (-22, 52)), bend=1)
        p.orient("hand_r", -124 + 0.7*head_world)

        knee_l = p.frame("shin_l")[0]
        p.solve_two("upper_arm_l", "forearm_l", "hand_l", add(knee_l, (32 + 2.0*groove, -24 - 2.0*beat)))
        p.orient("hand_l", 18 + 6.0*breath(t, 1.0, .05) + 1.5*groove)

        broom_x = 322 + 3.5*wave(t, 4.0, .10) + 1.5*groove
        broom_y = 745 - 10.0*breath(t, 2.0, .16) - 3.0*breath(t, 1.0, .12)
        broom_angle = -7.5 + 1.6*wave(t, 4.0, .14) + 0.8*wave(t, 2.0, .12)
        p.hold("weapon", broom_angle, (broom_x, broom_y))
        p.angles.update(
            ribbon=1.8 + 1.4*wave(t, 2.0, .18) + 0.8*phrase,
            ribbon_long=3.2 + 2.6*wave(t, 2.0, .24) + 1.4*phrase,
            ribbon_short=-1.4 + 2.2*wave(t, 2.0, .28) + 1.2*phrase,
        )
        return p

    # One fist lands just ahead of the broom ferrule. The other catches the
    # middle of the wood. These are material points in the original painting.
    front_grip = (395, 834)
    rear_grip = (433.5, 1104)
    # Mirrored glaive blade tip in setup canvas space (sharp upturned steel point).
    thrust_tip = (238.0, 122.4)
    thrust_angle = -96.5
    attack_hit = .54
    attack_catch = .31
    attack_release = .705
    # Coil back cleanly after the two-handed catch while the lead knee chambers,
    # then accelerate monotonically into attack_hit (0.54 s) with a crisp hit-hold.
    attack_phrase = Phrasing(
        [0, .052, .085, .16, .23, .285, .31, .375, .430, .462, .488, .512, .528, .540, .556, .598, .650, .705, .765, .865, .965, 1.05],
        dx=   [0, -5,   -12,  -27, -49, -64,  -69, -75,  -78,  -55,   10,  128,  208,  246,  249,  243,  194,  122,   60,    8,   -2,   0],
        down= [0,  3,     7,   20,  37,  51,   57,  67,   76,   83,  100,  122,  134,  139,  141,  136,  113,   86,   54,   15,    1,   0],
        lean= [0, 2.5,  4.4,    6, 8.8,  10, 10.1, 12.0, 13.8, 10.6,   0.0,-16.5,-27.2,-31.5,-32.2,-30.5,-21.0,-10.0,  0.5,  2.1,  0.3,   0],
        head= [0, -1,  -2.1,   -3,-4.5,-5.9, -6.1, -7.0, -8.0, -6.2,  -1.2,  5.8, 11.2, 13.4, 14.3, 13.0,  9.8,  5.2,  0.9, -0.9,    0,   0],
        gx=   [0, -8,   -10,    7,  59, 117,  141, 118,  102,  115,  198,  415,  598,  684,  688,  680,  588,  422,  238,   89,    1,   0],
        lift= [0,-13,   -25,  -39,  -6,  97,  132, 134,  136,  135,  133,  132,  131,  131,  131,  131,  126,   98,   42,  -20,  -12,   0],
        shaft=[0, 6.5,  8.2,  -35, -76,-96.5,-96.5,-96.5,-96.5,-96.5,-96.5,-96.5,-96.5,-96.5,-96.5,-96.5,-96.5,-95,  -83,  -30,  1.2,   0],
        upper=[0,-1.3, -3.2,  -13, -25, -36,  -39, -39,  -39,  -39,  -35,  -27,  -21,  -18,  -17,  -17,  -17,  -16,  -13,   -6,  -.6,   0],
        elbow=[0, 2.8,    7,   21,  42,  59,   64,  64,   64,   64,   60,   50,   41,   37,   35,   35,   34,   32,   24,   11,    1,   0],
        palm= [0,   0,   -1,   -3,  -6,  -8,   -8,  -8,   -8,   -8,   -7,   -5,   -3,   -3,   -3,   -3,   -3,   -3,   -2,    2,    0,   0],
    )

    step_phrase = Phrasing(
        [0, .275, .31, .365, .425, .465, .495, .515, .655, .675, .72, .78, .855, .965, 1.05],
        advance=  [0, 0,  0,   20,   56,  108,  214,  284,  284,  280, 232, 146,   57,    0,    0],
        clearance=[0, 0,  5,   36,   62,   66,   36,    0,    0,    7,  59,  75,   49,    0,    0],
        toe=      [0, 0, -1,   -6,   -7,   -4,    6,    0,    0,   -1,  -7,   3,    7,    0,    0],
        landing=  [0, 0,  0,    0,    0,    6,   20,   32,   32,   32,  25,  16,    5,    0,    0],
    )

    thrust_fx_phrase = Phrasing(
        [0, .468, .495, .515, .530, .540, .560, .600, .650, 1.05],
        streak_sx=   [.30, .34, .88, 1.16, 1.30, 1.36, 1.44, 1.50, 1.54, 1.54],
        streak_sy=   [.62, .66, .98, 1.18, 1.24, 1.22, .88,  .42,  .18,  .18],
        streak_drift=[0,    0,   0,   0,    0,    0,  34,   66,   82,   82],
        flash_sx=    [.30, .30, .30, .40, .94, 1.32, 1.46, 1.54, 1.58, 1.58],
        flash_sy=    [.26, .26, .26, .34, .80, 1.20, 1.04, .56,  .24,  .24],
        flash_drift= [0,    0,   0,   0,    0,    0,  16,   30,   38,   38],
    )

    def lunge_feet(self, pose, time):
        # The rear sole supports the gather, swing and recovery. The lead sole
        # lands flat in the rear foot's floor plane before the hands reach out.
        rear = self.bones["foot_r"]["pivot"]
        pose.solve_two("thigh_r", "shin_r", "foot_r", rear)
        pose.orient("foot_r", 0)
        step = self.step_phrase.at(time)
        x, y = self.bones["foot_l"]["pivot"]
        # Register the painted sole, including its round toe, at the keyed
        # clearance. Rotating around the ankle alone makes the toe scrape.
        angle = math.radians(step["toe"])
        foot_x, foot_y = self.lead_sole.T
        rotated_depth = max(math.cos(angle) * foot_y - math.sin(angle) * foot_x)
        sole_height = self.lead_sole_depth - rotated_depth + step["landing"] - step["clearance"]
        ankle = (x + step["advance"], y + sole_height)
        pose.solve_two("thigh_l", "shin_l", "foot_l", ankle)
        pose.orient("foot_l", step["toe"])

    def thrust_elbow(self, pose, t):
        # Roll the large-pauldron elbow toward the viewer as the weapon lowers.
        # The moving wrist and its original shaft registration stay fixed.
        roll = smooth((t - .045) / .18) * (1 - smooth((t - .845) / .18))
        if not roll:
            return
        upper, lower, hand = "upper_arm_r", "forearm_r", "hand_r"
        shoulder = pose.frame(upper)[0]
        original_elbow = pose.frame(lower)[0]
        wrist, hand_angle = pose.frame(hand)
        pose.solve_two(upper, lower, hand, wrist, bend=1)
        if roll < 1:
            other_elbow = pose.frame(lower)[0]
            elbow = tuple(mix(a, b, roll) for a, b in zip(original_elbow, other_elbow))
            # Both IK solutions share a wrist. The projected elbow passes
            # between them as the arm rolls through camera depth.
            parent_scale = 1.0
            for bone, child, first, last in ((upper, lower, shoulder, elbow), (lower, hand, elbow, wrist)):
                rest = delta(self.bones[child]["pivot"], self.bones[bone]["pivot"])
                projected = delta(last, first)
                pose.orient(bone, heading(projected) - heading(rest))
                scale = math.hypot(*projected) / math.hypot(*rest)
                local_scale = scale / parent_scale
                pose.scales[bone] = (local_scale, local_scale)
                parent_scale = scale
            pose.scales[hand] = (1 / parent_scale, 1 / parent_scale)
        pose.orient(hand, hand_angle)

    def _attack_body(self, t):
        k = self.attack_phrase.at(t)
        p = Pose(self.canvas)
        p.moves["pelvis"] = (k["dx"], k["down"])
        p.angles.update(pelvis=k["lean"]*.25, chest=k["lean"]*.75, head=k["head"])
        self.lunge_feet(p,t)
        axis=turn(delta(self.front_grip,self.rear_grip),self.thrust_angle)
        # Choke back on the wood while bringing it down. The slide finishes
        # before the front fist closes, and only resumes after it lets go.
        slide=smooth((t-.095)/(.30-.095))*(1-smooth((t-.725)/.22))
        back_offset=delta(self.rear_grip,self.bones["weapon"]["pivot"])
        # The shaft's centre bows about 2.6 pixels outwards halfway down the
        # slide, measured on the alpha silhouette between the two registrations.
        back_x=back_offset[0]*slide+10.4*slide*(1-slide)
        self.grip(p,k["gx"],k["lift"]+k["gx"]*axis[1]/axis[0],k["shaft"],k["shaft"],
                  slide=back_offset[1]*slide,slide_x=back_x)

        p.angles.update(upper_arm_l=k["upper"],forearm_l=k["elbow"])
        caught=smooth((t-.159)/(self.attack_catch-.159))*(1-smooth((t-self.attack_release)/.135))
        hand_angle=mix(p.frame("forearm_l")[1]+k["palm"],k["shaft"]+99.21,caught)
        # The grip attachment mirrors the existing fist around the palm bone's
        # long axis. Its wrist→shaft offset is 38.013 source px on that axis.
        palm_axis=delta(self.bones["hand_l"]["tip"], self.bones["hand_l"]["pivot"])
        offset=turn(tuple(38.0131556*v/math.hypot(*palm_axis) for v in palm_axis),hand_angle)
        locked_wrist=delta(p.transform("weapon",self.front_grip),offset)
        free_wrist=p.frame("hand_l")[0]
        if t<self.attack_catch:
            # Reach to the catch before the moving timber arrives. Chasing its
            # earlier position would tuck the elbow hard against the shoulder.
            guide=(791,868)
            joined=smooth((t-.25)/(self.attack_catch-.25))
            locked_wrist=tuple(mix(a,b,joined) for a,b in zip(guide,locked_wrist))
        elif t>self.attack_release:
            # Let the open hand fall away instead of following the climbing
            # shaft toward the face after release.
            leave=smooth((t-self.attack_release)/.085)
            locked_wrist=tuple(mix(a,b,leave) for a,b in zip(locked_wrist,free_wrist))
        wrist=tuple(mix(a,b,caught) for a,b in zip(free_wrist,locked_wrist))
        # The far clavicle drops under the small pauldron at the coil reversal.
        # Solve to the same wood mark after the sleeve has settled into place.
        shoulder_load = smooth((t - .40) / .032) * (1 - smooth((t - .462) / .038))
        if shoulder_load:
            p.moves["upper_arm_l"] = (0, 5 * shoulder_load)
        p.solve_two("upper_arm_l","forearm_l","hand_l",wrist)
        p.orient("hand_l", hand_angle)
        self.follow(p, self.attack_phrase, t, k, "lean", "shaft")
        # The weight moves underneath the loose cloth first. The two ribbon
        # tails have different trailing lengths during the straight thrust.
        hip_travel=k["dx"]-self.attack_phrase.at(t-.065)["dx"]
        shaft_travel=k["gx"]-self.attack_phrase.at(t-.052)["gx"]
        settle=smooth((1.05-t)/.09)
        for cloth,factor in (("coat_back",.055),("coat_r",.075),("coat_l",.10),("hair_l",.018)):
            p.angles[cloth]-=factor*hip_travel*settle
        p.angles["ribbon_long"]+=.065*shaft_travel*settle
        p.angles["ribbon_short"]+=.044*shaft_travel*settle
        self.thrust_elbow(p, t)
        seat_cuffs(p, smooth((t-.20)/.11)*(1-smooth((t-self.attack_release)/.13)))
        return p

    def attack(self, t):
        p = self._attack_body(t)
        fx = self.thrust_fx_phrase.at(t)
        axis = turn(delta(self.front_grip, self.rear_grip), self.thrust_angle)
        axis_len = math.hypot(*axis)
        ux, uy = axis[0] / axis_len, axis[1] / axis_len
        thrust_deg = heading(axis)
        # Track the moving glaive tip into attack_hit, then anchor to the strike point
        # so the piercing wake and impact glint carry slightly forward as the spear settles.
        if t <= self.attack_hit:
            tip = p.transform("weapon", self.thrust_tip)
        else:
            if not hasattr(self, "_attack_tip_anchor"):
                self._attack_tip_anchor = self._attack_body(self.attack_hit).transform("weapon", self.thrust_tip)
            tip = self._attack_tip_anchor
        s_drift = fx["streak_drift"]
        f_drift = fx["flash_drift"]
        # Center the hollow wind-cone over the curved blade belly (+38 y) while pinning
        # the impact starburst right onto the upturned blade point (+16 forward, +14 y).
        p.place("thrust_streak", add(tip, (ux * s_drift, uy * s_drift + 38.0)))
        p.angles["thrust_streak"] = thrust_deg
        p.scales["thrust_streak"] = (fx["streak_sx"], fx["streak_sy"])
        p.place("thrust_flash", add(tip, (ux * (16.0 + f_drift), uy * (16.0 + f_drift) + 14.0)))
        p.angles["thrust_flash"] = thrust_deg
        p.scales["thrust_flash"] = (fx["flash_sx"], fx["flash_sy"])
        return p

    hurt_phrase = Phrasing(
        [0,.065,.15,.27,.43,.62,.8],dx=[0,-7,-15,-11,-7,1,0],down=[0,5,12,9,4,1,0],
        lean=[0,5.5,8,5.8,2.8,-.7,0],head=[0,5.2,9.0,5.6,1.3,-.8,0],
        gx=[0,-14,-27,-20,-9,1,0],gy=[0,-18,-42,-30,-14,-3,0],
        shaft=[0,5,12,10,4,-1,0],hand=[0,4,9,7,3,0,0],free=[0,7,15,12,4,-1,0],
    )

    def hurt(self, t):
        k = self.hurt_phrase.at(t)
        p = Pose(self.canvas)
        p.moves["pelvis"] = (k["dx"],k["down"])
        p.angles.update(pelvis=.15*k["lean"],chest=.85*k["lean"],head=.8*k["head"],
                        upper_arm_l=k["free"],forearm_l=1.25*k["free"],hand_l=-.7*k["free"])
        self.planted(p)
        self.grip(p,k["gx"],k["gy"],k["hand"],k["shaft"])
        self.follow(p, self.hurt_phrase, t, k, "lean", "shaft")
        return p

    guard_phrase = Phrasing(
        [0, .055, .12, .18, .24, .34, .48, .61, .72],
        dx=   [0,   -6, -15, -18, -22, -16,  -8,  -1,   0],
        down= [0,    8,  21,  28,  30,  24,  11,   2,   0],
        lean= [0, -2.5,-6.8,-9.5,-8.2,-6.5,-2.8,-0.4,   0],
        head= [0,  1.8, 4.5, 6.2, 5.6, 4.2, 1.8, 0.2,   0],
        gx=   [0,   28,  78, 106, 100,  92,  44,   6,   0],
        gy=   [0,  -16, -38, -48, -45, -40, -18,  -2,   0],
        shaft=[0,  -10, -26, -35, -33, -30, -14,  -1,   0],
        hand= [0,   -7, -18, -24, -22, -20,  -9,  -1,   0],
        fx=   [0,  -18, -52, -74, -70, -62, -26,  -3,   0],
        fy=   [0,  -52,-148,-212,-206,-186, -78,  -8,   0],
        palm= [0,   16,  44,  62,  58,  52,  22,   2,   0],
    )

    guard_ward_phrase = Phrasing(
        [0, .07, .14, .18, .24, .34, .48, .72],
        sx=       [.18, .22, .38, .44, .46, .45, .48, .48],
        sy=       [.36, .44, .74, .86, .88, .86, .92, .92],
        inner_rot=[0,     0, -28, -52, -74,-108,-148,-148],
        core_s=   [.45, .52, .92, 1.12,1.02, .84, .48, .48],
        core_rot= [0,     0,  10,  20,  28,  40,  52,  52],
    )

    def guard(self, t):
        k = self.guard_phrase.at(t)
        w = self.guard_ward_phrase.at(t)
        p = Pose(self.canvas)
        p.moves["pelvis"] = (k["dx"], k["down"])
        p.angles.update(pelvis=.22 * k["lean"], chest=.78 * k["lean"], head=k["head"])
        brace = smooth(t / .15) * (1 - smooth((t - .42) / .26))
        self.planted(p, right=-8.0 * brace, left=18.0 * brace)
        self.grip(p, k["gx"], k["gy"], k["hand"], k["shaft"])
        self.free_hand(p, k["fx"], k["fy"], k["palm"])
        self.follow(p, self.guard_phrase, t, k, "lean", "shaft")
        seat_cuffs(p, 0.55 * brace)
        ward_anchor = p.transform("chest", (835, 760))
        p.place("sigil_root", ward_anchor)
        p.angles["sigil_root"] = -4.0
        p.scales["sigil_root"] = (w["sx"], w["sy"])
        p.angles["sigil_inner"] = w["inner_rot"]
        p.scales["sigil_inner"] = (1.0, 1.0)
        p.angles["sigil_core"] = w["core_rot"]
        p.scales["sigil_core"] = (w["core_s"], w["core_s"])
        return p

    cast_phrase = Phrasing(
        [0, .07, .14, .20, .25, .30, .38, .50, .58, .76, .98, 1.16, 1.3],
        dx=   [0, -14, -26,   6,  28,  21,  17,  15,  13,   8,   3,  -.5, 0],
        down= [0,  12,  24,  18,  14,  17,  15,  12,  10,   6,   2,    0, 0],
        lean= [0,   5,   9,  -4, -13, -9.5, -10.5, -9, -7.5, -4, -1, .3, 0],
        head= [0,  -3,  -5,   2,   6,   4.5,   5,  4,  3.2, 1.8, .4,  0, 0],
        fx=   [0, -30, -55,  72, 156, 144, 149, 138, 128,  86,  34,  4, 0],
        fy=   [0, -48, -95, -138, -166, -158, -162, -152, -142, -96, -34, -3, 0],
        palm= [0, -12, -20,  28,  56,  50,  53,  46,  40,  24,  10,  -1, 0],
        gx=   [0, -12, -24, -29, -32, -30, -28, -24, -20, -12,  -4,   0, 0],
        gy=   [0,  -7, -14, -26, -34, -31, -28, -24, -19, -11,  -3,   0, 0],
        shaft=[0,   7,  14, 16.5, 18, 16.5, 15.5, 13, 10.5, 6,  1.8, -.2, 0],
    )

    sigil_phrase = Phrasing(
        [0, .09, .14, .21, .25, .26, .32, .42, .52, .58, .65, .72, .82, .90, 1.3],
        sx=       [.08, .08, .10, .38, .53, .56, .53, .56, .53, .53, .55, .57, .59, .61, .61],
        sy=       [.16, .16, .20, .78, 1.12, 1.18, 1.12, 1.18, 1.13, 1.13, 1.16, 1.20, 1.24, 1.26, 1.26],
        outer_rot=[0,     0, -40,  45,  110,  122,  156,  198,  234,  254,  276,  295,  316,  330,  330],
        inner_rot=[0,     0,  55, -55, -135, -150, -192, -248, -298, -326, -356, -384, -416, -436, -436],
        inner_s=  [1.0, 1.0, 1.0, 1.0,  1.0,  1.0,  1.0,  1.0,  1.0,  1.0,  .90,  .76,  .56,  .52,  .52],
        core_s=   [.40, .40, .45, .95, 1.28, 1.38, 1.04, 1.08,  .96,  .88,  .68,  .45,  .40,  .40,  .40],
        core_rot= [0,     0,  -8,   8,   18,   20,   26,   34,   40,   44,   48,   52,   55,   56,   56],
    )

    def _cast_body(self, t):
        k = self.cast_phrase.at(t)
        p = Pose(self.canvas)
        p.moves["pelvis"] = (k["dx"], k["down"])
        p.angles.update(pelvis=.18*k["lean"], chest=.82*k["lean"], head=k["head"])
        self.planted(p)
        self.grip(p, k["gx"], k["gy"], -k["shaft"]*.45, k["shaft"])
        self.free_hand(p, k["fx"], k["fy"], k["palm"])
        self.follow(p, self.cast_phrase, t, k, "lean", "shaft")
        # Arcane discharge wind kickback at cast release.
        blast = smooth((t - .23) / .06) * (1 - smooth((t - .34) / .34))
        p.angles["hat"] = p.angles.get("hat", 0) + 3.2 * blast
        p.angles["hat_tip"] = p.angles.get("hat_tip", 0) + 7.5 * blast
        p.angles["hair_r"] = p.angles.get("hair_r", 0) - 7.0 * blast
        p.angles["hair_l"] = p.angles.get("hair_l", 0) - 8.5 * blast
        p.angles["coat_r"] = p.angles.get("coat_r", 0) - 6.5 * blast
        p.angles["coat_l"] = p.angles.get("coat_l", 0) - 5.5 * blast
        p.angles["coat_back"] = p.angles.get("coat_back", 0) - 5.0 * blast
        p.angles["ribbon"] = p.angles.get("ribbon", 0) - 4.0 * blast
        p.angles["ribbon_long"] = p.angles.get("ribbon_long", 0) - 10.5 * blast
        p.angles["ribbon_short"] = p.angles.get("ribbon_short", 0) - 8.5 * blast
        return p

    def cast(self, t):
        p = self._cast_body(t)
        s = self.sigil_phrase.at(t)
        # Anchor the arcane sigil cleanly in front of her extended casting palm,
        # then let it hover in place while dissolving as her hand lowers.
        casting_fingertip = (1004, 1118)  # Between the two extended tips in the registered glove.
        if t <= .55:
            palm_tip = p.transform("hand_l", casting_fingertip)
        else:
            if not hasattr(self, "_cast_sigil_anchor"):
                ref = self._cast_body(.55)
                self._cast_sigil_anchor = ref.transform("hand_l", casting_fingertip)
            palm_tip = self._cast_sigil_anchor
        p.place("sigil_root", add(palm_tip, (62, -18)))
        p.angles["sigil_root"] = -5.0
        p.scales["sigil_root"] = (s["sx"], s["sy"])
        p.angles["sigil_outer"] = s["outer_rot"]
        p.angles["sigil_inner"] = s["inner_rot"]
        p.scales["sigil_inner"] = (s["inner_s"], s["inner_s"])
        p.angles["sigil_core"] = s["core_rot"]
        p.scales["sigil_core"] = (s["core_s"], s["core_s"])
        return p

    @staticmethod
    def follow(pose, phrase, t, current, lean, staff):
        # The loose edges answer the preceding movement and catch up after its peak.
        mantle = phrase.at(t - .068)[lean] - current[lean]
        hair = phrase.at(t - .046)[lean] - current[lean]
        shaft = phrase.at(t - .047)[staff] - current[staff]
        tail = phrase.at(t - .086)[staff] - current[staff]
        decay = smooth((phrase.times[-1]-t)/.1)
        pose.angles.update(hat=.28*hair*decay, hat_tip=.8*mantle*decay,
                           hair_r=.55*hair*decay,hair_l=.68*mantle*decay,
                           coat_r=.7*mantle*decay,coat_l=.53*mantle*decay,
                           coat_back=.4*mantle*decay,ribbon=.085*shaft*decay,
                           ribbon_long=.13*tail*decay,ribbon_short=.095*tail*decay)

    @staticmethod
    def articulate_pauldrons(pose):
        arm_r = shortest(pose.angles.get("upper_arm_r", 0.0))
        arm_l = shortest(pose.angles.get("upper_arm_l", 0.0))
        pose.angles.setdefault("pauldron_r", max(-22.0, min(14.0, 0.28 * arm_r)))
        pose.angles.setdefault("pauldron_l", max(-15.0, min(26.0, 0.30 * arm_l)))
        return pose

    # The first boot rolls onto its toe before the opposite knee catches up.
    # The greaves hold at the kneel and slide rearward as weight moves onto the arms.
    death_lower_phrase = Phrasing(
        [0, .08, .18, .29, .39, .48, .56, .68, .78, .92, 1.02, 1.14, 1.26, 1.38, 1.48, 1.56, 1.66, 2.2],
        lean=[0, 6, 2, -8, -16, -22, -24, -27, -28, -28, -31, -40, -51, -65, -74, -73, -72, -72],
        thigh_r=[0, 7, 17, 31, 41, 45, 43, 41, 42, 42, 39, 29, 8, -26, -53, -65, -62, -62],
        shin_r=[0, -3, -14, -35, -61, -75, -76, -76, -76, -76, -76, -76, -76, -76, -76, -76, -76, -76],
        boot_r=[0, -2, -13, -40, -73, -102, -106, -106, -106, -106, -106, -106, -106, -106, -106, -106, -106, -106],
        knee_r=[673, 700, 738, 785, 836, 866, 866, 866, 866, 866, 865, 846, 793, 730, 695, 674, 672, 672],
        thigh_l=[0, 4, 13, 27, 33, 32, 28, 24, 24, 24, 22, 12, -11, -49, -83, -96, -95, -95],
        shin_l=[0, -3, -16, -48, -77, -93, -100, -100, -100, -100, -100, -100, -100, -100, -100, -100, -100, -100],
        boot_l=[0, -2, -18, -61, -107, -120, -129, -129, -129, -129, -129, -129, -129, -129, -129, -129, -129, -129],
        knee_l=[833, 852, 881, 907, 937, 951, 970, 970, 970, 970, 968, 943, 881, 799, 755, 725, 721, 721],
        depth_r=[1, 1, .99, .96, .89, .83, .83, .83, .83, .83, .82, .82, .86, .96, 1, 1, 1, 1],
        depth_l=[1, 1, .96, .82, .80, .65, .77, .77, .75, .75, .73, .66, .63, .70, .79, .83, .84, .84],
    )

    # Fabric gathers at the waist before the white hem comes to rest across the calves.
    death_drape_phrase = Phrasing(
        death_lower_phrase.times,
        panel_pitch=[0, -0.01, -0, 0, -0.04, -7.66, -9.55, -10.83, -11.16, -11.16, -12.55, -20.2, -28.49, -39.49, -48.77, -64.32, -58.55, -58.55],
        panel_length=[1, 1, 0.976, 0.908, 0.786, 0.497, 0.555, 0.637, 0.64, 0.64, 0.69, 0.822, 0.907, 0.955, 0.984, 1.211, 1.128, 1.128],
        panel_width=[1, 1, 0.973, 0.897, 0.763, 0.67, 0.593, 0.555, 0.551, 0.551, 0.548, 0.534, 0.475, 0.399, 0.313, 0.129, 0.178, 0.178],
        panel_x=[0, 0.02, -1, -3.84, -8.65, 8.12, 13.11, 19.17, 19.8, 19.8, 25.15, 38.6, 34.15, 32.23, 34.04, 124.5, 86.04, 86.04],
        panel_y=[0, -0.02, 0.31, 1.3, 3.29, 48.58, 20.8, -5.47, -7.14, -7.14, -20.55, -61.55, -96.6, -108.74, -110.49, -98.41, -109.69, -109.69],
        hem_pitch=[0, -0.04, 0.16, 0.76, 2.07, 1.37, 2.48, 5.25, 4.93, 4.93, 5.48, -4.79, -4.22, -7.23, -11.55, -7.71, -9.99, -9.99],
        hem_length=[1, 1, 0.975, 0.905, 0.779, 0.173, 0.18, 0.273, 0.306, 0.306, 0.358, 0.557, 0.58, 0.536, 0.533, 0.544, 0.575, 0.575],
        hem_width=[1, 1, 0.969, 0.883, 0.731, 0.645, 0.554, 0.512, 0.513, 0.513, 0.511, 0.47, 0.476, 0.487, 0.454, 0.459, 0.453, 0.453],
        hem_x=[0, 0.29, -3.9, -15.76, -36.7, -34.95, -43.49, -54.56, -55.94, -55.94, -62.77, -66.79, -142.23, -208.33, -239.47, -284.18, -266.32, -266.32],
        hem_y=[0, -0.06, 0.94, 3.89, 9.77, 198.64, 197.52, 153.93, 133.8, 133.8, 112.66, -5.63, -39.8, -54, -95.87, -155.27, -154.1, -154.1],
        panel_spread=[0, 0.03, 0.68, 2.59, 6.25, 22.87, 32.61, 41.97, 43.86, 43.86, 53.54, 70.16, 92.44, 105.67, 109.44, 105.71, 101.12, 101.12],
        panel_drop=[0, -0.02, -1, -3.8, -8.84, -21.42, -23.32, -24.79, -22.7, -22.7, -23.23, -20.69, -2.02, 20.32, 35.79, 34.12, 24.45, 24.45],
        hem_spread=[0, 0.22, 1.24, 4.23, 9.87, 0.16, 2.25, 0.74, 0.7, 0.7, 0.2, 42.72, -4.61, -29.67, -12.39, 14.32, 10.08, 10.08],
        hem_drop=[0, -0.18, -0.83, -2.76, -6.65, 4.8, 6.58, 8.76, 9.6, 9.6, 10.11, -20.46, -3.15, 2.25, -6.29, -1.58, -3.32, -3.32],
        panel_turn=[0, -0, -0.25, -1.05, -2.89, -11.06, -13.74, -15.13, -15.81, -15.81, -17.33, -17.86, -18.97, -19.33, -17.3, -5.56, -6.67, -6.67],
        hem_turn=[0, -0.03, -0.11, -0.38, -1.04, -1.22, -0.58, 1.08, 0.32, 0.32, 0.69, -7.42, 1.45, 6.28, 3.79, -5.95, -2.86, -2.86],
    )

    # The far knee tucks into the front opening while the right side seam rounds the kneecap.
    death_skirt_phrase = Phrasing(
        [0, 0.08, 0.18, 0.29, 0.39, 0.48, 0.56, 0.68, 0.78, 0.92, 1.02, 1.14, 1.26, 1.38, 1.48, 1.56, 1.66, 2.2],
        rear_knee=[0, 0, -12, -52, -60, -76, -80, -77, -76, -76, -72, -60, -39, -9, 0, 0, 0, 0],
        far_panel=[0, 0, 15, 65, 65, 85, 83, 79, 79, 79, 77, 80, 62, 27, 5, 0, 0, 0],
        far_hem=[0, 0, 18, 42, 54, 83, 91, 86, 86, 86, 85, 75, 62, 30, 5, 0, 0, 0],
        near_panel=[0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -8, -26, -20, -20],
        near_hem=[0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -8, -45, -40, -40],
        near_lift=[0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -7, -6, -6],
        leg_depth=[0, 0, 0, -0.07, -0.09, -0.08, -0.1, -0.1, -0.1, -0.1, -0.09, -0.06, -0.03, 0, 0, 0, 0, 0],
    )

    # The painted right facing rounds each kneecap before returning underneath the waist.
    death_facing_phrase = Phrasing(
        [0.0, 0.08, 0.18, 0.25, 0.29, 0.33333, 0.39, 0.44444, 0.48, 0.5, 0.56, 0.68, 0.78, 0.92, 1.02, 1.08333, 1.14, 1.16667, 1.19444, 1.22222, 1.26, 1.33333, 1.38, 1.44444, 1.48, 1.56, 1.66, 2.2],
        right_x=[0.0, 0.0, 19.8166, 47.4273, 46.4762, 33.942, -8.6874, 84.3402, 69.542, 48.9902, -28.5965, -58.4406, -48.7648, -48.8324, -65.8753, -73.855, -91.6322, -56.9547, -55.2672, -53.5148, -57.6242, -44.0135, -25.4498, -6.3031, 0.0, 0.0, 0.0, 0.0],
        right_y=[0.0, 0.0, -2.6657, -18.3677, -18.4432, -12.4267, 2.0995, -16.7918, -36.7971, -28.4088, -12.3527, -22.8388, -28.7522, -28.7554, -28.2908, -28.5142, -42.4773, -26.6984, -17.8869, -12.7564, -7.8093, -2.629, -1.0376, 0.2134, 0.0, 0.0, 0.0, 0.0],
        right_angle=[0.0, 0.0, -1.6591, -7.6525, -8.5119, -5.2944, -0.0779, -15.6708, -10.4271, -7.2919, 6.1008, 6.849, 9.5598, 9.5786, 11.4033, 10.5994, 11.8003, -0.1513, -0.6115, -0.2716, -0.0287, 1.7571, 0.5908, -0.3811, 0.0, 0.0, 0.0, 0.0],
        right_length=[0.0, 0.0, -0.0218, -0.0602, -0.0625, -0.071, -0.0599, 0.0556, 0.1535, 0.1425, 0.0927, 0.0933, 0.137, 0.1371, 0.1356, 0.122, 0.1412, 0.0387, 0.0164, 0.0062, -0.0212, -0.0223, -0.003, 0.0127, 0.0, 0.0, 0.0, 0.0],
        right_width=[0.0, 0.0, -0.0456, -0.1433, -0.1549, -0.1534, -0.0721, -0.2316, -0.2197, -0.1731, -0.0156, 0.1097, 0.0912, 0.0911, 0.0961, 0.0893, 0.0971, 0.1307, 0.1179, 0.0949, 0.1138, 0.0296, 0.0061, -0.0025, 0.0, 0.0, 0.0, 0.0],
        edge_x=[0.0, 0.0, -6.3037, -9.6846, -10.83, -15.5568, -18.7945, -27.3653, -30.7311, -29.238, -14.0435, -2.3302, -4.3199, -4.322, -1.7564, 5.0046, 22.4571, 20.0336, 15.3704, 10.1936, 6.3455, 2.3011, -2.4701, -2.6697, 0.0, 0.0, 0.0, 0.0],
        edge_y=[0.0, 0.0, -0.2915, 0.1131, 0.0827, 0.3857, -0.1099, -1.1114, -0.6432, -0.7897, -1.2677, -7.0047, -7.5491, -7.5562, -7.0746, -6.9678, -16.3591, -6.1408, -6.0845, -1.3143, -0.5342, 0.5834, -0.0327, -0.1289, 0.0, 0.0, 0.0, 0.0],
        edge_angle=[0.0, 0.0, -1.4662, -2.8166, -3.2981, -4.3908, -5.159, -3.6294, -2.1944, -2.2947, -2.3394, 1.5808, 0.305, 0.2958, 1.0204, 2.2975, -3.4721, -0.5109, -1.4583, -2.1524, -2.3292, -1.5553, -1.7248, -0.7521, 0.0, 0.0, 0.0, 0.0],
        edge_length=[0.0, 0.0, -0.0103, -0.0082, -0.0088, -0.006, -0.0105, -0.0214, -0.0122, -0.01, -0.0194, 0.0252, 0.0264, 0.0263, 0.0341, 0.037, 0.0514, 0.0837, 0.0714, 0.0369, 0.0222, 0.0087, -0.0049, -0.0016, 0.0, 0.0, 0.0, 0.0],
        edge_width=[0.0, 0.0, -0.0231, -0.0354, -0.0408, -0.0564, -0.0763, -0.1233, -0.1473, -0.1454, -0.0938, -0.0341, -0.0461, -0.0462, -0.0329, -0.0054, 0.0548, 0.0381, 0.0279, 0.0108, -0.0031, -0.0056, -0.0214, -0.0136, 0.0, 0.0, 0.0, 0.0],
    )

    def die_lower(self, pose, t):
        key = self.death_lower_phrase.at(t)
        coverage = self.death_skirt_phrase.at(t)
        pose.angles["pelvis"] = key["lean"]
        for side, floor_gap, catch in (("r", 1.0, .48), ("l", 17.0, .56)):
            thigh, shin, foot, boot = (name + "_" + side for name in ("thigh", "shin", "foot", "boot"))
            pose.orient(thigh, key[thigh])
            pose.orient(shin, key[shin])
            pose.orient(foot, key[boot])
            knee = pose.frame(shin)[0]
            # Measure the actual greave and shoe. The source foot starts on its own
            # painted depth plane and rolls onto the greave as the upper leg folds.
            rest_sole = next(hull for name, _, hull in self.ground.items if name == boot)
            plane = mix(float(rest_sole[:, 1].max()), self.ground.floor - floor_gap, smooth(t / catch))
            foot_depth = self.ground.bottom(pose, only=(shin, boot)) - knee[1]
            knee_x = key["knee_" + side] + (coverage["rear_knee"] if side == "l" else 0)
            target = (knee_x, plane - foot_depth)
            travel = delta(target, knee)
            if side == "r":
                pose.place("pelvis", add(pose.frame("pelvis")[0], travel))
            else:
                # The far hip recedes behind the near one as the three-quarter
                # waist tips onto its side; the knee and ankle retain their lengths.
                pose.place(thigh, add(pose.frame(thigh)[0], travel))

        # Compress only the depth projection of each thigh about its fixed knee.
        # An inverse scale on the shin preserves the size of its armor and boot.
        for side in ("r", "l"):
            thigh, shin = "thigh_" + side, "shin_" + side
            root = pose.frame(thigh)[0]
            knee = pose.frame(shin)[0]
            projection = key["depth_" + side] + (coverage["leg_depth"] if side == "l" else 0)
            offset = delta(knee, root)
            pose.place(thigh, (knee[0] - offset[0] * projection, knee[1] - offset[1] * projection))
            pose.scales[thigh] = (projection, projection)
            pose.scales[shin] = (1 / projection, 1 / projection)

        fabric = self.death_drape_phrase.at(t)
        facing = self.death_facing_phrase.at(t)
        # The painted fur extends a few pixels beyond its sampled mesh edge.
        fur_clearance = 3.5 * smooth((t - .28) / .20)
        for part, row, side in (
            ("coat_back", "panel", 0), ("coat_r", "panel", -1), ("coat_l", "panel", 1),
            ("skirt_hem_r", "hem", -1), ("skirt_hem_l", "hem", 1),
        ):
            angle = fabric[row + "_pitch"] + side * fabric[row + "_turn"]
            dx = fabric[row + "_x"] + side * fabric[row + "_spread"]
            dy = fabric[row + "_y"] + side * fabric[row + "_drop"] - fur_clearance
            long_scale = fabric[row + "_length"]
            across_scale = fabric[row + "_width"]
            if side == 1:
                anchor = "right" if row == "panel" else "edge"
                dx += coverage["far_" + row] + facing[anchor + "_x"]
                dy += facing[anchor + "_y"]
                angle += facing[anchor + "_angle"]
                long_scale += facing[anchor + "_length"]
                across_scale += facing[anchor + "_width"]
            elif side == -1:
                dx += coverage["near_" + row]
                dy += coverage["near_lift"] if row == "panel" else 0
            pose.orient(part, angle)
            pose.scales[part] = (long_scale, across_scale)
            pose.place(part, add(pose.frame("pelvis")[0], (dx, dy)))
        pose.garment_supported = True

    death_hat_phrase = Phrasing(
        [0, .085, .16, .25, .34, .43, .52, .61, .685, .72, .77, .82, .865, .91, .96, 1.02],
        # The head carries the cap at first. After .43 its path is entirely in floor coordinates.
        worn=[0, 3, 6, 2, -5, -8, -8, -8, -8, -8, -8, -8, -8, -8, -8, -8],
        turn=[0, 4, 2, -9, -25, -40, -61, -90, -121, -136, -151, -158.14, -160.8, -157.2, -158.33, -158.14],
        approach=[680, 670, 632, 546, 410, 267, 138, 44, 8, 0, 0, 0, 0, 0, 0, 0],
        clearance=[920, 880, 826, 758, 690, 612, 498, 326, 117, 0, 0, 0, 0, 0, 0, 0],
        tip=[0, -2, -6, 3, 10, 6, -1, -7, -10, -8, 5, 3, -2, 1, 0, 0],
    )

    drop_phrase = Phrasing(
        [.12, .20, .31, .42, .52, .62, .685, .718, .75, .794, .84, .89, .96, 1.03],
        turn=[-2, -4.8, -15, -34, -57, -79, -94.5, -100.03, -100.88, -99.58, -100.17, -100.03, -100.03, -100.03],
        travel=[0, .016, .105, .284, .535, .8, .95, .985, .998, 1, 1, 1, 1, 1],
        streamer_long=[0, 1, 3, 7, 11, 15, 21, 26, 31, 37, 42, 45.5, 45.8, 45.8],
        streamer_short=[0, -1, 2, 7, 13, 18, 24, 30, 38, 47, 55, 62, 67.8, 68.1],
    )

    def die_props(self, p, t):
        h = self.death_hat_phrase.at(t)
        p.angles.update(hat=h["worn"], hat_tip=h["tip"])
        leaving = smooth((t - .25) / .18)
        if leaving:
            at, worn_angle = p.frame("hat")
            angle = mix(worn_angle, h["turn"], leaving)
            p.orient("hat", angle)
            # The curved brim rolls first, then the folded crown takes the small rebound.
            # These are the two painted points on the supporting line at rest.
            resting = -158.14
            contact = (954, 436) if angle >= resting else (630, 306)
            radius = delta(contact, self.bones["hat"]["pivot"])
            rocking_x = turn(radius, resting)[0] - turn(radius, angle)[0]
            target_x = 1752 + rocking_x - h["approach"]
            depth = self.ground.bottom(p, only=("hat", "hat_back")) - at[1]
            target_y = self.ground.floor - depth - h["clearance"]
            p.place("hat", (mix(at[0], target_x, leaving), mix(at[1], target_y, leaving)))

        if t <= .12:
            return
        # The wrist solve owns the shared release frame: four pixels back, ten up, two degrees clockwise.
        # The bristles settle first. Both the bristles and the blade touch at the final painted angle.
        d = self.drop_phrase.at(t)
        release_x, release_y = add(self.bones["weapon"]["pivot"], (-4, -10))
        opening = self.ground.lowest_grip(-2) - release_y
        gap = opening * (1 - smooth((t - .12) / .08))
        grip_x = mix(release_x, 650, d["travel"])
        p.hold("weapon", d["turn"], (grip_x, self.ground.lowest_grip(d["turn"]) - gap))
        # The knot turns on the wood as its two lengths settle; bending a short tail alone would kink it.
        knot = .20 * d["streamer_short"]
        p.angles.update(ribbon=knot, ribbon_long=d["streamer_long"] - knot, ribbon_short=d["streamer_short"] - knot)

    # The ribs lead the fall, pause on the weak brace, then roll over the
    # grounded forearm. The head trails both changes of support.
    death_phrase = Phrasing(
        [0, .08, .16, .29, .40, .50, .62, .75, .90, 1.02, 1.12, 1.25, 1.36, 1.45, 1.53, 1.66, 2.2],
        ribs=[0, 7, 2, -17, -39, -58, -67, -71, -72, -72, -73, -81, -94, -102, -100, -101, -101],
        skull=[0, 10, 7, -8, -25, -39, -43, -45, -48, -54, -65, -76, -82, -85, -88, -87, -87],
        wrist_r_x=[-177, -171, -165, -111, -63, -36, -19, -12, -12, -15, -20, -23, -12, 0, 8, 12, 12],
        wrist_r_y=[130, 126, 144, 192, 214, 224, 230, 232, 232, 230, 222, 210, 207, 188, 177, 172, 172],
        glove=[0, -3, 1, 23, 51, 74, 87, 94, 94, 96, 108, 128, 149, 160, 166, 164, 164],
        wrist_l_x=[118, 102, 84, 90, 104, 115, 108, 103, 103, 111, 129, 157, 178, 208, 225, 225, 225],
        wrist_l_y=[255, 243, 248, 238, 228, 220, 228, 236, 236, 220, 205, 155, 95, 38, 24, 22, 22],
        palm=[0, -5, -3, 7, 15, 24, 34, 40, 40, 43, 50, 59, 67, 73, 77, 76, 76],
        elbow=[0, -6, -13, -20, -22, -22, -22, -22, -22, -25, -34, -42, -55, -77, -105, -109, -109],
    )

    def die(self, t):
        p = Pose(self.canvas)
        self.die_lower(p, t)
        k = self.death_phrase.at(t)
        p.orient("chest", k["ribs"])
        p.orient("head", k["skull"])
        lag = self.death_phrase.at(t - .065)["skull"] - k["skull"]
        rest = smooth((t - 1.16) / .36)
        p.angles["hair_r"] = .50 * lag - 5 * rest
        p.angles["hair_l"] = .65 * lag + 2 * rest

        # Once the cheek-side hair reaches the floor, let the ribs take the
        # remaining drop. Keep the knees, pelvis and both detached props in
        # their own floor planes instead of grounding the entire character.
        if t > 1.26:
            for _ in range(3):
                skull_edge = self.ground.bottom(p, only=("head", "hair_l"))
                clearance = self.ground.floor - 3 - skull_edge
                if clearance >= 0:
                    break
                chest = p.frame("chest")[1] - clearance / 3.5
                p.orient("chest", chest)
                p.orient("head", k["skull"])

        # Roll the near shoulder inward as the breastplate comes down. The
        # pauldron follows the same socket and the free wrist can clear the chin.
        roll = smooth((t - 1.12) / .38)
        for socket in ("upper_arm_r", "pauldron_r"):
            p.place(socket, add(p.frame(socket)[0], (8 * roll, 30 * roll)))

        # The open hand takes the first load. Its heel has one planted contact
        # while the elbow folds; the spent hand slides out at the very end.
        far_shoulder = p.frame("upper_arm_l")[0]
        reach = add(far_shoulder, (k["wrist_l_x"], k["wrist_l_y"]))
        p.orient("hand_l", k["palm"])
        palm_depth = self.ground.bottom(p, only=("hand_l",)) - p.frame("hand_l")[0][1]
        brace = smooth((t - .46) / .24)
        far_target = (mix(reach[0], 1038, brace),
                      mix(reach[1], self.ground.floor - 13 - palm_depth, brace))
        p.solve_two("upper_arm_l", "forearm_l", "hand_l", far_target, bend=1)
        # As the shoulder passes the planted palm, tuck the upper arm back and
        # roll the weight onto its forearm. Both bone lengths remain intact.
        buckle = smooth((t - .96) / .33)
        if buckle:
            upper, lower = "upper_arm_l", "forearm_l"
            upper_world = p.frame(upper)[1]
            fore_world = p.frame(lower)[1]
            p.orient(upper, mix(upper_world, k["elbow"], buckle))
            elbow = p.frame(lower)[0]
            elbow_limit = self.ground.floor - 69
            if elbow[1] > elbow_limit:
                vector = delta(self.bones[lower]["pivot"], self.bones[upper]["pivot"])
                height = (elbow_limit - far_shoulder[1]) / math.hypot(*vector)
                arm_heading = -180 + math.degrees(math.asin(max(-1, min(1, height))))
                p.orient(upper, arm_heading - heading(vector))
                elbow = p.frame(lower)[0]
            fore = delta(self.bones["hand_l"]["pivot"], self.bones[lower]["pivot"])
            rise = max(-1, min(1, (far_target[1] - elbow[1]) / math.hypot(*fore)))
            fore_goal = -math.degrees(math.asin(rise)) - heading(fore)
            p.orient(lower, fore_world + buckle * shortest(fore_goal - fore_world))
        p.orient("hand_l", k["palm"])

        # The near glove leaves the thigh with the ribs. Its elbow folds back
        # beside the belt so the foreground gauntlet lands below the cheek.
        near_shoulder = p.frame("upper_arm_r")[0]
        near_target = add(near_shoulder, (k["wrist_r_x"], k["wrist_r_y"]))
        # Roll through the narrow edge of the glove before the palm takes weight.
        palm_roll = smooth((t - 1.04) / .35)
        glove_angle = mix(k["glove"], 180, palm_roll)
        edge_time = 1.18
        p.orient("hand_r", glove_angle)
        glove = "hand_r-open" if t < edge_time else "hand_r-ground"
        glove_depth = self.ground.bottom(p, only=(glove,)) - p.frame("hand_r")[0][1]
        contact = smooth((t - 1.20) / .27)
        near_target = (near_target[0], mix(near_target[1], self.ground.floor - glove_depth, contact))
        p.solve_two("upper_arm_r", "forearm_r", "hand_r", near_target, bend=1)
        p.orient("hand_r", glove_angle)

        if t <= .12:
            hit = smooth(t / .12)
            self.grip(p, -4 * hit, -10 * hit, -3 * hit, -2 * hit)
        elif t < .29:
            chain = ("upper_arm_r", "forearm_r", "hand_r")
            free = [p.angles[bone] for bone in chain]
            self.grip(p, -4, -10, -3, -2)
            let_go = smooth((t - .12) / .17)
            for bone, goal in zip(chain, free):
                held = p.angles[bone]
                p.angles[bone] = held + let_go * shortest(goal - held)
        if t > 1.18:
            # The shoulder plates usually hinge with the arms. Once the arms
            # unload on the floor, both plates settle back against the collar.
            self.articulate_pauldrons(p)
            load = 1 - smooth((t - 1.18) / .34)
            for plate in ("pauldron_r", "pauldron_l"):
                p.angles[plate] *= load
        self.die_props(p, t)
        return p

    def assemble(self):
        setup_order = self.slots
        # Keep the small-pauldron arm behind the shirt. The front glove only
        # comes around the silhouette when it actually closes on the shaft.
        far_stack = ["upper_arm_l", "pauldron_l", "forearm_l", "hand_l"]
        weapon_stack = ["weapon", "weapon_head", "ribbon_long", "ribbon_short", "ribbon"]
        near_sleeve = ["upper_arm_r", "pauldron_r", "forearm_r"]
        thrust_fx = ["thrust_streak", "thrust_flash"]
        moved_attack = {*far_stack, *weapon_stack, *near_sleeve, "hand_r", *thrust_fx}
        grasp_order = [n for n in setup_order if n not in moved_attack]
        grasp_order[0:0] = far_stack
        # The large shoulder plate belongs to the camera-side arm. Its blue
        # sleeve, plate and cuff turn together in front of the breastplate.
        shoulder_plane = grasp_order.index("pouch") + 1
        grasp_order[shoulder_plane:shoulder_plane] = near_sleeve
        # The prop crosses both clothing and hat in the foreground. Only the
        # fingers on its exposed wood cover it during a grip; thrust FX overlay the blade tip.
        grasp_order.extend([*weapon_stack, "hand_r", *thrust_fx])
        caught_order = [n for n in grasp_order if n != "hand_l"]
        caught_order.insert(caught_order.index("hand_r"), "hand_l")
        # Spine backfills unreserved positions in setup order. Reserve the
        # shifted clothing as well as the two arm chains.
        def attack_offsets(order):
            target = {name: index for index, name in enumerate(order)}
            return [{"slot": name, "offset": target[name] - old} for old, name in enumerate(setup_order)
                    if old != target[name]]

        # The small-pauldron arm reaches around the far side of the coat.
        cast_offsets = attack_offsets(far_stack + [name for name in setup_order if name not in far_stack])
        ready_order = [n for n in setup_order if n != "upper_arm_r"]
        ready_order.insert(ready_order.index("pauldron_r"), "upper_arm_r")
        ready_offsets = attack_offsets(ready_order)
        grasp_offsets = attack_offsets(grasp_order)
        caught_offsets = attack_offsets(caught_order)
        # In the crouch, the floating broom sits behind the silhouette and the near right leg crosses ahead of the far left leg under the coat.
        broom_slots = [n for n in setup_order if n in ("weapon", "weapon_head", "ribbon_long", "ribbon_short", "ribbon")]
        near_leg = [n for n in setup_order if n in ("thigh_r", "shin_r", "boot_r")]
        relaxed_order = broom_slots + [n for n in setup_order if n not in (*broom_slots, *near_leg)]
        leg_at = relaxed_order.index("coat_r")
        relaxed_order[leg_at:leg_at] = near_leg
        relaxed_offsets = attack_offsets(relaxed_order)
        near_arm = ["upper_arm_r", "pauldron_r", "forearm_r", "hand_r"]
        limbs_and_broom = {*far_stack, *near_arm, *broom_slots}
        standing_body = [n for n in setup_order if n not in limbs_and_broom]
        fallen_body = [n for n in relaxed_order if n not in limbs_and_broom]
        held_offsets = attack_offsets([*far_stack, *standing_body, *broom_slots, *near_arm])
        fallen_offsets = attack_offsets([*broom_slots, *far_stack, *fallen_body, *near_arm])
        clips = {}
        def solved(name, duration, maker, beats, extra_tracks, *, loop=False, draw=None, cues=None):
            # Both feet and palms have fixed contacts through a fast waist turn.
            # Resolve that counter-rotation before simplifying the linear keys.
            rate = {"attack": 192, "guard": 96, "cast": 96, "die": 96}.get(name, 60)
            times = sample_times(duration, rate, beats)
            piece = {"duration":duration,"loop":loop,
                     "tracks": bake(times,[self.articulate_pauldrons(support_garment(maker(t))) for t in times],beats) + extra_tracks}
            if draw:
                piece["drawOrder"] = draw
            if cues:
                piece["events"] = [{"t":t,"name":n} for t,n in cues]
            clips[name] = piece
        # Track 0 idle_loop owns upright breathing; Track 1 blink_loop owns asynchronous blinking.
        solved("idle_loop", 2.6, self.idle, [1.3], face([(0, "neutral")], [(0, "neutral")]), loop=True)
        clips["blink_loop"] = {
            "duration": 3.6,
            "loop": True,
            "tracks": [
                channel("eye_r", "attachment", [(0, "neutral"), (1.84, "closed"), (1.975, "neutral"), (3.6, "neutral")], slot=True),
                channel("eye_l", "attachment", [(0, "neutral"), (1.84, "closed"), (1.975, "neutral"), (3.6, "neutral")], slot=True),
            ],
        }
        thrust_tracks = [
            channel("thrust_streak", "attachment", [(0, None), (.468, "thrust_streak"), (.654, None)], slot=True),
            channel("thrust_streak", "alpha", [(0, [0]), (.468, [0]), (.492, [.78]), (.518, [.96]), (.548, [.94]), (.588, [.52]), (.650, [0]), (1.05, [0])], slot=True),
            channel("thrust_flash", "attachment", [(0, None), (.512, "thrust_flash"), (.640, None)], slot=True),
            channel("thrust_flash", "alpha", [(0, [0]), (.512, [0]), (.530, [.80]), (.540, [1.0]), (.560, [.82]), (.600, [.26]), (.636, [0]), (1.05, [0])], slot=True),
        ]
        solved("attack",1.05,self.attack,[*self.attack_phrase.times,*self.step_phrase.times,*self.thrust_fx_phrase.times,.654],
               face([(0,"neutral"),(.11,"attack"),(.73,"neutral")])+
               weapon_flip_tracks(.145,.82)+thrust_tracks+[
                   channel("forearm_r","attachment",[(0,"grasp")],slot=True),
                   channel("forearm_l","attachment",[(0,"grasp")],slot=True),
                   channel("hand_l","attachment",[(0,"hand_l"),(self.attack_catch,"grip"),(self.attack_release,"hand_l")],slot=True)],
               draw=[{"t":0,"offsets":ready_offsets},{"t":.110,"offsets":grasp_offsets},
                     {"t":self.attack_catch,"offsets":caught_offsets},
                     {"t":self.attack_release,"offsets":grasp_offsets},{"t":.895,"offsets":ready_offsets}],
               cues=[(self.attack_hit,"attack_hit")])
        guard_ward_tracks = [
            channel("sigil_inner", "attachment", [(0, None), (.07, "sigil_inner"), (.50, None)], slot=True),
            channel("sigil_inner", "alpha", [(0, [0]), (.07, [0]), (.14, [.52]), (.18, [.72]), (.34, [.55]), (.48, [0]), (.72, [0])], slot=True),
            channel("sigil_core", "attachment", [(0, None), (.09, "sigil_core"), (.46, None)], slot=True),
            channel("sigil_core", "alpha", [(0, [0]), (.09, [0]), (.15, [.58]), (.18, [.78]), (.32, [.48]), (.45, [0]), (.72, [0])], slot=True),
        ]
        solved("guard", .72, self.guard, [*self.guard_phrase.times, *self.guard_ward_phrase.times, .50],
               face([(0, "neutral"), (.06, "attack"), (.54, "neutral")], [(0, "neutral"), (.06, "closed"), (.54, "neutral")])
               + guard_ward_tracks + [
                   channel("forearm_r", "attachment", [(0, "grasp")], slot=True),
                   channel("forearm_l", "attachment", [(0, "grasp")], slot=True),
                   channel("hand_l", "attachment", [(0, "hand_l"), (.09, "casting"), (.54, "hand_l")], slot=True),
               ],
               draw=[{"t": 0, "offsets": ready_offsets}, {"t": .06, "offsets": grasp_offsets}, {"t": .58, "offsets": ready_offsets}],
               cues=[(.18, "guard_brace")])
        solved("hurt",.8,self.hurt,self.hurt_phrase.times,face([(0,"neutral"),(.035,"hurt"),(.48,"neutral")]))
        sigil_tracks = [
            channel("sigil_outer", "attachment", [(0, None), (.09, "sigil_outer"), (.92, None)], slot=True),
            channel("sigil_outer", "alpha", [(0, [0]), (.09, [0]), (.14, [.45]), (.23, [.95]), (.65, [.95]), (.90, [0]), (1.3, [0])], slot=True),
            channel("sigil_inner", "attachment", [(0, None), (.09, "sigil_inner"), (.92, None)], slot=True),
            channel("sigil_inner", "alpha", [(0, [0]), (.09, [0]), (.14, [.42]), (.24, [.95]), (.60, [.95]), (.82, [0]), (1.3, [0])], slot=True),
            channel("sigil_core", "attachment", [(0, None), (.14, "sigil_core"), (.74, None)], slot=True),
            channel("sigil_core", "alpha", [(0, [0]), (.14, [0]), (.22, [.85]), (.26, [1.0]), (.38, [.78]), (.55, [.70]), (.72, [0]), (1.3, [0])], slot=True),
        ]
        solved("cast",1.3,self.cast,[*self.cast_phrase.times,*self.sigil_phrase.times,.92],
               face([(0,"neutral"),(.12,"attack"),(.58,"neutral")]) + sigil_tracks + [
                   channel("hand_l", "attachment", [(0, "hand_l"), (.14, "casting"), (.90, "hand_l")], slot=True)],
               draw=[{"t":0,"offsets":cast_offsets}], cues=[(.25,"cast_release")])
        solved("relaxed_loop",8,self.relaxed,[i*0.5 for i in range(1, 16)],
               face([(0,"ponder"),(2.32,"closed"),(2.47,"ponder"),(6.08,"closed"),(6.23,"ponder")],[(0,"ponder")]) + [
                   channel("hand_r", "attachment", [(0, "hand_r-back")], slot=True)],
               loop=True,draw=[{"t":0,"offsets":relaxed_offsets}])
        solved("die",2.2,self.die,[*self.death_phrase.times, *self.death_lower_phrase.times,
                                 *self.death_hat_phrase.times, *self.drop_phrase.times],
               face([(0,"neutral"),(.08,"hurt"),(1.05,"fallen")]) + [
                   channel("hand_r", "attachment", [(0, "hand_r"), (.12, "loosening"),
                           (.165, "unfolding"), (.215, "open"),
                           *((time, name) for time, name, _, _ in GLOVE_ROLL), (1.34, "ground")], slot=True)], draw=[
                   {"t":0,"offsets":held_offsets}, {"t":.12,"offsets":fallen_offsets}],
               cues=[(.12,"weapon_release")])
        return clips


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("layout", type=Path)
    parser.add_argument("--images", type=Path, required=True)
    parser.add_argument("--out", type=Path, required=True)
    args = parser.parse_args()
    subject = Performance(CanvasRig(json.loads(args.layout.read_text(encoding="utf-8-sig"))), args.images)
    clips = subject.assemble()
    args.out.mkdir(parents=True, exist_ok=True)
    save_json(args.out / "realta.rig.json", subject.rig)
    save_json(args.out / "realta.motion.json", motion(clips))
