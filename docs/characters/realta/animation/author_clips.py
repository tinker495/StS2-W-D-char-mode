"""Deterministic clip authoring from the frozen setup, runtime parts and death registration spec.

Input: rig-base.json (setup + idle/relaxed clips frozen from the v10 candidate) and the runtime parts.
Output: rig.json (input of build_combat_rig.gd) plus authoring-report.json (observations, not criteria).
Edits: primary pose curves, shaft-axis twist, floor-supported prop, secondary springs and cel tracks.
No image is painted here; weapon clearance uses the rigid weapon's actual alpha pixels.
"""
from pathlib import Path
import json, math, copy
import numpy as np

P = Path(__file__).resolve().parent
import rig_parts
d = json.loads((P / 'rig-base.json').read_text())
rest = d['rest']
observations = {}
# Front cloth occludes thigh roots; sleeves default behind the chest/pauldrons.
# Attack keys move only its crossing sleeve in front, retaining tree-order drawing.
d['order'].remove('thigh_l')
d['order'].insert(d['order'].index('thigh_r')+1, 'thigh_l')
d['order'].insert(d['order'].index('upper_arm_r'), 'coat-front')
# Rounded boot ankle plugs tuck under the greaves; their original pixels do not overlap.
for side in 'lr':
    d['order'].remove('foot_'+side)
    d['order'].insert(d['order'].index('shin_'+side),'foot_'+side)
d['order'].remove('upper_arm_l')
d['order'].insert(d['order'].index('torso'), 'upper_arm_l')
d['order'].insert(d['order'].index('torso')+1, 'pauldron-r')
d['order'].insert(d['order'].index('torso'), 'neck-overlap')
d['order'].insert(d['order'].index('forearm_l'), 'wrist-overlap')
# The complete prop/ribbon is the frontmost body layer in every clip. Only the
# gripping glove covers its shaft; VFX and death-cel placement belong to the builder.
for part in ('weapon', 'hand_l'):
    d['order'].remove(part)
    d['order'].append(part)
DEATH_RELEASE = .1

# ---------------------------------------------------------------- helpers
def smooth(u): return u * u * (3 - 2 * u)

def key(t, keys):
    # Monotone quintic Hermite: shared velocity and zero acceleration at each key. Near-straight
    # planted IK chains otherwise turn a cubic's initial acceleration into a sharp knee snap.
    for i, ((a, x), (b, y)) in enumerate(zip(keys, keys[1:])):
        if t <= b:
            h = b-a; u = max(0, min(1, (t-a)/h))
            def tangent(k):
                if k == 0 or k == len(keys)-1: return 0.0
                h0 = keys[k][0]-keys[k-1][0]; h1 = keys[k+1][0]-keys[k][0]
                s0 = (keys[k][1]-keys[k-1][1])/h0; s1 = (keys[k+1][1]-keys[k][1])/h1
                if s0*s1 <= 0: return 0.0
                w0, w1 = 2*h1+h0, h1+2*h0
                m=(w0+w1)/(w0/s0+w1/s1)
                return math.copysign(min(abs(m),1.5*min(abs(s0),abs(s1))),m)
            return (1-10*u**3+15*u**4-6*u**5)*x + (u-6*u**3+8*u**4-3*u**5)*h*tangent(i) + (10*u**3-15*u**4+6*u**5)*y + (-4*u**3+7*u**4-3*u**5)*h*tangent(i+1)
    return keys[-1][1]

def rot(p, a):
    c, s = math.cos(a), math.sin(a); return [c * p[0] - s * p[1], s * p[0] + c * p[1]]
def add(a, b): return [a[0] + b[0], a[1] + b[1]]
def sub(a, b): return [a[0] - b[0], a[1] - b[1]]
def angle(p): return math.atan2(p[1], p[0])
def length(p): return math.hypot(*p)

def solve(root, target, first, second, branch):
    delta = sub(target, root); a, b = length(first), length(second)
    distance = max(abs(a - b) + .01, min(a + b - .01, length(delta)))
    off = math.acos(max(-1, min(1, (a * a + distance * distance - b * b) / (2 * a * distance))))
    heading = angle(delta) + branch * off; elbow = add(root, rot([a, 0], heading))
    return heading - angle(first), angle(sub(target, elbow)) - angle(second)

def solve_signed_reach(root, target, first, second, bend):
    """Attack IK with an authored signed bend and an exactly reachable straight sample.

    Reach is derived from the signed bend below. Unlike the planted-chain solver,
    this permits full extension: both branches coincide there. atan2 keeps the
    shoulder offset continuous through zero without an acos precision cusp.
    """
    a, b = length(first), length(second)
    heading = angle(sub(target, root)) - math.atan2(b * math.sin(bend), a + b * math.cos(bend))
    return heading - angle(first), heading + bend - angle(second)

def world(joints, n):
    """Forward kinematics identical to the Bone2D chain built by rig.gd: returns (origin, rotation)."""
    parent = rest[n][2]; j = joints[n]
    if parent is None: return list(j['position']), j['rotation']
    po, pr = world(joints, parent)
    return add(po, rot(j['position'], pr)), pr + j['rotation']

def unwrap(samples):
    for prev, cur in zip(samples, samples[1:]):
        for n in cur['joints']:
            r = cur['joints'][n]['rotation']; p = prev['joints'][n]['rotation']
            while r - p > math.pi: r -= math.tau
            while r - p < -math.pi: r += math.tau
            cur['joints'][n]['rotation'] = r

# Rigid weapon silhouette in weapon-bone space (rig.gd offsets the sprite by -grip).
alpha = np.asarray(rig_parts.canvas('weapon-rigid').getchannel('A'))
ys, xs = np.nonzero(alpha > 24)
def alpha_edges(mask):
    """Convex support hull of every nonzero-alpha pixel's four edges, not just opaque centers."""
    pts=[]
    for y in np.flatnonzero(mask.any(1)):
        row=np.flatnonzero(mask[y]);a,b=float(row[0]-131),float(row[-1]+1-131)
        pts.extend([(a,float(y-278)),(b,float(y-278)),(a,float(y+1-278)),(b,float(y+1-278))])
    pts=sorted(set(pts))
    def cross(o,a,b):return (a[0]-o[0])*(b[1]-o[1])-(a[1]-o[1])*(b[0]-o[0])
    def half(points):
        out=[]
        for p in points:
            while len(out)>1 and cross(out[-2],out[-1],p)<=0:out.pop()
            out.append(p)
        return out
    return np.array(half(pts)[:-1]+half(reversed(pts))[:-1])
WEAPON=alpha_edges(alpha>0)
brush_mask=alpha>0;brush_mask[:399]=False
BRUSH=alpha_edges(brush_mask)
# Fit the actual wooden shaft (source rows 220..380), not the PNG's left edge.
shaft_rows = [(y, float(xs[ys == y].mean())) for y in range(220, 381) if np.any(ys == y)]
SHAFT_SLOPE, SHAFT_X = np.polyfit(np.array(shaft_rows)[:, 0] - 278, np.array(shaft_rows)[:, 1], 1)
SHAFT_ANGLE = -math.atan(float(SHAFT_SLOPE))
# Put edge-on poses on common 24/60-Hz frames so a real display sample sees the axial turn.
TURN_KEYS = [(0, 1.0), (.05, 1.0), (1/12, .035), (.1, -1.0),
             (.64, -1.0), (2/3, -.035), (.7, 1.0), (1.05, 1.0)]

def weapon_points(points, turn=1.0):
    q = np.array([float(SHAFT_X) - 131, 0.0])
    c, s = math.cos(SHAFT_ANGLE), math.sin(SHAFT_ANGLE)
    R = np.array([[c, -s], [s, c]])
    p = (points - q) @ R
    p[:, 0] *= turn
    return p @ R.T + q

def weapon_turn(t):
    # Match AnimationPlayer's linear value tracks exactly.
    for (a, x), (b, y) in zip(TURN_KEYS, TURN_KEYS[1:]):
        if t <= b: return x + (y-x) * max(0, min(1, (t-a)/(b-a)))
    return TURN_KEYS[-1][1]
SETUP_LOW = 278 + BRUSH[:, 1].max()   # source broom bottom
BOOT_LOW = 523                         # rear boot bottom in source (foot_l alpha bbox)
FLOOR = 527.0                         # source floor, independent of alpha cleanup / bounds

def brush_low(joints, extra=0.0, turn=1.0):
    o, r = world(joints, 'weapon'); r += extra
    c, s = math.cos(r), math.sin(r)
    brush = weapon_points(BRUSH, turn)
    return float((o[1] + s * brush[:, 0] + c * brush[:, 1]).max())

def author_idle():
    """Quiet combat breathing; feet and grip have independent targets."""
    for name in ('idle_loop',):
        clip = d['clips'][name]; base = copy.deepcopy(clip['samples'][0]['joints']); samples = []
        L = clip['length']
        for i in range(round(L*60)+1):
            t = i/60; phase = math.tau*t/L; breath = phase
            hx = 240 + .65*math.sin(phase)
            hy = 338 + .8*(1-math.cos(breath))
            j = copy.deepcopy(base); j['hip']['position'] = [hx,hy]
            torso = .006*math.sin(breath)
            j['torso']['rotation'] = torso
            j['head']['rotation'] = base['head']['rotation'] - torso*.65 + .003*math.sin(breath)
            j['upper_arm_r']['rotation'] = base['upper_arm_r']['rotation'] + .009*math.sin(breath)
            j['forearm_r']['rotation'] = base['forearm_r']['rotation'] - .012*math.sin(breath-.3) + .012*math.sin(-.3)
            shoulder = add(add([hx,hy],j['torso']['position']),rot(j['upper_arm_l']['position'],torso))
            target = [131+.15*math.sin(phase),278-.2*(1-math.cos(breath))]
            a,b = solve(shoulder,target,j['forearm_l']['position'],j['hand_l']['position'],-1)
            shaft = .002*math.sin(phase)
            j['upper_arm_l']['rotation']=a-torso; j['forearm_l']['rotation']=b-a
            j['hand_l']['rotation']=shaft-b; j['weapon']['rotation']=0.0
            for side in 'lr':
                n,sh,ft = 'thigh_'+side,'shin_'+side,'foot_'+side
                branch = min((-1,1),key=lambda b:abs(solve(rest[n][:2],rest[ft][:2],j[sh]['position'],j[ft]['position'],b)[0]))
                a,b=solve(add([hx,hy],j[n]['position']),rest[ft][:2],j[sh]['position'],j[ft]['position'],branch)
                j[n]['rotation']=a; j[sh]['rotation']=b-a; j[ft]['rotation']=-b
            samples.append({'time':t,'joints':j})
        samples[-1]['joints']=copy.deepcopy(samples[0]['joints'])
        unwrap(samples)
        clip['samples']=samples

author_idle()

def author_relaxed():
    """Settled shop stance: narrow planted feet, weight over the left leg, a low loose grip.

    The grip moves down the existing shaft, rather than lifting the broom through the floor or
    folding the upper arm into its pauldron. The weapon-bone translation is a fixed material
    attachment: hand and weapon share a world rotation and remain rigidly connected.
    """
    clip=d['clips']['relaxed_loop']; L=8.0
    base=d['clips']['idle_loop']['samples'][0]['joints']; samples=[]
    grip=[float(SHAFT_X)+float(SHAFT_SLOPE)*22,300.0]
    grip_delta=sub(grip,[131,278])
    feet={'l':[224,466],'r':[279,466]}
    branches={side:min((-1,1),key=lambda b:abs(solve(rest['thigh_'+side][:2],rest['foot_'+side][:2],base['shin_'+side]['position'],base['foot_'+side]['position'],b)[0])) for side in 'lr'}
    for i in range(round(L*60)+1):
        t=i/60; phase=math.tau*t/L; breath=phase*2
        j=copy.deepcopy(base)
        hx=242+1.0*math.sin(phase); hy=338.5+.65*(1-math.cos(breath))
        j['hip']['position']=[hx,hy]
        torso=-.028+.004*math.sin(breath)+.003*math.sin(phase)
        j['torso']['rotation']=torso
        j['head']['rotation']=.025-torso*.7+.002*math.sin(breath)
        shaft=.035+.002*math.sin(phase)
        c,s=math.cos(shaft),math.sin(shaft)
        # Plant the complete rigid bristle silhouette just above the boot support plane.
        origin=[139.0,525.0-float((s*WEAPON[:,0]+c*WEAPON[:,1]).max())]
        target=add(origin,rot(grip_delta,shaft))
        chest=add([hx,hy],j['torso']['position'])
        shoulder=add(chest,rot(j['upper_arm_l']['position'],torso))
        a,b=solve(shoulder,target,j['forearm_l']['position'],j['hand_l']['position'],-1)
        j['upper_arm_l']['rotation']=a-torso; j['forearm_l']['rotation']=b-a
        j['hand_l']['rotation']=shaft-b
        j['weapon']['position']=[-grip_delta[0],-grip_delta[1]]
        j['weapon']['rotation']=0.0
        # Existing downward glove rests beside the belt; an invented palm-on-hip drawing is
        # unnecessary. Keep the elbow outside the torso and the wrist softly aligned.
        shoulder=add(chest,rot(j['upper_arm_r']['position'],torso))
        target=add([hx,hy],rot([39,-28],torso))
        a,b=solve(shoulder,target,j['forearm_r']['position'],j['hand_r']['position'],-1)
        j['upper_arm_r']['rotation']=a-torso; j['forearm_r']['rotation']=b-a
        j['hand_r']['rotation']=-.18
        for side in 'lr':
            n,sh,ft='thigh_'+side,'shin_'+side,'foot_'+side
            a,b=solve(add([hx,hy],j[n]['position']),feet[side],j[sh]['position'],j[ft]['position'],branches[side])
            j[n]['rotation']=a; j[sh]['rotation']=b-a; j[ft]['rotation']=-b
        samples.append({'time':t,'joints':j})
    unwrap(samples)
    samples[-1]['joints']=copy.deepcopy(samples[0]['joints'])
    clip.update({'length':L,'loop':True,'samples':samples})
    observations['relaxed']={'grip_source':grip,'foot_targets':feet,'support_source_y':525.0,
        'upper_arm_l_rotation_range':[min(s['joints']['upper_arm_l']['rotation'] for s in samples),max(s['joints']['upper_arm_l']['rotation'] for s in samples)],
        'breaths_per_loop':2,'pose_endpoint_identical':samples[0]['joints']==samples[-1]['joints']}

author_relaxed()

# ---------------------------------------------------------------- attack
def author_attack():
    base = d['clips']['idle_loop']['samples'][0]['joints']; samples = []
    upper_length = length(base['forearm_l']['position'])
    fore_length = length(base['hand_l']['position'])
    def bend_for_reach(reach):
        return math.acos(max(-1,min(1,(reach*reach-upper_length**2-fore_length**2)/(2*upper_length*fore_length))))
    leg_branch = {}
    for side in 'lr':
        root = rest['thigh_' + side][:2]; first = base['shin_' + side]['position']; second = base['foot_' + side]['position']
        leg_branch[side] = min([-1, 1], key=lambda b: abs(solve(root, rest['foot_' + side][:2], first, second, b)[0]))
    for i in range(253):
        t = i / 240; j = copy.deepcopy(base)
        hx = key(t, [(0, 240), (.055, 233), (.15, 272), (.22, 275), (.4, 271), (.65, 253), (.9, 240), (1.05, 240)])
        hy = key(t, [(0, 338), (.055, 344), (.15, 356), (.22, 358), (.4, 356), (.65, 345), (.9, 338), (1.05, 338)])
        torso = key(t, [(0, 0), (.055, -.09), (.15, .23), (.24, .27), (.4, .23), (.65, .06), (.9, -.015), (1.05, 0)])
        j['hip'] = {'position': [hx, hy], 'rotation': 0.0}; j['torso']['rotation'] = torso
        j['head']['rotation'] = key(t, [(0, base['head']['rotation']), (.055, .05), (.15, -.21), (.25, -.23), (.45, -.16), (.7, .015), (1.05, base['head']['rotation'])])
        chest = add([hx, hy], j['torso']['position']); shoulder = add(chest, rot(j['upper_arm_l']['position'], torso))
        # The arm reaches straight just after contact, then folds on the opposite side.
        # A second straight reach during the return restores the upright setup branch.
        # A small material slide along the shaft lets the arm extend without moving the
        # established .15s weapon/contact pose (source origin 310,280; rotation pi/2).
        slide = key(t,[(0,0),(.035,0),(.15,-14),(.28,-14),(.55,-8),(.85,0),(1.05,0)])
        grip_delta = [float(SHAFT_SLOPE)*slide,slide]
        contact_shoulder = add([272,307],rot([-42,-50],.23))
        contact_hand = add([310,280],rot([float(SHAFT_SLOPE)*-14,-14],math.pi/2))
        contact_delta = sub(contact_hand,contact_shoulder)
        bend = key(t,[(0,bend_for_reach(length(sub([131,278],[198,239])))),
                      (.035,bend_for_reach(78)),(1/12,bend_for_reach(80)),
                      (.15,bend_for_reach(length(contact_delta))),(11/60,0),
                      (.28,math.radians(-28)),(.42,math.radians(-40)),(.5,math.radians(-32)),
                      (.65,0),(.85,math.radians(54)),
                      (1.05,bend_for_reach(length(sub([131,278],[198,239]))))])
        # cos(bend) makes the hand distance peak quadratically at each zero crossing;
        # interpolating distance alone would produce a square-root angular cusp.
        reach = math.sqrt(upper_length**2+fore_length**2+2*upper_length*fore_length*math.cos(bend))
        heading = key(t,[(0,angle(sub([131,278],[198,239]))),(.025,math.radians(158)),
                         (.07,math.radians(110)),(.115,math.radians(45)),(.15,angle(contact_delta)),
                         (.28,math.radians(24)),(.45,math.radians(48)),(.65,math.radians(105)),
                         (.85,math.radians(150)),(1.05,angle(sub([131,278],[198,239])))])
        target = add(shoulder,rot([reach,0],heading))
        # Turn before the hand descends. Keep the returning shaft sufficiently horizontal
        # through .60..65 too: floor clipping beside the .64 twist would kink the hand arc.
        shaft = key(t, [(0, 0), (.015,.15),(.045,.85),(.083333,1.15),(.115,1.40),(.15, math.pi / 2), (.23, 1.68), (.38, 1.56), (.6, 1.25), (.72, .95), (.84, .42), (.93, .02), (1.05, 0)])
        # Raise the actual grip/arm through wind-up when the wide broom requires clearance.
        # Moving only the prop by this amount would pull it out of the fist. The event pose
        # clears without correction; the short setup/plant margins ease to zero at endpoints.
        shape=weapon_points(WEAPON,weapon_turn(t))
        relative_low=float((math.sin(shaft)*shape[:,0]+math.cos(shaft)*shape[:,1]).max())
        margin=smooth(max(0,min(1,t/.03,(1.05-t)/.03)))
        max_hand_y = FLOOR-margin-relative_low+rot(grip_delta,shaft)[1]
        if target[1] > max_hand_y:
            # Lift along the reach circle, not vertically off it: a translated target
            # would shorten the chain and destroy a scheduled straight crossing.
            limit = math.asin(max(-1,min(1,(max_hand_y-shoulder[1])/reach)))
            heading = limit if math.cos(heading) >= 0 else math.pi-limit
            target = add(shoulder,rot([reach,0],heading))
        a, b = solve_signed_reach(shoulder, target, j['forearm_l']['position'], j['hand_l']['position'], bend)
        j['upper_arm_l']['rotation'] = a - torso; j['forearm_l']['rotation'] = b - a
        # A pole can roll/turn in the grip without forcing a 90-degree wrist break. Keep the
        # glove aligned to the forearm and give the prop the remaining world rotation.
        # Explicit wrist sharing avoids feeding an unwrapped atan2 elbow angle to tanh:
        # that used to saturate +.4 even when the wrist should recoil the opposite way.
        wrist = key(t,[(0,0),(.035,-.08),(1/12,.02),(.15,.10),(.3,.08),
                       (.5,.04),(.7,-.05),(.9,-.03),(1.05,0)])
        j['hand_l']['rotation'] = wrist; j['weapon']['rotation'] = shaft-b-wrist
        j['weapon']['position'] = rot([-v for v in grip_delta],shaft-b-wrist)
        free_shoulder=add(chest,rot(j['upper_arm_r']['position'],torso))
        free_target=add([hx,hy],[key(t,[(0,60),(.055,62),(.15,24),(.35,28),(.7,50),(1.05,60)]),
                                     key(t,[(0,-12),(.055,-18),(.15,-32),(.35,-20),(.7,-14),(1.05,-12)])])
        fa,fb=solve(free_shoulder,free_target,j['forearm_r']['position'],j['hand_r']['position'],1)
        j['upper_arm_r']['rotation']=fa-torso; j['forearm_r']['rotation']=fb-fa
        j['hand_r']['rotation']=key(t,[(0,0),(.15,-.08),(.45,-.04),(1.05,0)])
        j['coat']['rotation'] = key(t, [(0, base['coat']['rotation']), (.075, .04), (.19, -.14), (.32, -.18), (.55, .07), (.78, -.025), (1.05, base['coat']['rotation'])])
        j['hat_tip']['rotation'] = key(t, [(0, base['hat_tip']['rotation']), (.08, .04), (.2, -.13), (.35, -.08), (.58, .05), (.8, -.025), (1.05, base['hat_tip']['rotation'])])
        targets = {'l': [209, 466], 'r': [key(t, [(0, 290), (.04, 290), (.14, 334), (.48, 334), (.83, 290), (1.05, 290)]),
                                          key(t, [(0, 466), (.04, 466), (.085, 454), (.14, 466), (.48, 466), (.65, 456), (.83, 466), (1.05, 466)])]}
        for side in 'lr':
            n = 'thigh_' + side; shin = 'shin_' + side; foot = 'foot_' + side
            root = add([hx, hy], j[n]['position'])
            a2, b2 = solve(root, targets[side], j[shin]['position'], j[foot]['position'], leg_branch[side])
            j[n]['rotation'] = a2; j[shin]['rotation'] = b2 - a2; j[foot]['rotation'] = -b2
        samples.append({'time': t, 'joints': j})
    unwrap(samples)

    # Keep the intended wrist/shaft angles. Dense full-alpha support translation below handles
    # contact: an angular correction near vertical first LOWERS the wide broom before lifting it.
    impact = next(k for k, s in enumerate(samples) if abs(s['time'] - .15) < 1e-9)
    assert all(brush_low(samples[k]['joints'],turn=weapon_turn(samples[k]['time']))<=FLOOR for k in (0,impact,len(samples)-1)), 'setup and strike keys must clear unaided'
    d['clips']['attack']['samples'] = samples
    lows = [round(brush_low(s['joints'], turn=weapon_turn(s['time'])), 2) for s in samples]
    observations['attack'] = {
        'floor_source_y': FLOOR, 'setup_brush_low': float(SETUP_LOW), 'rear_boot_low': BOOT_LOW,
        'raw_max_brush_low_keys': max(lows), 'max_extra_shaft_rad':0.0,
        'shaft_axis_x_at_grip': float(SHAFT_X), 'shaft_axis_clockwise_rad': SHAFT_ANGLE,
        'elbow_straight_times': [11/60, .65], 'arm_reach_source_px': upper_length+fore_length,
    }
    return samples

author_attack()

# ---------------------------------------------------------------- hurt
def author_hurt():
    """Short torso/head recoil with a slight supported knee give, rather than a deep straddle."""
    base = d['clips']['idle_loop']['samples'][0]['joints']; samples = []
    leg_branch = {}
    for side in 'lr':
        root = rest['thigh_' + side][:2]
        leg_branch[side] = min([-1, 1], key=lambda b: abs(solve(root, rest['foot_' + side][:2], base['shin_' + side]['position'], base['foot_' + side]['position'], b)[0]))
    L = .8
    for i in range(49):
        t = i / 60; j = copy.deepcopy(base)
        hx = key(t, [(0, 240), (.07, 236), (.15, 235), (.30, 238), (.48, 240), (.8, 240)])
        hy = key(t, [(0, 338), (.08, 341), (.15, 342), (.30, 339), (.48, 338), (.8, 338)])
        torso = key(t, [(0, 0), (.05, -.3), (.12, -.13), (.22, .06), (.4, .04), (.62, -.02), (.8, 0)])
        j['hip'] = {'position': [hx, hy], 'rotation': 0.0}; j['torso']['rotation'] = torso
        j['head']['rotation'] = key(t, [(0, base['head']['rotation']), (.04, -.2), (.12, .05), (.24, .14), (.45, .06), (.66, -.02), (.8, base['head']['rotation'])])
        # Tilt about the lower jaw/collar contact, preserving the painted neck overlap.
        neck=[14,6]
        j['head']['position']=add(base['head']['position'],sub(rot(neck,base['head']['rotation']),rot(neck,j['head']['rotation'])))
        chest = add([hx, hy], j['torso']['position']); shoulder = add(chest, rot(j['upper_arm_l']['position'], torso))
        target = [key(t, [(0, 131), (.06, 140), (.18, 152), (.38, 148), (.62, 130), (.8, 131)]),
                  key(t, [(0, 278), (.06, 270), (.18, 268), (.38, 271), (.62, 279), (.8, 278)])]
        a, b = solve(shoulder, target, j['forearm_l']['position'], j['hand_l']['position'], -1)
        shaft = key(t, [(0, 0), (.06, -.14), (.18, -.1), (.4, -.05), (.62, .015), (.8, 0)])
        j['upper_arm_l']['rotation'] = a - torso; j['forearm_l']['rotation'] = b - a; j['hand_l']['rotation'] = shaft - b; j['weapon']['rotation'] = 0.0
        j['upper_arm_r']['rotation'] = key(t, [(0, 0), (.05, .22), (.14, -.32), (.32, -.36), (.58, -.1), (.8, 0)])
        j['forearm_r']['rotation'] = key(t, [(0, 0), (.05, .12), (.14, -.75), (.32, -.82), (.6, -.2), (.8, 0)])
        j['coat']['rotation'] = key(t, [(0, base['coat']['rotation']), (.08, .1), (.2, -.06), (.4, .035), (.62, -.01), (.8, base['coat']['rotation'])])
        j['hat_tip']['rotation'] = key(t, [(0, base['hat_tip']['rotation']), (.06, .18), (.18, -.1), (.36, .06), (.58, -.02), (.8, base['hat_tip']['rotation'])])
        for side in 'lr':
            n = 'thigh_' + side; shin = 'shin_' + side; foot = 'foot_' + side
            root=add([hx,hy],j[n]['position'])
            old_a,old_b=solve(root,rest[foot][:2],j[shin]['position'],j[foot]['position'],leg_branch[side])
            forward_a,forward_b=solve(root,rest[foot][:2],j[shin]['position'],j[foot]['position'],-1)
            blend=key(t,[(0,0),(.06,1),(.36,1),(.60,0),(.8,0)])
            a2=old_a+(forward_a-old_a)*blend;b2=old_b+(forward_b-old_b)*blend
            j[n]['rotation'] = a2; j[shin]['rotation'] = b2 - a2; j[foot]['rotation'] = -b2
        samples.append({'time': t, 'joints': j})
    unwrap(samples)
    d['clips']['hurt'].update({'length': L, 'samples': samples})
    lows = [brush_low(s['joints']) for s in samples]
    observations['hurt'] = {'samples': len(samples), 'max_brush_low': round(max(lows), 2), 'floor_source_y': FLOOR}

author_hurt()

# ---------------------------------------------------------------- die
import death_poses as dp
def author_die():
    """Supported continuous buckle, then registered full-body paintings; no lower-body swaps."""
    seq=json.loads((P/'death-sequence.json').read_text()); d['death_sequence']=seq
    base=d['clips']['idle_loop']['samples'][0]['joints']; samples=[]
    branches={side:min((-1,1),key=lambda b:abs(solve(rest['thigh_'+side][:2],rest['foot_'+side][:2],base['shin_'+side]['position'],base['foot_'+side]['position'],b)[0])) for side in 'lr'}
    for i in range(133):
        t=min(i/60,seq['start_time']); j=copy.deepcopy(base)
        hx=key(t,[(0,240),(.08,239),(.18,238),(.3,240)])
        hy=key(t,[(0,338),(.08,350),(.18,385),(.3,435)])
        torso=key(t,[(0,0),(.06,-.06),(.18,.06),(.3,.12)])
        j['hip']['position']=[hx,hy];j['torso']['rotation']=torso
        j['head']['rotation']=key(t,[(0,base['head']['rotation']),(.08,.06),(.3,.1)])
        for side in 'lr':
            th,sh,ft='thigh_'+side,'shin_'+side,'foot_'+side
            # Both knees buckle toward battle-forward (+X), while the feet draw under the hip.
            # Blend out of setup before shortening support; never mirror the two knee branches.
            target=[key(t,[(0,rest[ft][0]),(.07,rest[ft][0]),(.24,225 if side=='l' else 273),(.3,225 if side=='l' else 273)]),466]
            root=add([hx,hy],j[th]['position'])
            aa,bb=solve(root,target,j[sh]['position'],j[ft]['position'],branches[side])
            fa,fb=solve(root,target,j[sh]['position'],j[ft]['position'],-1)
            blend=key(t,[(0,0),(.055,1),(.3,1)])
            a=aa+(fa-aa)*blend;b=bb+(fb-bb)*blend
            j[th]['rotation']=a;j[sh]['rotation']=b-a
            j[ft]['rotation']=-b+key(t,[(0,0),(.18,0),(.3,.1)])
        # Retain the grip until release; after that the empty hand descends with the body.
        chest=add([hx,hy],j['torso']['position']);shoulder=add(chest,rot(j['upper_arm_l']['position'],torso))
        target=[131,278+max(0,hy-key(DEATH_RELEASE,[(0,338),(.08,350),(.18,385),(.3,435)]))]
        a,b=solve(shoulder,target,j['forearm_l']['position'],j['hand_l']['position'],-1)
        j['upper_arm_l']['rotation']=a-torso;j['forearm_l']['rotation']=b-a;j['hand_l']['rotation']=-b
        j['weapon']['rotation']=0.0
        # Brief outward balance clears the coat before the shop back-slot handoff.
        # Return exactly to the existing buckle before the captured cel boundary.
        j['upper_arm_r']['rotation']=key(t,[(0,0),(.3,.08)])-.38*key(t,[(0,0),(.1,1),(.18,1),(.27,0)])
        j['forearm_r']['rotation']=key(t,[(0,0),(.3,-.12)])
        # Fold the lower cloth toward the ground while the waist stays attached to the hip.
        compress=max(0,hy-400)*1.05
        j['coat_hem']={'position':[0,102-compress],'rotation':0.0}
        samples.append({'time':i/60,'joints':j})
    unwrap(samples);d['clips']['die']['samples']=samples
    d['clips']['die']['lower']={'full':{'part':'death-full','topleft':list(dp.FULL_CEL_TOPLEFT)}}
    observations['die']={'rig_buckle_until':seq['start_time'],'painted_frames':len(seq['frames']),
                         'final_time':seq['final_time'],'settled_time':seq['settled_time']}

author_die()

# ---------------------------------------------------------------- cast
def author_cast():
    """Spell release at CastAnimDelay .25 s: pull the free hand in, then thrust it out with the whole body."""
    base = d['clips']['idle_loop']['samples'][0]['joints']; samples = []
    leg_branch = {}
    for side in 'lr':
        root = rest['thigh_' + side][:2]
        leg_branch[side] = min([-1, 1], key=lambda b: abs(solve(root, rest['foot_' + side][:2], base['shin_' + side]['position'], base['foot_' + side]['position'], b)[0]))
    L = 1.3
    for i in range(79):
        t = i / 60; j = copy.deepcopy(base)
        hx = key(t, [(0, 240), (.12, 235), (.25, 248), (.5, 246), (.9, 239.5), (1.3, 240)])
        hy = key(t, [(0, 338), (.12, 343), (.25, 342), (.5, 341), (.9, 337.5), (1.3, 338)])
        torso = key(t, [(0, 0), (.12, -.09), (.25, .13), (.45, .1), (.85, -.012), (1.3, 0)])
        j['hip'] = {'position': [hx, hy], 'rotation': 0.0}; j['torso']['rotation'] = torso
        j['head']['rotation'] = key(t, [(0, base['head']['rotation']), (.12, -.08), (.27, .06), (.5, .04), (.9, -.01), (1.3, base['head']['rotation'])])
        chest = add([hx, hy], j['torso']['position']); shoulder = add(chest, rot(j['upper_arm_l']['position'], torso))
        target = [key(t, [(0, 131), (.12, 134), (.25, 136), (.5, 135), (.9, 131), (1.3, 131)]), key(t, [(0, 278), (.12, 270), (.25, 274), (.5, 275), (.9, 278), (1.3, 278)])]
        a, b = solve(shoulder, target, j['forearm_l']['position'], j['hand_l']['position'], -1)
        shaft = key(t, [(0, 0), (.12, -.1), (.25, .06), (.5, .04), (.9, -.01), (1.3, 0)])
        j['upper_arm_l']['rotation'] = a - torso; j['forearm_l']['rotation'] = b - a; j['hand_l']['rotation'] = shaft - b; j['weapon']['rotation'] = 0.0
        j['upper_arm_r']['rotation'] = key(t, [(0, 0), (.12, .2), (.24, -1.38), (.3, -1.3), (.55, -1.22), (.95, -.12), (1.3, 0)])
        j['forearm_r']['rotation'] = key(t, [(0, 0), (.12, -1.2), (.24, -.2), (.3, -.3), (.55, -.36), (.95, -.08), (1.3, 0)])
        j['hand_r']['rotation'] = key(t, [(0, 0), (.12, .2), (.24, -.45), (.55, -.4), (.95, 0), (1.3, 0)])
        j['coat']['rotation'] = key(t, [(0, base['coat']['rotation']), (.14, .05), (.3, -.08), (.55, .03), (.9, -.01), (1.3, base['coat']['rotation'])])
        j['hat_tip']['rotation'] = key(t, [(0, base['hat_tip']['rotation']), (.15, .1), (.32, -.12), (.6, .05), (.95, -.02), (1.3, base['hat_tip']['rotation'])])
        for side in 'lr':
            n = 'thigh_' + side; shin = 'shin_' + side; foot = 'foot_' + side
            a2, b2 = solve(add([hx, hy], j[n]['position']), rest[foot][:2], j[shin]['position'], j[foot]['position'], leg_branch[side])
            j[n]['rotation'] = a2; j[shin]['rotation'] = b2 - a2; j[foot]['rotation'] = -b2
        samples.append({'time': t, 'joints': j})
    unwrap(samples)
    d['clips']['cast'].update({'length': L, 'samples': samples})
    release = next(s for s in samples if abs(s['time'] - .25) < 1e-9)['joints']
    o, r = world(release, 'hand_r'); palm = add(o, rot([6, 16], r))
    observations['cast'] = {'samples': len(samples), 'release_time': .25, 'palm_at_release': [round(palm[0], 2), round(palm[1], 2)],
                            'max_brush_low': round(max(brush_low(s['joints']) for s in samples), 2)}
    return palm

CAST_PALM = author_cast()
def bake_death_prop():
    """Early release, gravity-driven toppling with continuous alpha support at both ends.

    The rigid rod never leaves its floor contact after release. Its final angle balances the
    blade and bristle support heights rather than holding the blade in midair. Dense overlay
    keys use the same linearly interpolated FK as Godot, including the held interval.
    """
    samples=d['clips']['die']['samples']; release=world(samples[round(DEATH_RELEASE*60)]['joints'],'weapon')
    r0=release[1]; o0=release[0]
    blade=WEAPON[WEAPON[:,1]<-75]; brush=BRUSH
    lo,hi=math.pi/2,2.0
    for _ in range(50):
        r=(lo+hi)/2
        def low(points):return float((math.sin(r)*points[:,0]+math.cos(r)*points[:,1]).max())
        if low(brush)>low(blade):lo=r
        else:hi=r
    endr=(lo+hi)/2; endx=268.0
    tracks={'%Weapon:position':[], '%Weapon:rotation':[]}; world_poses=[]; lows=[]
    def pose(t):
        f=min(t*60,len(samples)-1);a=samples[int(f)]['joints'];b=samples[min(int(f)+1,len(samples)-1)]['joints'];u=f-int(f)
        return {n:{'position':[a[n]['position'][k]*(1-u)+b[n]['position'][k]*u for k in range(2)],'rotation':a[n]['rotation']*(1-u)+b[n]['rotation']*u} for n in a}
    for i in range(round(d['clips']['die']['length']*240)+1):
        t=i/240; j=pose(t); u=max(0,min(1,(t-DEATH_RELEASE)/.70)); ease=smooth(u)
        r=r0+(endr-r0)*ease
        x=o0[0]+(endx-o0[0])*ease
        highest_local_y=float((math.sin(r)*WEAPON[:,0]+math.cos(r)*WEAPON[:,1]).max())
        y=FLOOR-.30-highest_local_y
        if t<=DEATH_RELEASE:
            o,r=world(j,'weapon');x,y=o
            highest_local_y=float((math.sin(r)*WEAPON[:,0]+math.cos(r)*WEAPON[:,1]).max())
            # Subpixel support margin also covers FK interpolation between held bone keys.
            # Sliding the cylindrical pole by at most .301 source px keeps the glove wrapped.
            y=min(y,FLOOR-.30-highest_local_y)
            if y+highest_local_y>FLOOR:raise ValueError(f'die held prop below floor at {t}: {y+highest_local_y}')
        o,br=world(j,'weapon'); local=rot([x-o[0],y-o[1]],-br)
        tracks['%Weapon:position'].append([t,local]);tracks['%Weapon:rotation'].append([t,r-br])
        if i%4==0:world_poses.append(([x,y],r))
        lows.append(y+highest_local_y)
    d['death_prop_world']=world_poses
    observations['death_prop']={'release_time':DEATH_RELEASE,'settled_time':.8,'max_silhouette_y':max(lows),'floor_source_y':FLOOR,'rest_rotation':endr,'rest_origin':[endx,y],'dense_fk_hz':240}
    return tracks

DEATH_PROP_TRACKS=bake_death_prop()

def held_support(clip,name):
    """Floor support of the complete reflected alpha, including the rapid axial turn."""
    samples=clip['samples'];values=[];raw_lows=[];supported=[];shifts=[]
    hz=960 if name=='attack' else 240
    for i in range(round(clip['length']*hz)+1):
        t=i/hz;f=min(t/clip['length']*(len(samples)-1),len(samples)-1);a=samples[int(f)]['joints'];b=samples[min(int(f)+1,len(samples)-1)]['joints'];u=f-int(f)
        j={n:{'position':[a[n]['position'][k]*(1-u)+b[n]['position'][k]*u for k in range(2)],'rotation':a[n]['rotation']*(1-u)+b[n]['rotation']*u} for n in a}
        o,r=world(j,'weapon');points=weapon_points(WEAPON,weapon_turn(t) if name=='attack' else 1)
        low=o[1]+float((math.sin(r)*points[:,0]+math.cos(r)*points[:,1]).max())
        dy=min(0.,FLOOR-.30-low);values.append([t,rot([0,dy],-r)]);raw_lows.append(low);supported.append(low+dy);shifts.append(-dy)
    observations.setdefault(name,{})['held_support']={'sample_hz':hz,'raw_max_y':max(raw_lows),'supported_max_y':max(supported),'maximum_vertical_shift':max(shifts),'floor_source_y':FLOOR}
    return values

HELD_SUPPORT={name:held_support(clip,name) for name,clip in d['clips'].items() if name!='die'}
import secondary_motion
observations['secondary_motion'] = secondary_motion.bake(d, world, weapon_points, weapon_turn)

# ---------------------------------------------------------------- overlay tracks (expressions, VFX)
# Keyed on the same AnimationPlayer as the joints so external Play() owns them. Every clip gets the
# defaults at t=0 so switching clips can never leave a stale face or a lingering trail.
def tip_world(joints, t=0.0):
    ys_, xs_ = np.nonzero(alpha > 24); k = ys_.argmin(); tip = (xs_[k] - 131.0, ys_[k] - 278.0)
    tip = weapon_points(np.array([tip]), weapon_turn(t))[0]
    o, r = world(joints, 'weapon'); c, s = math.cos(r), math.sin(r)
    return (o[0] + c * tip[0] - s * tip[1], o[1] + s * tip[0] + c * tip[1])

def slash_placement():
    """Register the unchanged painting to the actual translated, axially turned tip path.

    A fixed circle cannot represent this swing: the weapon origin moves with the hand.
    The strip follows FK at 240 Hz; radial sections point back along the real blade.
    UV.x is travelled distance, UV.y is depth into the blade. The shader unfolds the
    existing painted crescent onto that strip and reveals only already swept sections.
    """
    a = np.asarray(rig_parts.canvas('slash-trail').getchannel('A'))
    yy, xx = np.nonzero(a > 180)
    cx, cy, _ = np.linalg.lstsq(np.c_[2*xx, 2*yy, np.ones(len(xx))], xx*xx+yy*yy, rcond=None)[0]
    yy, xx = np.nonzero(a > 24)
    angles = np.arctan2(yy-cy, xx-cx); radii = np.hypot(xx-cx, yy-cy)
    amin, amax = float(angles.min()), float(angles.max())
    inner, outer = [], []
    for angle_ in np.linspace(amin, amax, 128):
        rr = radii[np.abs(angles-angle_) < (amax-amin)/128]
        inner.append(float(rr.min()-3) if len(rr) else 0.)
        outer.append(float(rr.max()+3) if len(rr) else 0.)
    samples = d['clips']['attack']['samples']
    rows = []
    for i in range(61):
        t = i/240; j = samples[i]['joints']; o, r = world(j, 'weapon')
        support = HELD_SUPPORT['attack'][i*4][1]
        o = np.asarray(o) + rot(support, r)
        tip = np.asarray(tip_world(j,t)) + rot(support,r)
        # The bright cutting edge is the left alpha boundary before axial reflection.
        # Sample its curved contour, including the thin edge-on portion of the turn.
        edge=[]
        for y in np.linspace(36,108,9):
            yy=int(round(y)); xx=int(np.flatnonzero(alpha[yy]>24)[0])
            point=weapon_points(np.array([[xx-131.,y-278.]]),weapon_turn(t))[0]
            edge.append((o+rot(point,r)).tolist())
        edge[0]=tip.tolist()
        rows.append({'time':t, 'tip':tip.tolist(), 'edge':edge})
    distances = np.r_[0., np.cumsum(np.linalg.norm(np.diff([q['tip'] for q in rows],axis=0),axis=1))]
    for q, distance in zip(rows,distances):
        q['progress']=float(distance/distances[-1])
        tip=np.array(q['tip']); q['edge']=(tip+(np.array(q['edge'])-tip)*q['progress']**1.35).tolist()
    return {'part':'slash-trail', 'position':[0,0], 'rotation':0., 'scale':1.,
            'circle':[float(cx),float(cy)], 'angle_min':amin, 'angle_max':amax,
            'radial_inner':np.convolve(np.pad(inner,2,mode='edge'),np.ones(5)/5,mode='valid').tolist(),
            'radial_outer':np.convolve(np.pad(outer,2,mode='edge'),np.ones(5)/5,mode='valid').tolist(), 'sweep':rows,
            'sweep_length':float(distances[-1]), 'sample_hz':240}

d['vfx'] = {'slash': slash_placement(),
            # Face-on sigil squashed into a disc perpendicular to the thrusting arm; 1200 px painted diameter -> ~150 source px.
            'cast': {'part': 'cast-sigil', 'anchor': [627, 627], 'position': [CAST_PALM[0] + 10, CAST_PALM[1] - 2], 'scale': [150 / 1200 * .45, 150 / 1200], 'rotation': -.12}}
TEX = {'open': 'head', 'closed': 'head-closed', 'hurt': 'head-hurt', 'attack': 'head-attack',
       'grip': 'hand_l', 'grip-turned':'hand-l-turned'}
d['textures'] = TEX
for part in d['death_sequence']['frames']:
    d['textures'][part]=part
d['weapon_axis'] = {'x': float(SHAFT_X), 'angle': SHAFT_ANGLE}
d['track_defaults'] = {'head:texture': 'open', 'hand_l:texture':'grip', 'Slash:modulate:a': 0.0, 'Slash:scale_mul': 1.0, '.:lower_mode': 0,
                      '.:weapon_arm_front':False, '.:weapon_gripped':True,
                      '.:free_arm_depth_hold':False, '.:free_arm_back':False,
                      'CastFx:modulate:a': 0.0, 'CastFx:scale_mul': 1.0, 'CastFx/Sigil:rotation': 0.0,
                      'DeathFull:settle': [0, 0], 'DeathFull:rotation': 0.0, '%WeaponTurn:scale:x': 1.0,
                      '%Ribbon:rotation': 0.0, 'DeathFull:modulate:a': 0.0,
                      '%Weapon:position':[0,0], '%Weapon:rotation':0.0, '%ShaftCore:modulate:a':0.0}
d['track_defaults'].update({'%Ribbon:position':[-13,-84],'%Ribbon:scale':[1,1],
    'Slash:position':d['vfx']['slash']['position'],'Slash:rotation':d['vfx']['slash']['rotation'],
    'Slash:material:shader_parameter/reveal':0.0})
for k,part in enumerate(d['death_sequence']['frames']):
    node=f'DeathSeq{k:02}'
    d['track_defaults'][node+':modulate:a']=0.0
    d['track_defaults'][node+':settle']=[0,0]
    d['track_defaults'][node+':rotation']=0.0
tracks = {
    'idle_loop': {'head:texture': [[0, 'open'], [1.9, 'closed'], [1.985, 'open']]},
    'relaxed_loop': {'.:free_arm_back': [[0,True]], 'head:texture': [[0, 'open'], [2.4, 'closed'], [2.49, 'open'], [6.1, 'closed'], [6.18, 'open'], [6.3, 'closed'], [6.37, 'open']]},
    'attack': {'head:texture': [[0, 'open'], [.05, 'attack'], [.62, 'open']],
               '.:weapon_arm_front':[[0,False],[1/12,True],[.75,False]],
               'hand_l:texture':[[0,'grip'],[1/12,'grip-turned'],[2/3,'grip']], '%WeaponTurn:scale:x': TURN_KEYS,
               '%ShaftCore:modulate:a': [[0,0.0],[.05,0.0],[.075,1.0],[.091667,1.0],[.1,0.0],[.64,0.0],[.658333,1.0],[.675,1.0],[.7,0.0]],
               'Slash:modulate:a': [[0,0.0],[.025,.35],[.075,.85],[.15,1.0],[.25,.85],[.34,0.0]],
               'Slash:scale_mul': [[0, 1.0]]},
    'hurt': {'head:texture': [[0, 'hurt'], [.42, 'open']]},
    'cast': {'head:texture': [[0, 'open'], [.1, 'attack'], [.7, 'open']],
             'CastFx:modulate:a': [[0, 0.0], [.19, 0.0], [.25, 1.0], [.42, .9], [.78, 0.0]],
             'CastFx:scale_mul': [[0, .2], [.19, .2], [.25, 1.0], [.32, 1.08], [.78, 1.22]],
             'CastFx/Sigil:rotation': [[0, 0.0], [.19, -.5], [.78, .9]]},
    'die': {'head:texture': [[0, 'hurt'], [.10, 'closed']], '.:lower_mode': [[0, 0], [d['death_sequence']['start_time'], 4], [d['death_sequence']['final_time'], 3]],
            'DeathFull:settle': [[0,[0,0]],[d['death_sequence']['final_time'],[0,-2]],[d['death_sequence']['settled_time'],[0,0]]]},
}
sweep=d['vfx']['slash']['sweep']
tracks['attack']['Slash:material:shader_parameter/reveal']=[[q['time'],q['progress']] for q in sweep]
seq=d['death_sequence']; die_tracks=tracks['die']
# Keep the outgoing arm slot through native blends until the arm has cleared the
# coat silhouette. Death briefly reaches outward before returning to its captured buckle.
# These keys do not change ordinary combat entry: its existing slot is already front.
for name, clear_time in [('idle_loop',.25),('attack',1/30),('hurt',.1),('cast',.15),('die',11/60)]:
    tracks[name]['.:free_arm_depth_hold']=[[0,True],[clear_time,False]]
die_tracks['.:weapon_gripped']=[[0,True],[DEATH_RELEASE,False]]
die_tracks.update(DEATH_PROP_TRACKS)
die_tracks['DeathFull:modulate:a']=[[0,0.0],[seq['final_time']-seq['crossfade'],0.0],[seq['final_time'],1.0]]
elapsed=seq['start_time']
for k,part in enumerate(seq['frames']):
    duration=seq.get('durations',[seq['frame_step']]*len(seq['frames']))[k]
    t=elapsed;end=t+duration;elapsed=end;node=f'DeathSeq{k:02}'
    # Neighbouring full-body drawings overlap for one display frame. The first is an exact native
    # capture, so rig -> cel has no redraw; the final native lying painting remains untouched.
    die_tracks[node+':modulate:a']=[[0,0.0],[max(0,t-seq['crossfade']),0.0],[t,1.0],[end-.0001,1.0],[end,0.0]]
    if k==0: die_tracks[node+':modulate:a']=[[0,0.0],[t-.0001,0.0],[t,1.0],[end-.0001,1.0],[end,0.0]]
    # Tiny ground-preserving downward drift removes a rigid hold between drawings.
    die_tracks[node+':settle']=[[0,[0,0]],[t,[0,-1]],[end,[0,1]]]
    if part=='death-full':
        # Reuse the actual approved painting for the two landing drawings. Slight rotation eases
        # onto its side; the last pose is pixel-identical to DeathFull before the visibility handoff.
        a,b=(-.04,-.015) if k==len(seq['frames'])-2 else (-.015,0.0)
        die_tracks[node+':rotation']=[[0,0.0],[t,a],[end,b]]
        die_tracks[node+':settle']=[[0,[0,0]],[t,[0,-2]],[end,[0,-2]]]
    elif k>=5 and not part.startswith('death-settle-'):
        # Ground registration converges to the canonical final body's support plane. Earlier
        # sheets drifted down to y540 then jumped upward 24px at the final painting.
        lift=min(24,(k-4)*6)
        die_tracks[node+':settle']=[[0,[0,0]],[t,[0,-lift-1]],[end,[0,-lift+1]]]
for name, clip in d['clips'].items():
    clip['tracks'] = {**tracks.get(name, {}), **clip.pop('secondary_tracks', {})}
    if name in HELD_SUPPORT:clip['tracks']['%Weapon:position']=HELD_SUPPORT[name]
(P / 'rig.json').write_text(json.dumps(d, separators=(',', ':')))
observations['slash'] = {k: (round(v, 4) if isinstance(v, float) else v) for k, v in d['vfx']['slash'].items()}
(P / 'authoring-report.json').write_text(json.dumps(observations, indent=2))
print('overlay tracks:', {n: list(c['tracks']) for n, c in d['clips'].items()})
