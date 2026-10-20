extends SceneTree
## Bakes rig.json + meshes.json + parts.json into the runtime scenes
##   res://Sts2WD/animations/realta/combat/realta_combat_rig.tscn   (idle = idle_loop)
##   res://Sts2WD/animations/realta/combat/realta_merchant_rig.tscn (idle = relaxed_loop, merchant autoplay)
## Run from the repository root after importing the parts:
##   MegaDot --headless --path <repo> --script docs/characters/realta/animation/build_combat_rig.gd -- <this folder>

const OUT := "res://Sts2WD/animations/realta/combat/"
const PARTS := OUT + "parts/"
const ACTIONS := ["attack", "hurt", "cast", "powerup", "revive"]

var data: Dictionary
var manifest: Dictionary
var meshes: Array

func _initialize() -> void:
	var dir := OS.get_cmdline_user_args()[0]
	data = JSON.parse_string(FileAccess.get_file_as_string(dir.path_join("rig.json")))
	meshes = JSON.parse_string(FileAccess.get_file_as_string(dir.path_join("meshes.json")))
	manifest = JSON.parse_string(FileAccess.get_file_as_string(dir.path_join("parts.json")))
	var failed := false
	for variant in ["combat", "merchant"]:
		var root := build(variant)
		var path := OUT + "realta_%s_rig.tscn" % variant
		var err := ERR_SCRIPT_FAILED
		if root != null and root.get_script() != null:
			var packed := PackedScene.new()
			err = packed.pack(root)
			if err == OK:
				err = ResourceSaver.save(packed, path)
		print("baked ", path, " -> ", error_string(err))
		failed = failed or err != OK
		if root != null:
			root.free()
	quit(1 if failed else 0)

func tex(part: String) -> Texture2D:
	return load(PARTS + part + ".png")

func off(part: String) -> Vector2:
	return Vector2(manifest[part].offset[0], manifest[part].offset[1])

func own(node: Node, owner_node: Node) -> void:
	for child in node.get_children():
		child.owner = owner_node
		own(child, owner_node)

func build(variant: String) -> Node2D:
	var root := Node2D.new()
	root.name = "RealtaRig"
	var controller: GDScript = load(OUT + "realta_combat_rig.gd")
	if controller == null or not controller.can_instantiate():
		push_error("runtime controller failed to load")
		return null
	root.set_script(controller)
	var default_idle := "relaxed_loop" if variant == "merchant" else "idle_loop"

	# All death artwork is registered in rig space, independent of the animated hip.
	var lower: Dictionary = data.clips.die.lower

	# Bones (positions are parent-relative source pixels).
	var skeleton := Skeleton2D.new()
	skeleton.name = "Skeleton2D"
	var bones := {}
	for n in data.rest:
		var r: Array = data.rest[n]
		var b := Bone2D.new()
		b.name = n
		b.set_autocalculate_length_and_angle(false)
		b.set_length(20)
		var origin := Vector2(r[0], r[1])
		if r[2] != null:
			origin -= Vector2(data.rest[r[2]][0], data.rest[r[2]][1])
		b.rest = Transform2D(0.0, origin)
		b.transform = b.rest
		(skeleton if r[2] == null else bones[r[2]]).add_child(b)
		bones[n] = b

	# Release motion owns Weapon; the separate shaft-axis frame owns the axial half-turn.
	var weapon := Node2D.new()
	weapon.name = "Weapon"
	weapon.unique_name_in_owner = true
	bones.weapon.add_child(weapon)
	# A circular wooden shaft retains its diameter while blade/bristles become edge-on.
	var shaft_core := Sprite2D.new()
	shaft_core.name = "ShaftCore"
	shaft_core.texture = tex("shaft-core")
	shaft_core.centered = false
	shaft_core.offset = off("shaft-core")-Vector2(131,278)
	shaft_core.modulate.a = 0.0
	shaft_core.unique_name_in_owner = true
	weapon.add_child(shaft_core)
	var axis := Node2D.new()
	axis.name = "ShaftAxis"
	axis.position.x = float(data.weapon_axis.x) - 131.0
	axis.rotation = float(data.weapon_axis.angle)
	weapon.add_child(axis)
	var turn := Node2D.new()
	turn.name = "WeaponTurn"
	turn.unique_name_in_owner = true
	axis.add_child(turn)
	var ink := Node2D.new()
	ink.name = "Ink"
	ink.rotation = -axis.rotation
	ink.position = Vector2(-axis.position.x,0).rotated(-axis.rotation)
	turn.add_child(ink)
	var blade := Sprite2D.new()
	blade.name = "Blade"
	blade.texture = tex("weapon-rigid")
	blade.centered = false
	blade.offset = off("weapon-rigid") - Vector2(131, 278)
	ink.add_child(blade)
	var ribbon := Sprite2D.new()
	ribbon.name = "Ribbon"
	ribbon.texture = tex("ribbon")
	ribbon.centered = false
	ribbon.position = Vector2(-13, -84)
	ribbon.offset = off("ribbon") - Vector2(118, 194)
	ribbon.unique_name_in_owner = true
	# Cloth is independent of the shaft's axial reflection. Authoring registers the knot through
	# that matrix, then lets the tails hang/trail in world space instead of flipping like antlers.
	weapon.add_child(ribbon)

	# Skinned parts in draw order; the skeleton (carrying the weapon) takes the weapon's slot.
	var mesh_by_name := {}
	for m in meshes:
		mesh_by_name[m.name] = m
	var polys := []
	for n in data.order:
		if n == "weapon":
			root.add_child(skeleton)
			continue
		var m: Dictionary = mesh_by_name[n]
		var poly := Polygon2D.new()
		poly.name = n
		poly.texture = tex(n)
		var points := PackedVector2Array()
		var uv := PackedVector2Array()
		for v in m.vertices:
			points.append(Vector2(v[0], v[1]))
			uv.append(Vector2(v[0], v[1]) - off(n))
		poly.polygon = points
		poly.uv = uv
		var tris: Array[PackedInt32Array] = []
		for tri in m.triangles:
			tris.append(PackedInt32Array(tri))
		poly.polygons = tris
		root.add_child(poly)
		polys.append([poly, m])
	for pm in polys:
		var poly: Polygon2D = pm[0]
		poly.skeleton = poly.get_path_to(skeleton)
		for n in pm[1].bones:
			poly.add_bone(skeleton.get_path_to(bones[n]), PackedFloat32Array(pm[1].bones[n]))

	# Side-lying final cel (whole body). The prop slot is moved above all body cels below.
	var full: Dictionary = lower.full
	var full_cel := Sprite2D.new()
	full_cel.name = "DeathFull"
	full_cel.texture = tex(full.part)
	full_cel.centered = false
	var full_size: Vector2 = full_cel.texture.get_size()
	full_cel.offset = -full_size * 0.5
	full_cel.position = Vector2(full.topleft[0], full.topleft[1]) + off(full.part) + full_size * 0.5
	full_cel.visible = false
	full_cel.unique_name_in_owner = true
	root.add_child(full_cel)
	var base_positions := {"DeathFull": full_cel.position}
	# Full-body collapse drawings are ground registered in parts.json; all draw behind the final cel.
	for i in data.death_sequence.frames.size():
		var part: String = data.death_sequence.frames[i]
		var cel := Sprite2D.new()
		cel.name = "DeathSeq%02d" % i
		cel.texture = tex(part)
		cel.centered = false
		var size := cel.texture.get_size()
		cel.offset = -size*.5
		cel.position = off(part)+size*.5
		if part == full.part:
			cel.position += Vector2(full.topleft[0],full.topleft[1])
		cel.modulate.a = 0.0
		root.add_child(cel)
		base_positions[cel.name] = cel.position
	root.move_child(full_cel,root.get_child_count()-1)
	# Skeleton2D carries only the prop's visible art. Keep its original bone hierarchy
	# and animation paths, but draw the whole prop/ribbon above every body surface/cel.
	# The existing glove (including its turned texture) wraps over the shaft.
	root.move_child(skeleton, root.get_child_count()-1)
	root.move_child(root.get_node("hand_l"), root.get_child_count()-1)

	# VFX layers on top. Canvas units -> trimmed/downscaled texture: offset=(off-anchor)*k, scale=s/k.
	var base_scales := {}
	var sv: Dictionary = data.vfx.slash
	var sk: float = manifest[sv.part].scale
	var slash := Polygon2D.new()
	slash.name = "Slash"
	slash.texture = tex(sv.part)
	var vertices := PackedVector2Array()
	var uvs := PackedVector2Array()
	var triangles: Array[PackedInt32Array] = []
	var texture_size := slash.texture.get_size()
	for i in sv.sweep.size():
		var row: Dictionary = sv.sweep[i]
		for j in row.edge.size():
			vertices.append(Vector2(row.edge[j][0], row.edge[j][1]))
			uvs.append(Vector2(row.progress, float(j)/8.0) * texture_size)
			if i > 0 and j > 0:
				var k: int = i*9+j
				triangles.append(PackedInt32Array([k-10,k-9,k-1]))
				triangles.append(PackedInt32Array([k-9,k,k-1]))
	slash.polygon = vertices
	slash.uv = uvs
	slash.polygons = triangles
	base_scales["Slash"] = Vector2.ONE
	slash.modulate.a = 0.0
	var slash_material := ShaderMaterial.new()
	# Reveal is animated per character; each instantiated rig owns its material.
	slash_material.resource_local_to_scene = true
	slash_material.shader = load(OUT + "slash_reveal.gdshader")
	slash_material.set_shader_parameter("paint_offset", off(sv.part))
	slash_material.set_shader_parameter("paint_scale", sk)
	slash_material.set_shader_parameter("circle_center", Vector2(sv.circle[0],sv.circle[1]))
	slash_material.set_shader_parameter("angle_min", float(sv.angle_min))
	slash_material.set_shader_parameter("angle_max", float(sv.angle_max))
	slash_material.set_shader_parameter("radial_inner", PackedFloat32Array(sv.radial_inner))
	slash_material.set_shader_parameter("radial_outer", PackedFloat32Array(sv.radial_outer))
	slash_material.set_shader_parameter("sweep_length", float(sv.sweep_length))
	slash.material = slash_material
	root.add_child(slash)
	var cv: Dictionary = data.vfx.cast
	var ck: float = manifest[cv.part].scale
	var cast_fx := Node2D.new()
	cast_fx.name = "CastFx"
	cast_fx.position = Vector2(cv.position[0], cv.position[1])
	cast_fx.rotation = cv.rotation
	base_scales["CastFx"] = Vector2(cv.scale[0], cv.scale[1])
	cast_fx.scale = base_scales["CastFx"]
	cast_fx.modulate.a = 0.0
	root.add_child(cast_fx)
	var sigil := Sprite2D.new()
	sigil.name = "Sigil"
	sigil.texture = tex(cv.part)
	sigil.centered = false
	sigil.offset = (off(cv.part) - Vector2(cv.anchor[0], cv.anchor[1])) * ck
	sigil.scale = Vector2.ONE / ck
	cast_fx.add_child(sigil)

	var animator := AnimationPlayer.new()
	base_scales["%Ribbon"] = Vector2.ONE
	animator.name = "AnimationPlayer"
	root.add_child(animator)
	var library := AnimationLibrary.new()
	for clip in data.clips:
		var d: Dictionary = data.clips[clip]
		var animation := Animation.new()
		animation.length = d.length
		if d.loop:
			animation.loop_mode = Animation.LOOP_LINEAR
		for n in bones:
			for property in ["position", "rotation"]:
				var track := animation.add_track(Animation.TYPE_VALUE)
				animation.track_set_path(track, NodePath(str(root.get_path_to(bones[n])) + ":" + property))
				for s in d.samples:
					var value = s.joints[n][property]
					value = Vector2(value[0], value[1]) if property == "position" else float(value)
					animation.track_insert_key(track, float(s.time), value)
		_key(animation, ".:modulate:a", [[0.0, 1.0]], false)
		_overlays(root, animation, d.get("tracks", {}), base_scales, base_positions)
		library.add_animation(clip, animation)
	library.add_animation("idle", library.get_animation(default_idle))
	library.add_animation("relaxed", library.get_animation("relaxed_loop"))
	library.add_animation("powerup", library.get_animation("cast"))
	library.add_animation("revive", _revive(root, bones, default_idle, base_scales, base_positions))
	animator.add_animation_library("", library)
	# Blend from the live relaxed playback state, never an assumed shop phase. The .1s release
	# occurs during this blend; full-alpha native observations cover that overlap at live phases.
	# Finish before the 7/24s cel boundary. Aliases are independent keys.
	var relaxed_names := ["relaxed_loop", "relaxed"]
	if variant == "merchant": relaxed_names.append("idle")
	for source in relaxed_names:
		animator.set_blend_time(source, "die", .16)
		var combat_names := ["idle_loop", "idle"] if variant == "combat" else ["idle_loop"]
		for target in combat_names:
			animator.set_blend_time(source, target, .24)
			animator.set_blend_time(target, source, .3)
		# Finish the native blend before each incoming clip's clear-arm slot key.
		for target in ["attack", "hurt", "cast", "powerup"]:
			animator.set_blend_time(source, target, .03 if target == "attack" else .08 if target == "hurt" else .12)
			animator.set_blend_time(target, source, .3)
	for clip in ACTIONS:
		animator.animation_set_next(clip, default_idle)
	animator.autoplay = default_idle

	own(root, root)
	return root

func _key(animation: Animation, path: String, keys: Array, discrete: bool) -> void:
	var track := animation.add_track(Animation.TYPE_VALUE)
	animation.track_set_path(track, NodePath(path))
	if discrete:
		animation.value_track_set_update_mode(track, Animation.UPDATE_DISCRETE)
	for k in keys:
		animation.track_insert_key(track, float(k[0]), k[1])

func _overlays(root: Node, animation: Animation, tracks: Dictionary, base_scales: Dictionary, base_positions: Dictionary) -> void:
	# Expressions and VFX share the joints' player; every clip keys the defaults so nothing goes stale.
	for path in data.track_defaults:
		var keys: Array = tracks.get(path, [[0.0, data.track_defaults[path]]])
		var target: String = path.get_slice(":", 0)
		var property: String = path.substr(path.find(":") + 1).replace("scale_mul", "scale").replace("settle", "position")
		var values := []
		for k in keys:
			var v = k[1]
			if path.ends_with(":texture"):
				v = tex(data.textures[v])
			elif path.ends_with(":scale_mul"):
				v = base_scales[target] * float(v)
			elif path.ends_with(":settle"):
				v = base_positions[target] + Vector2(v[0], v[1])
			elif path.ends_with(":position") or path.ends_with(":scale"):
				v = Vector2(v[0],v[1])
			elif path == ".:lower_mode":
				v = int(v)
			else:
				v = float(v)
			values.append([k[0], v])
		_key(animation, target + ":" + property, values, path.ends_with(":texture") or path in [".:lower_mode", ".:weapon_arm_front", ".:weapon_gripped", ".:free_arm_depth_hold", ".:free_arm_back"])

func _revive(root: Node, bones: Dictionary, default_idle: String, base_scales: Dictionary, base_positions: Dictionary) -> Animation:
	# Official default revive: 0.2 s fade-out, reset to idle while hidden, 0.2 s fade-in (not reversed death).
	var revive := Animation.new()
	revive.length = .4
	for n in bones:
		for property in ["position", "rotation"]:
			var keys := []
			for k in [[0.0, "die", -1], [.2, default_idle, 0]]:
				var value = data.clips[k[1]].samples[k[2]].joints[n][property]
				keys.append([k[0], Vector2(value[0], value[1]) if property == "position" else float(value)])
			_key(revive, str(root.get_path_to(bones[n])) + ":" + property, keys, true)
	_key(revive, ".:modulate:a", [[0.0, 1.0], [.2, 0.0], [.4, 1.0]], false)
	var reset_tracks := {"head:texture": [[0.0, "closed"], [.2, "open"]], ".:lower_mode": [[0.0, 3], [.2, 0]], "DeathFull:modulate:a": [[0.0,1.0],[.1999,1.0],[.2,0.0]]}
	reset_tracks[".:weapon_gripped"] = [[0.0, false], [.2, true]]
	reset_tracks[".:free_arm_back"] = [[0.0, false], [.2, default_idle == "relaxed_loop"]]
	var prop_reset_paths := ["%Weapon:position", "%Weapon:rotation", "%Ribbon:rotation", "%Ribbon:position", "%Ribbon:scale"]
	for path in prop_reset_paths:
		var last = data.clips.die.tracks[path][-1][1]
		var first = data.clips[default_idle].tracks[path][0][1] if data.clips[default_idle].tracks.has(path) else data.track_defaults[path]
		reset_tracks[path] = [[0.0,last],[.2,first]]
	_overlays(root, revive, reset_tracks, base_scales, base_positions)
	# Reset the prop with the bones at the hidden key. Interpolating even a short
	# interval beforehand mixes idle prop offsets with the still-prone skeleton.
	for track in revive.get_track_count():
		if str(revive.track_get_path(track)) in prop_reset_paths:
			revive.value_track_set_update_mode(track, Animation.UPDATE_DISCRETE)
	return revive

func _world(clip: Dictionary, n: String, t: float) -> Transform2D:
	var f := clampf(t / float(clip.length) * (clip.samples.size() - 1), 0, clip.samples.size() - 1)
	var lo := int(f)
	var hi := mini(lo + 1, clip.samples.size() - 1)
	var a: Dictionary = clip.samples[lo].joints[n]
	var b: Dictionary = clip.samples[hi].joints[n]
	var local := Transform2D(lerpf(a.rotation, b.rotation, f - lo), Vector2(lerpf(a.position[0], b.position[0], f - lo), lerpf(a.position[1], b.position[1], f - lo)))
	var parent = data.rest[n][2]
	return local if parent == null else _world(clip, parent, t) * local

