"""Bake this Realta rig-c model into native Godot 4.x bones, polygons and animation resources."""
from __future__ import annotations

import argparse
import json
import math
import struct
from pathlib import Path


def num(x):
    return format(x, '.10g') if abs(x) >= 1e-11 else '0'


def fnum(x):
    s = num(x)
    return s if ('.' in s or 'e' in s or 'E' in s) else s + '.0'


def vec(x, y):
    return f'Vector2({num(x)}, {num(y)})'


def packed(kind, values):
    return f'Packed{kind}Array({", ".join(map(num, values))})'


def local_frame(row):
    a = math.radians(row.get('rotation', 0))
    return math.cos(a), math.sin(a), row.get('x', 0), row.get('y', 0)


def point(frame, x, y):
    c, s, ox, oy = frame
    return c*x-s*y+ox, s*x+c*y+oy


def compose(parent, child):
    pc, ps, _, _ = parent
    c, s, x, y = child
    return pc*c-ps*s, ps*c+pc*s, *point(parent, x, y)


def order_at(setup, row):
    placed = [None]*len(setup)
    moved = {item['slot'] for item in row.get('offsets', [])}
    for item in row.get('offsets', []):
        i = setup.index(item['slot'])+item['offset']
        assert placed[i] is None
        placed[i] = item['slot']
    others = iter(n for n in setup if n not in moved)
    return [n if n is not None else next(others) for n in placed]


def compact(samples):
    result = []
    for time, value in samples:
        if result and abs(result[-1][0]-time) < 1e-9:
            result[-1] = (time, value)
        elif not result or result[-1][1] != value:
            result.append((time, value))
    return result


def godot_step_time(t):
    # Spine compares its float32 keys strictly to a double animation clock.
    # Godot's native step tracks select the next key early by its comparison
    # epsilon (10 µs at 1 s, relative above 1 s). Preserve the change boundary.
    if t == 0:
        return 0
    def f32(v):
        return struct.unpack('<f',struct.pack('<f',v))[0]
    source=f32(t); ideal=source+max(1.0,source)*.00001; chosen=f32(ideal)
    # Keep the original side at a human-authored decimal exactly on the beat.
    upward=source>t
    if (upward and chosen<ideal) or (not upward and chosen>ideal):
        bits=struct.unpack('<I',struct.pack('<f',chosen))[0]+(1 if upward else -1)
        chosen=struct.unpack('<f',struct.pack('<I',bits))[0]
    return chosen


BLEND_MODES = {
    'additive': 1,  # CanvasItemMaterial.BLEND_MODE_ADD
    'screen': 1,    # Closest built-in CanvasItemMaterial mode (Godot 4 has MIX=0, ADD=1, SUB=2, MUL=3, PREMULT_ALPHA=4)
    'multiply': 3,  # CanvasItemMaterial.BLEND_MODE_MUL
}
COLOR_TIMELINES = ('alpha', 'rgba', 'rgb')


def parse_hex_color(value):
    text = value.lstrip('#')
    if len(text) == 8:
        return tuple(int(text[i:i+2], 16) / 255.0 for i in (0, 2, 4, 6))
    if len(text) == 6:
        return (*tuple(int(text[i:i+2], 16) / 255.0 for i in (0, 2, 4)), 1.0)
    raise ValueError(f'Expected 6- or 8-digit hex color, got {value!r}')


def color_str(r, g, b, a):
    return f'Color({num(r)}, {num(g)}, {num(b)}, {num(a)})'


def _cubic(a, b, c, d, t):
    q = 1 - t
    return q*q*q*a + 3*q*q*t*b + 3*q*t*t*c + t*t*t*d


def _solve_cubic_t(begin, cx1, cx2, end, target_t):
    lo, hi = 0.0, 1.0
    for _ in range(24):
        mid = 0.5 * (lo + hi)
        if _cubic(begin, cx1, cx2, end, mid) < target_t:
            lo = mid
        else:
            hi = mid
    return 0.5 * (lo + hi)


def sample_channels(keys, extract_fn, default_val, allow_discrete=True):
    values = [extract_fn(k) for k in keys]
    has_step_change = any(
        first.get('curve') == 'stepped' and v0 != v1
        for first, v0, v1 in zip(keys, values, values[1:])
    )
    all_stepped = (
        allow_discrete
        and len(keys) >= 2
        and has_step_change
        and all(first.get('curve') == 'stepped' for first in keys[:-1])
    )
    if all_stepped:
        samples = []
        if keys[0]['time'] > 0:
            samples.append((0, default_val))
        for k, v in zip(keys, values):
            samples.append((godot_step_time(k['time']), v))
        return compact(samples), True

    samples = []
    if keys[0]['time'] > 0:
        t0 = keys[0]['time']
        samples.append((0, default_val))
        if values[0] != default_val and t0 > 1e-4:
            samples.append((t0 - 1e-4, default_val))
    for first, last, v0, v1 in zip(keys, keys[1:], values, values[1:]):
        begin, end = first['time'], last['time']
        curve = first.get('curve')
        samples.append((begin, v0))
        if isinstance(curve, list):
            # Spine's keyed BezierTimeline uses ten equal parameter segments;
            # native linear Godot keys reproduce the exact same lookup table.
            count = 10
            n_ch = len(v0)
            if len(curve) != 4 * n_ch:
                raise ValueError(f'Unexpected bezier curve length {len(curve)} for {n_ch} channels')
            same_x = all(curve[4*ch] == curve[0] and curve[4*ch+2] == curve[2] for ch in range(1, n_ch))
            for i in range(1, count):
                u = i / count
                if same_x:
                    t_s = _cubic(begin, curve[0], curve[2], end, u)
                    vals = tuple(_cubic(v0[ch], curve[4*ch+1], curve[4*ch+3], v1[ch], u) for ch in range(n_ch))
                else:
                    t_s = begin + (end - begin) * u
                    vals = tuple(
                        _cubic(v0[ch], curve[4*ch+1], curve[4*ch+3], v1[ch],
                               _solve_cubic_t(begin, curve[4*ch], curve[4*ch+2], end, t_s))
                        for ch in range(n_ch)
                    )
                samples.append((t_s, vals))
        elif curve == 'stepped' and v0 != v1:
            if end - begin > 1e-4:
                samples.append((end - 1e-4, v0))
    samples.append((keys[-1]['time'], values[-1]))
    return samples, False


def _eval_piecewise(samples, t, discrete=False):
    if t <= samples[0][0]:
        return samples[0][1]
    for (t0, v0), (t1, v1) in zip(samples, samples[1:]):
        if t < t1:
            if discrete or t1 == t0:
                return v0
            f = (t - t0) / (t1 - t0)
            return tuple(a + (b - a) * f for a, b in zip(v0, v1))
    return samples[-1][1]


def slot_color_track(slot, slot_channels):
    name = slot['name']
    setup_rgba = parse_hex_color(slot['color']) if slot.get('color') else (1.0, 1.0, 1.0, 1.0)
    rgba_keys = slot_channels.get((name, 'rgba'))
    rgb_keys = slot_channels.get((name, 'rgb'))
    alpha_keys = slot_channels.get((name, 'alpha'))

    if rgba_keys:
        samples, discrete = sample_channels(rgba_keys, lambda k: parse_hex_color(k['color']), setup_rgba)
        return [(t, color_str(*rgba)) for t, rgba in samples], discrete
    if rgb_keys and alpha_keys:
        rgb_samples, rgb_disc = sample_channels(rgb_keys, lambda k: parse_hex_color(k['color'])[:3], setup_rgba[:3])
        a_samples, a_disc = sample_channels(alpha_keys, lambda k: (k['value'],), (setup_rgba[3],))
        if rgb_disc and a_disc:
            times = sorted({t for t, _ in rgb_samples} | {t for t, _ in a_samples})
            merged = [(t, (*_eval_piecewise(rgb_samples, t, True), _eval_piecewise(a_samples, t, True)[0])) for t in times]
            return [(t, color_str(*rgba)) for t, rgba in compact(merged)], True
        if rgb_disc:
            rgb_samples, _ = sample_channels(rgb_keys, lambda k: parse_hex_color(k['color'])[:3], setup_rgba[:3], allow_discrete=False)
        if a_disc:
            a_samples, _ = sample_channels(alpha_keys, lambda k: (k['value'],), (setup_rgba[3],), allow_discrete=False)
        times = []
        for t in sorted([t for t, _ in rgb_samples] + [t for t, _ in a_samples]):
            if not times or abs(times[-1] - t) >= 1e-9:
                times.append(t)
        merged = [(t, (*_eval_piecewise(rgb_samples, t, False), _eval_piecewise(a_samples, t, False)[0])) for t in times]
        return [(t, color_str(*rgba)) for t, rgba in merged], False
    if alpha_keys:
        samples, discrete = sample_channels(alpha_keys, lambda k: (k['value'],), (setup_rgba[3],))
        return [(t, color_str(setup_rgba[0], setup_rgba[1], setup_rgba[2], a)) for t, (a,) in samples], discrete
    if rgb_keys:
        samples, discrete = sample_channels(rgb_keys, lambda k: parse_hex_color(k['color'])[:3], setup_rgba[:3])
        return [(t, color_str(r, g, b, setup_rgba[3])) for t, (r, g, b) in samples], discrete
    return [(0, color_str(*setup_rgba))], False


class GodotRig:
    def __init__(self, model):
        self.model = model
        self.world, self.paths = {}, {}
        self.bones = {b['name']: b for b in model['bones']}
        for n, b in self.bones.items():
            if any(k in b for k in ('scaleX','scaleY','shearX','shearY','inherit')):
                raise ValueError('This candidate expects rigid setup bone axes')
            loc = local_frame(b)
            p = b.get('parent')
            self.world[n] = compose(self.world[p],loc) if p else loc
            self.paths[n] = self.paths[p]+'/'+n if p else n
        self.slots = {s['name']: s for s in model['slots']}
        self.skin = model['skins'][0]['attachments']
        page, = model['pages']
        assert page['name']=='skeleton.png' and not page['pma'] and not model['constraints']
        self.regions = {r['name']: r for r in page['regions']}
        self.properties = {(b['name'],tl['name']) for a in model['animations'] for b in a['bones'] for tl in b['timelines']}
        assert all(prop in ('rotate','translate','scale') for _,prop in self.properties)
        self.body_slots = [name for name, slot in self.slots.items() if slot.get('setup') is not None]
        self.clip_group_alpha = {a['name']: self._uniform_body_alpha(a) for a in model['animations']}
        self.has_group_alpha = any(keys is not None for keys in self.clip_group_alpha.values())
        animated_color_slots = set()
        for a in model['animations']:
            grouped = self.clip_group_alpha[a['name']] is not None
            for s in a['slots']:
                for tl in s['timelines']:
                    if tl['name'] not in COLOR_TIMELINES:
                        continue
                    if grouped and tl['name'] == 'alpha' and s['name'] in self.body_slots:
                        continue
                    animated_color_slots.add(s['name'])
        self.color_slots = [
            name for name, slot in self.slots.items()
            if name in animated_color_slots
            or (slot.get('color') and parse_hex_color(slot['color']) != (1.0, 1.0, 1.0, 1.0))
        ]
        self.blend_ids = {}
        for row in self.slots.values():
            blend = (row.get('blend') or 'normal').lower()
            if blend == 'normal':
                continue
            if blend not in BLEND_MODES:
                raise ValueError(f'Unsupported slot blend mode: {blend}')
            self.blend_ids.setdefault(blend, f'blend_{blend}')

    def _uniform_body_alpha(self, clip):
        if not self.body_slots:
            return None
        slot_channels = {(s['name'], tl['name']): tl['keys'] for s in clip['slots'] for tl in s['timelines']}
        first_keys = slot_channels.get((self.body_slots[0], 'alpha'))
        if not first_keys:
            return None
        for name in self.body_slots:
            if (name, 'rgb') in slot_channels or (name, 'rgba') in slot_channels:
                return None
            if slot_channels.get((name, 'alpha')) != first_keys:
                return None
        return first_keys

    def mesh(self, slot, state, att):
        r = self.regions[att.get('path',state)]
        assert r['degrees']==0 and r['offsetX']==r['offsetY']==0 and r['width']==r['originalWidth'] and r['height']==r['originalHeight']
        slot_bone = self.slots[slot]['bone']
        if att['kind']=='mesh':
            bindings=att['vertices']['bindings']
            positions, weight_maps = [], []
            for vertex in bindings:
                positions_for_bones = [(point(self.world[v['bone']],v['x'],v['y']),v['weight']) for v in vertex]
                x=sum(p[0]*w for p,w in positions_for_bones); y=sum(p[1]*w for p,w in positions_for_bones)
                # A common bind point is exactly what native Polygon2D represents.
                if any(math.hypot(p[0]-x,p[1]-y)>.003 for p,_ in positions_for_bones):
                    raise ValueError(f'Distinct bind positions on {slot}, outside Godot linear-skinning tolerance')
                positions.extend([x,-y]); weight_maps.append({v['bone']:v['weight'] for v in vertex})
            uvs=list(zip(att['uvs'][::2],att['uvs'][1::2])); tris=att['triangles']
        else:
            w,h=att['width']*att.get('scaleX',1)/2, att['height']*att.get('scaleY',1)/2
            attachment=compose(self.world[slot_bone],local_frame(att));positions=[]
            for lx,ly in ((-w,h),(w,h),(w,-h),(-w,-h)):
                x,y=point(attachment,lx,ly);positions.extend([x,-y])
            uvs=[(0,0),(1,0),(1,1),(0,1)]; tris=[0,1,2,0,2,3];weight_maps=[{slot_bone:1}]*4
        atlas_uvs=[v for u,vv in uvs for v in (r['x']+u*r['width'], r['y']+vv*r['height'])]
        bone_names=sorted({b for v in weight_maps for b in v}, key=list(self.bones).index)
        weights=', '.join(f'"{self.paths[b]}", '+packed('Float32', [v.get(b,0) for v in weight_maps]) for b in bone_names)
        triangles=', '.join(packed('Int32',tris[i:i+3]) for i in range(0,len(tris),3))
        return ['texture = ExtResource("atlas")','skeleton = NodePath("../../Skeleton2D")',
                'polygon = '+packed('Vector2',positions),'uv = '+packed('Vector2',atlas_uvs),
                'polygons = Array[PackedInt32Array](['+triangles+'])', 'bones = ['+weights+']']

    def scene(self):
        load_steps = 4 + len(self.blend_ids) + (1 if self.has_group_alpha else 0)
        lines=[f'[gd_scene load_steps={load_steps} format=3]', '', '[ext_resource type="Texture2D" path="res://build/skeleton.png" id="atlas"]',
               '[ext_resource type="AnimationLibrary" path="res://godot/animations.tres" id="motions"]',
               '[ext_resource type="Script" path="res://godot/preview.gd" id="preview"]']
        if self.has_group_alpha:
            lines.append('[ext_resource type="Script" path="res://godot/visuals_alpha.gd" id="visuals"]')
        lines.append('')
        for blend, sub_id in self.blend_ids.items():
            lines += [f'[sub_resource type="CanvasItemMaterial" id="{sub_id}"]',
                      f'blend_mode = {BLEND_MODES[blend]}', '']
        lines += ['[node name="Realta" type="Node2D"]','script = ExtResource("preview")', '',
                  '[node name="Visuals" type="Node2D" parent="."]','position = Vector2(490, 728)', 'scale = Vector2(0.41, 0.41)',
                  'texture_filter = 2']
        if self.has_group_alpha:
            lines.append('script = ExtResource("visuals")')
        lines += ['','[node name="Skeleton2D" type="Skeleton2D" parent="Visuals"]','']
        for name,b in self.bones.items():
            parent='Visuals/Skeleton2D'+('/'+self.paths[b['parent']] if b.get('parent') else '')
            c,s,x,y=local_frame(b)
            lines += [f'[node name="{name}" type="Bone2D" parent="{parent}"]',
                      'position = '+vec(x,-y),'rotation = '+num(-math.radians(b.get('rotation',0))),
                      'rest = Transform2D('+', '.join(map(num,(c,-s,s,c,x,-y)))+')',
                      'auto_calculate_length_and_angle = false','length = '+num(b.get('length',4)),'bone_angle = 0.0','']
        for index,(s,row) in enumerate(self.slots.items()):
            slot_lines = [f'[node name="{s}" type="Node2D" parent="Visuals"]', f'z_index = {index}']
            if row.get('color'):
                slot_lines.append('modulate = ' + color_str(*parse_hex_color(row['color'])))
            slot_lines.append('')
            lines += slot_lines
            blend = (row.get('blend') or 'normal').lower()
            mat_line = [f'material = SubResource("{self.blend_ids[blend]}")'] if blend in self.blend_ids else []
            for state,att in self.skin.get(s, {}).items():
                lines += [f'[node name="{state}" type="Polygon2D" parent="Visuals/{s}"]',
                          'visible = '+str(row.get('setup')==state).lower(), *mat_line, *self.mesh(s,state,att),'']
        lines += ['[node name="AnimationPlayer" type="AnimationPlayer" parent="."]',
                  'libraries = { &"": ExtResource("motions") }','autoplay = "idle_loop"',
                  *[f'next/{name} = &"idle_loop"' for name in ('attack','hurt','cast','revive')],'',
                  '[node name="Controls" type="CanvasLayer" parent="."]','',
                  '[node name="Motion" type="OptionButton" parent="Controls"]','offset_left = 18.0','offset_top = 16.0',
                  'offset_right = 166.0','offset_bottom = 52.0','tooltip_text = "동작 선택 (1–7)"','']
        return '\n'.join(lines)

    def tracks(self, clip):
        tracks=[]
        channels={(b['name'],t['name']):t['keys'] for b in clip['bones'] for t in b['timelines']}
        slot_channels={(s['name'],t['name']):t['keys'] for s in clip['slots'] for t in s['timelines']}
        for name,prop in sorted(self.properties):
            b=self.bones[name]; prop_path={'rotate':'rotation','translate':'position','scale':'scale'}[prop]
            out=[]
            for k in channels.get((name,prop), [{'time':0}]):
                if 'curve' in k: raise ValueError(f'Unexpected curve in baked bone keys: {name}')
                if prop=='rotate': v=num(-math.radians(b.get('rotation',0)+k.get('value',0)))
                elif prop=='translate': v=vec(b.get('x',0)+k.get('x',0),-b.get('y',0)-k.get('y',0))
                else: v=vec(k.get('x',1),k.get('y',1))
                out.append((k['time'],v))
            tracks.append(('value','Visuals/Skeleton2D/'+self.paths[name]+':'+prop_path, out, False))
        setup=list(self.slots); orders=[(0,setup)]+[(godot_step_time(row['time']),order_at(setup,row)) for row in clip['drawOrder']]
        for name,slot in self.slots.items():
            tracks.append(('value',f'Visuals/{name}:z_index',compact([(t,str(order.index(name))) for t,order in orders]),True))
            states=slot_channels.get((name,'attachment'),[])
            for state in self.skin.get(name, {}):
                visible=[(0,slot.get('setup')==state)]+[(godot_step_time(k['time']),k.get('name')==state) for k in states]
                tracks.append(('value',f'Visuals/{name}/{state}:visible',compact([(t,str(v).lower()) for t,v in visible]),True))
        for name in self.color_slots:
            keys, discrete = slot_color_track(self.slots[name], slot_channels)
            tracks.append(('value', f'Visuals/{name}:modulate', keys, discrete))
        if self.has_group_alpha:
            group_keys = self.clip_group_alpha.get(clip['name'])
            if group_keys is not None:
                samples, discrete = sample_channels(group_keys, lambda k: (k['value'],), (1.0,))
                tracks.append(('value', 'Visuals:group_alpha', [(t, fnum(a)) for t, (a,) in samples], discrete))
            else:
                tracks.append(('value', 'Visuals:group_alpha', [(0, '1.0')], False))
        if clip['events']:
            tracks.append(('method','.',[(e['time'],'{"method": &"notify_cue", "args": [&"'+e['name']+'"]}') for e in clip['events']],True))
        return tracks

    def library(self):
        clips=[{'name':'RESET','duration':.01,'bones':[],'slots':[],'drawOrder':[],'events':[]}, *self.model['animations']]
        lines=['[gd_resource type="AnimationLibrary" load_steps='+str(len(clips)+1)+' format=3]','']
        for clip in clips:
            n=clip['name'];lines += [f'[sub_resource type="Animation" id="clip_{n}"]',f'resource_name = "{n}"','length = '+num(clip['duration']),
                                     'loop_mode = '+('1' if n.endswith('_loop') else '0')]
            for i,(kind,path,keys,discrete) in enumerate(self.tracks(clip)):
                prefix=f'tracks/{i}/'; contents=['"times": '+packed('Float32',[t for t,_ in keys]),
                        '"transitions": '+packed('Float32',[1]*len(keys)), '"values": ['+', '.join(v for _,v in keys)+']']
                # Visibility and draw order describe a held state at *any* time.
                # Event-style UPDATE_DISCRETE loses sub-frame handoffs on advance.
                if kind=='value': contents.append('"update": 0')
                lines += [prefix+'type = "'+kind+'"',prefix+'path = NodePath("'+path+'")',prefix+'interp = '+('0' if discrete else '1'),
                          prefix+'loop_wrap = false',prefix+'keys = {'+', '.join(contents)+'}']
            lines.append('')
        lines += ['[resource]','_data = {'+', '.join('&"'+c['name']+'": SubResource("clip_'+c['name']+'")' for c in clips)+'}','']
        return '\n'.join(lines)


VISUALS_ALPHA_GD = """extends Node2D

var _grouped := false

var group_alpha: float = 1.0:
\tset(value):
\t\tgroup_alpha = clampf(value, 0.0, 1.0)
\t\tself_modulate.a = group_alpha
\t\t# Transparent CanvasGroup mode isolates body fade without breaking additive sigil blend at alpha 1.
\t\tvar need_group := group_alpha > 0.0 and group_alpha < 1.0
\t\tif need_group != _grouped:
\t\t\t_grouped = need_group
\t\t\tRenderingServer.canvas_item_set_canvas_group_mode(
\t\t\t\tget_canvas_item(),
\t\t\t\tRenderingServer.CANVAS_GROUP_MODE_TRANSPARENT if need_group else RenderingServer.CANVAS_GROUP_MODE_DISABLED
\t\t\t)
\t\t\tif not need_group:
\t\t\t\tqueue_redraw()
"""


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('package',type=Path); parser.add_argument('--output',type=Path, help='Godot resources folder; defaults to <package>/godot')
    args=parser.parse_args(); out=args.output or args.package/'godot'; out.mkdir(parents=True,exist_ok=True)
    rig=GodotRig(json.loads((args.package/'build/skeleton.model.json').read_text(encoding='utf-8')))
    if rig.has_group_alpha:
        (out/'visuals_alpha.gd').write_text(VISUALS_ALPHA_GD,encoding='utf-8',newline='\n')
    (out/'realta.tscn').write_text(rig.scene(),encoding='utf-8',newline='\n')
    (out/'animations.tres').write_text(rig.library(),encoding='utf-8',newline='\n')
