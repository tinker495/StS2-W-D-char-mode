"""Adapt a compiled Realta rig-c package to the game's two AnimationPlayer entry points."""
import argparse
import importlib.util
import json
from pathlib import Path
import re
import shutil
import tempfile


ROOT = Path(__file__).resolve().parents[2]
GAME = "res://Sts2WD/animations/realta/combat"
LOOPS = ("idle_loop", "relaxed_loop")
ACTIONS = ("attack", "hurt", "cast", "powerup", "revive")


def write(path, text):
    path.write_text(text.rstrip() + "\n", encoding="utf-8", newline="\n")


def load_baker(package):
    author = Path("authoring/bake_godot.py")
    source = package / author
    if not source.is_file():
        source = ROOT / "docs/characters/realta/animation/rigc" / author
    spec = importlib.util.spec_from_file_location("realta_baker", source)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def art_scene(baker, rig, orders):
    scene = rig.scene()
    materials = scene[scene.index('[sub_resource '):scene.index('[node name="Realta"')]
    bones_and_slots = scene[scene.index('[node name="Skeleton2D"'):scene.index('[node name="AnimationPlayer"')]
    bones_and_slots = re.sub(r'parent="Visuals(?:/([^"]+))?"', lambda m: f'parent="{m[1] or "."}"', bones_and_slots)
    bones_and_slots = re.sub(r'^z_index = \d+\n', '', bones_and_slots, flags=re.M)
    slot_ids = {name: i for i, name in enumerate(rig.slots)}
    slot_names = ", ".join(json.dumps(name) for name in slot_ids)
    serialized = ", ".join(
        baker.packed("Int32", [slot_ids[name] for name in order]) for order in orders
    )
    return "\n".join([
        f'[gd_scene load_steps={3 + len(rig.blend_ids)} format=3]', '',
        f'[ext_resource type="Texture2D" path="{GAME}/realta_skeleton.png" id="atlas"]',
        f'[ext_resource type="Script" path="{GAME}/realta_draw_order.gd" id="order"]', '',
        materials.rstrip(), '',
        '[node name="Visuals" type="Node2D"]', 'texture_filter = 2', 'script = ExtResource("order")',
        f'slot_names = PackedStringArray({slot_names})',
        f'orders = Array[PackedInt32Array]([{serialized}])', '',
        bones_and_slots.rstrip(),
    ])


def clip_tracks(baker, rig, clip, orders):
    setup = list(rig.slots)
    depth_keys = []
    for t, order in [(0, setup)] + [
        (baker.godot_step_time(row['time']), baker.order_at(setup, row)) for row in clip['drawOrder']
    ]:
        if order not in orders:
            orders.append(order)
        depth_keys.append((t, str(orders.index(order))))
    result = [("value", "Visuals:draw_order", baker.compact(depth_keys), True)]
    for kind, path, keys, stepped in rig.tracks(clip):
        if not path.endswith(":z_index"):
            result.append((kind, path, keys, stepped))
    return result


def merchant_revival(baker, combat_revive, relaxed):
    """Fade out the shared fallen skeleton, switch to the shop pose while hidden, and fade in."""
    default = {path: keys[0][1] for _, path, keys, _ in relaxed}
    swap = baker.godot_step_time(0.2)
    result = []
    for kind, path, keys, stepped in combat_revive:
        if path == "Visuals:group_alpha" or path.endswith(":modulate"):
            result.append((kind, path, keys, stepped))
        else:
            result.append((kind, path, baker.compact([(0, keys[0][1]), (swap, default[path])]), True))
    return result


def animation(baker, name, duration, tracks):
    rows = ['[gd_resource type="Animation" format=3]', '', '[resource]',
            f'resource_name = "{name}"', 'length = ' + baker.num(duration),
            'loop_mode = ' + ('1' if name in LOOPS else '0')]
    for i, (kind, path, keys, stepped) in enumerate(tracks):
        prefix = f'tracks/{i}/'
        values = ', '.join(v for _, v in keys)
        times = baker.packed('Float32', [t for t, _ in keys])
        transitions = baker.packed('Float32', [1] * len(keys))
        update = ', "update": 0' if kind == 'value' else ''
        rows.extend([prefix + f'type = "{kind}"', prefix + f'path = NodePath("{path}")',
                     prefix + ('interp = 0' if stepped else 'interp = 1'),
                     prefix + 'loop_wrap = false',
                     prefix + f'keys = {{"times": {times}, "transitions": {transitions}, "values": [{values}]{update}}}'])
    return '\n'.join(rows)


def rig_scene(clips, merchant):
    idle = "relaxed_loop" if merchant else "idle_loop"
    # Alias references point to the same Animation object. BaseLib restarts using these names.
    resources = {c: ("revive_merchant" if merchant and c == "revive" else c) for c in clips}
    aliases = dict(resources, idle=idle, relaxed="relaxed_loop", powerup="cast")
    rows = [f'[gd_scene load_steps={len(clips) + 4} format=3]', '',
            f'[ext_resource type="PackedScene" path="{GAME}/realta_body.tscn" id="body"]',
            f'[ext_resource type="Script" path="{GAME}/realta_combat_rig.gd" id="controller"]']
    for name in resources.values():
        rows.append(f'[ext_resource type="Animation" path="{GAME}/clips/{name}.tres" id="{name}"]')
    mapping = ', '.join(f'&"{name}": ExtResource("{target}")' for name, target in aliases.items())
    rows.extend(['', '[sub_resource type="AnimationLibrary" id="clips"]', f'_data = {{{mapping}}}', '',
                 '[node name="RealtaRig" type="Node2D"]', 'script = ExtResource("controller")', '',
                 '[node name="Visuals" parent="." instance=ExtResource("body")]',
                 # Match the original 384 × 576 registration with one source pixel
                 # of clearance for the faint blade and bristle edges at ground contact.
                 'position = Vector2(192, 575)', 'scale = Vector2(0.3333333333, 0.3333333333)', '',
                 '[node name="AnimationPlayer" type="AnimationPlayer" parent="."]',
                 'libraries = {&"": SubResource("clips")}', f'autoplay = "{idle}"'])
    rows.extend(f'next/{action} = &"{idle}"' for action in ACTIONS)
    relaxed = ["relaxed", "relaxed_loop"] + (["idle"] if merchant else [])
    standing = ["idle_loop"] + ([] if merchant else ["idle"])
    times = {(a, b): .3 for a in standing + ["attack", "cast", "hurt", "powerup"] for b in relaxed}
    times.update({(a, b): t for a in relaxed for b, t in
                  [("attack", .03), ("hurt", .08), ("cast", .12), ("powerup", .12), ("die", .16)]})
    times.update({(a, b): .24 for a in relaxed for b in standing})
    transitions = ', '.join(f'&"{a}", &"{b}", {t:g}' for (a, b), t in sorted(times.items()))
    rows.append(f'blend_times = [{transitions}]')
    return '\n'.join(rows)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("package", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    package, out = args.package.resolve(), args.output.resolve()
    build = package / "build"
    baker = load_baker(package)
    model = json.loads((build / "skeleton.model.json").read_bytes())
    rig = baker.GodotRig(model)
    clips = {r["name"]: r for r in model["animations"]}
    orders = [list(rig.slots)]
    tracks = {name: clip_tracks(baker, rig, clip, orders) for name, clip in clips.items()}
    # Keep the input folder read-only and commit only fully rendered resources.
    out.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix=".realta-", dir=out) as work:
        staging = Path(work)
        (staging / "clips").mkdir()
        for name, clip in clips.items():
            write(staging / "clips" / f"{name}.tres", animation(baker, name, clip["duration"], tracks[name]))
        shop = merchant_revival(baker, tracks["revive"], tracks["relaxed_loop"])
        write(staging / "clips/revive_merchant.tres", animation(baker, "revive", clips["revive"]["duration"], shop))
        write(staging / "realta_body.tscn", art_scene(baker, rig, orders))
        for variant in ("combat", "merchant"):
            write(staging / f"realta_{variant}_rig.tscn", rig_scene(clips, variant == "merchant"))
        shutil.copyfile(build / "skeleton.png", staging / "realta_skeleton.png")
        (out / "clips").mkdir(exist_ok=True)
        for p in staging.rglob("*"):
            if p.is_file() and p.suffix in (".tscn", ".tres", ".png"):
                p.replace(out / p.relative_to(staging))
        if out == (ROOT / "Sts2WD/animations/realta/combat").resolve():
            shutil.copyfile(build / "skeleton.atlas", ROOT / "docs/characters/realta/animation/rigc/atlas.txt")
    print(f"Realta: {len(clips)} motions, {len(rig.bones)} bones, {len(rig.slots)} slots, {len(orders)} draw orders.")


if __name__ == "__main__":
    main()
