"""Canonical final body/prop placement, plus legacy lower-body guide coordinates.

Current authoring uses FULL_CEL_TOPLEFT and BROOM_REST. The collapse timeline and registered
full-body frames are in death-sequence.json; the coat now has a separately weighted hem bone.
The older A/B coordinates below are retained only for death_guide.py, a historical painting guide.
Those lower-body cels and their switch times are no longer part of runtime death playback.
"""
import math

FLOOR_Y = 523
HIP_REST = (240, 338)
THIGH = {'l': ((-5, 5), (-13, 53), (-13, 70)), 'r': ((11, 5), (27, 53), (12, 70))}  # hip->thigh, thigh->shin, shin->foot

# Keyed hip path (time, x, y); the cel swaps happen while the hip is moving fastest.
HIP_KEYS = [(0, 240, 338), (.08, 240, 345), (.19, 237, 392), (.21, 236, 403), (.27, 235, 412), (.31, 233, 424), (.35, 228, 463), (.41, 226, 454), (.5, 225, 459)]
CEL_A_TIME, CEL_B_TIME = .2, .31
# Forward pitch from the kneel: the v10 upper-body fall plays 3.5x faster after UPPER_SPEEDUP_FROM, and the
# side-lying cel replaces the whole body at FALL_TIME (the impact frame), then settles.
UPPER_SPEEDUP_FROM, UPPER_SPEEDUP = .3, 3.5
FALL_TIME = .62
FULL_CEL_TOPLEFT = (82, 322)         # v6 placement of the 460x230 side-lying canvas
FULL_SETTLE = [(FALL_TIME, -9, -.05), (FALL_TIME + .07, 2, .012), (FALL_TIME + .17, 0, 0.0)]  # (t, dy, rotation)
BROOM_REST = (math.pi / 2, 268, 455)  # v6 floor transform of the released broom (behind the lying body)
# Cels are ground-fixed children of the rig root; a whole-cel vertical registration seats the
# boots/knees on the source floor (the painted boots ran 6-9 px below the guide's floor line).
CEL_SHIFT = {'a': (0, -6), 'b': (0, -9)}
CEL_A_HIP = (235, 408)      # hip where cel A was drawn (between its show/hide keys)
CEL_B_HIP = (227, 459)      # hip where cel B was drawn (final resting hip)

# Pose B (kneel) leg targets in source space: knees on the floor in front, ankles tucked behind.
KNEE_B = {'l': (252, 506), 'r': (270, 509)}
ANKLE_B = {'l': (186, 497), 'r': (204, 500)}
# Pose A (deep buckle) targets: knees driven forward and down, heels lifting.
KNEE_A = {'l': (262, 440), 'r': (282, 446)}
ANKLE_A = {'l': (205, 468), 'r': (240, 470)}
# Boot world rotation (0 = upright source boot). Kneeling boots lie on their tops, soles facing back.
FOOT_A = {'l': .2, 'r': .25}
FOOT_B = {'l': 1.6, 'r': 1.65}

def _ang(v): return math.atan2(v[1], v[0])

def leg_rotations(hip, knee, ankle, side, foot_world=None):
    """Bone2D local rotations placing the knee/ankle joints toward the targets (lengths stay rigid)."""
    o, s, f = THIGH[side]
    root = (hip[0] + o[0], hip[1] + o[1])
    thigh_world = _ang((knee[0] - root[0], knee[1] - root[1])) - _ang(s)
    k = (root[0] + math.cos(thigh_world) * s[0] - math.sin(thigh_world) * s[1],
         root[1] + math.sin(thigh_world) * s[0] + math.cos(thigh_world) * s[1])
    shin_world = _ang((ankle[0] - k[0], ankle[1] - k[1])) - _ang(f)
    foot_local = 0.0 if foot_world is None else foot_world - shin_world
    return thigh_world, shin_world - thigh_world, foot_local
