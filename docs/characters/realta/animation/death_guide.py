"""Render the pose guide given to the image tool for death cels A/B (rigid part composite, no painting).

Each panel shows the actual source parts (torso, thighs, shins, boots) placed by the rig's forward
kinematics at the cel pose, with the coat hidden and the floor line drawn. Panel = source region
x 56..376, y 150..577 at 2.4x, so a returned drawing can be mapped back to source pixels exactly.
"""
from pathlib import Path
import json, math
from PIL import Image, ImageDraw
import death_poses as dp
import rig_parts

P = Path(__file__).resolve().parent
d = json.loads((P / 'rig.json').read_text()); rest = d['rest']
REGION = (56, 150, 376, 577); SCALE = 2.4

def world_affine(joints, n):
    """(a,b,c,d,e,f) for x' = a x + b y + c, y' = d x + e y + f: source rest pixel -> posed pixel."""
    def W(js, m):
        parent = rest[m][2]; j = js[m]; r = j['rotation']; px, py = j['position']
        local = (math.cos(r), -math.sin(r), px, math.sin(r), math.cos(r), py)
        if parent is None: return local
        A = W(js, parent)
        return (A[0]*local[0]+A[1]*local[3], A[0]*local[1]+A[1]*local[4], A[0]*local[2]+A[1]*local[5]+A[2],
                A[3]*local[0]+A[4]*local[3], A[3]*local[1]+A[4]*local[4], A[3]*local[2]+A[4]*local[5]+A[5])
    rest_joints = {m: {'position': ([rest[m][0], rest[m][1]] if rest[m][2] is None else
                                    [rest[m][0]-rest[rest[m][2]][0], rest[m][1]-rest[rest[m][2]][1]]), 'rotation': 0.0} for m in rest}
    Pw, R = W(joints, n), W(rest_joints, n)
    # R is a pure translation (rest rotations are zero): M = Pw * R^-1
    return (Pw[0], Pw[1], Pw[2] - Pw[0]*R[2] - Pw[1]*R[5], Pw[3], Pw[4], Pw[5] - Pw[3]*R[2] - Pw[4]*R[5])

def posed_layer(joints, part, bone):
    a, b, c, d_, e, f = world_affine(joints, bone)
    det = a * e - b * d_
    inv = (e/det, -b/det, (b*f - e*c)/det, -d_/det, a/det, (d_*c - a*f)/det)
    return rig_parts.canvas(part).transform((384, 640), Image.Transform.AFFINE, inv, Image.Resampling.BICUBIC)

def pose_joints(hip, knees, ankles, torso_rot, feet):
    j = json.loads(json.dumps(d['clips']['idle_loop']['samples'][0]['joints']))
    j['hip']['position'] = list(hip); j['torso']['rotation'] = torso_rot
    for s in 'lr':
        t, sh, ft = dp.leg_rotations(hip, knees[s], ankles[s], s, feet[s])
        j['thigh_' + s]['rotation'], j['shin_' + s]['rotation'], j['foot_' + s]['rotation'] = t, sh, ft
    return j

POSES = {
    'A': pose_joints(dp.CEL_A_HIP, dp.KNEE_A, dp.ANKLE_A, .12, dp.FOOT_A),
    'B': pose_joints(dp.CEL_B_HIP, dp.KNEE_B, dp.ANKLE_B, .2, dp.FOOT_B),
}
ORDER = ['thigh_r', 'shin_r', 'foot_r', 'thigh_l', 'shin_l', 'foot_l', 'torso']

def panel(joints, ground=True):
    canvas = Image.new('RGBA', (384, 640), (0, 0, 0, 0))
    for part in ORDER: canvas.alpha_composite(posed_layer(joints, part, part))
    crop = canvas.crop(REGION)
    out = Image.new('RGBA', crop.size, (255, 255, 255, 255)); out.alpha_composite(crop)
    if ground:
        ImageDraw.Draw(out).line((0, dp.FLOOR_Y - REGION[1], crop.width, dp.FLOOR_Y - REGION[1]), fill=(120, 120, 120, 255), width=1)
    return out.resize((round(crop.width * SCALE), round(crop.height * SCALE)), Image.Resampling.LANCZOS)

if __name__ == '__main__':
    a, b = panel(POSES['A']), panel(POSES['B'])
    guide = Image.new('RGBA', (a.width * 2, a.height), (255, 255, 255, 255))
    guide.paste(a, (0, 0)); guide.paste(b, (a.width, 0))
    guide.convert('RGB').save(P / 'death-cel-guide.png')
    json.dump({k: v for k, v in POSES.items()}, open(P / 'death-cel-guide-poses.json', 'w'), separators=(',', ':'))
    print('guide', guide.size)
