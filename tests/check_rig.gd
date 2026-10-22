extends SceneTree
## Human requester: wjdrb, 2026-10-20, retrospective items 2 and "수정하기."
## Explicitly requested: zero Z/no top_level; foreground weapon with only the
## gripping hand/effects above it; ten aliases and next transitions; defaults in
## every clip; weapon/broom ground clearance; DeathAnimTime matching settlement.
## Expectations come from that request and the tracked animation contract:
## source floor Y527, held/released grip ownership, and death-sequence.json.
## Clearance checks nonzero-alpha pixel EDGES, not an opaque-pixel threshold.
## Sample every authored key and interpolation quarter-point, as specified by
## docs/agents/codex-cutout-rig.md. This is sampled geometry, not visual approval.

## Human requester: wjdrb, 2026-10-22, explicitly instructed "RESET 추가와 해당 검사 수정 지시"
## during the native Godot workflow improvement. Permit the editor-only RESET in
## addition to the original ten game dispatch names so reset_on_save can restore
## the variant's authored defaults. Keep all existing dispatch, alias, key-default,
## transition, layer, clearance and instance-isolation requirements unchanged.
const NAMES := ["RESET", "attack", "cast", "die", "hurt", "idle", "idle_loop", "powerup", "relaxed", "relaxed_loop", "revive"]
const CLIPS := ["idle_loop", "attack", "hurt", "cast", "die", "relaxed_loop", "revive"]
const ACTIONS := ["attack", "hurt", "cast", "powerup", "revive"]
const FLOOR_Y := 527.0
var failures: Dictionary = {}
var sample_count := 0

func _initialize() -> void:
	run.call_deferred()

func require(condition: bool, rule: String, detail: String = "") -> void:
	if not condition and not failures.has(rule):
		failures[rule] = detail

func run() -> void:
	var sequence: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(
		"res://docs/characters/realta/animation/death-sequence.json"))
	var settled: float = sequence["settled_time"]
	var expression := RegEx.new()
	expression.compile("DeathAnimTime\\s*=>\\s*([0-9.]+)f\\s*;")
	var declaration := expression.search(FileAccess.get_file_as_string(
		"res://Sts2WDCode/Characters/Realta/Realta.cs"))
	require(declaration != null, "DeathAnimTime declaration", "Expected an explicit float duration")
	if declaration != null:
		require(float(declaration.get_string(1)) == settled, "DeathAnimTime equals settled_time")
	for variant in ["combat", "merchant"]:
		var scene := load("res://Sts2WD/animations/realta/combat/realta_%s_rig.tscn" % variant) as PackedScene
		if scene == null:
			require(false, variant + ": scene loads")
			continue
		var rig := scene.instantiate() as Node2D
		root.add_child(rig)
		var player: AnimationPlayer = rig.get_node("AnimationPlayer")
		player.callback_mode_process = AnimationMixer.ANIMATION_CALLBACK_MODE_PROCESS_MANUAL
		player.stop()
		await process_frame
		check_library(player, variant)
		var hulls := weapon_hulls(rig, variant)
		for clip in CLIPS:
			if not player.has_animation(clip):
				continue
			player.stop()
			player.play(clip, 0)
			player.advance(0)
			var times := sample_times(player.get_animation(clip))
			var maximum_y := -INF
			var deepest := ""
			for time in times:
				player.seek(time, true)
				sample_count += 1
				var label: String = variant + "/" + clip
				check_layers(rig, label, time)
				for sprite: Sprite2D in hulls:
					if not sprite.is_visible_in_tree() or inherited_alpha(sprite, rig) <= 0.0:
						continue
					var transform := rig.global_transform.affine_inverse() * sprite.global_transform
					for point: Vector2 in hulls[sprite]:
						var y := (transform * point).y
						if y > maximum_y:
							maximum_y = y
							deepest = "%s at %.6fs" % [sprite.name, time]
			require(maximum_y != -INF, variant + "/" + clip + ": visible weapon sampled")
			require(maximum_y <= FLOOR_Y, variant + "/" + clip + ": ground clearance",
				"max Y=%s, floor=%s, %s" % [maximum_y, FLOOR_Y, deepest])
			print("%s/%s: %d samples, minimum clearance=%.6f" % [variant, clip, times.size(), FLOOR_Y - maximum_y])
		check_completion(rig, player, variant, settled)
		rig.free()
	for rule in failures:
		printerr("FAIL: ", rule, " ", failures[rule])
	if failures.is_empty():
		print("PASS: Realta rig checks (%d native samples)" % sample_count)
		quit(0)
	else:
		printerr("FAIL: Realta rig checks (%d rules)" % failures.size())
		quit(1)

func check_library(player: AnimationPlayer, variant: String) -> void:
	var names := Array(player.get_animation_list())
	names.sort()
	require(names == NAMES, variant + ": ten dispatch names plus editor RESET", str(names))
	var idle := "relaxed_loop" if variant == "merchant" else "idle_loop"
	require(player.autoplay == idle, variant + ": autoplay")
	for alias in {"idle": idle, "relaxed": "relaxed_loop", "powerup": "cast"}:
		var target: String = {"idle": idle, "relaxed": "relaxed_loop", "powerup": "cast"}[alias]
		require(player.has_animation(alias) and player.has_animation(target), variant + ": alias " + alias)
		if player.has_animation(alias) and player.has_animation(target):
			require(player.get_animation(alias) == player.get_animation(target), variant + ": alias resource " + alias)
	var union: Dictionary = {}
	var paths_by_clip: Dictionary = {}
	for name in names:
		var animation := player.get_animation(name)
		var paths: Dictionary = {}
		for track in animation.get_track_count():
			var path := str(animation.track_get_path(track))
			require(not paths.has(path), variant + "/" + name + ": unique track " + path)
			paths[path] = true
			union[path] = true
			require(animation.track_get_key_count(track) > 0, variant + "/" + name + ": keys " + path)
			if animation.track_get_key_count(track) > 0:
				require(animation.track_get_key_time(track, 0) == 0.0, variant + "/" + name + ": time-zero default " + path)
		paths_by_clip[name] = paths
		var expected_next: String = idle if name in ACTIONS else ""
		require(str(player.animation_get_next(name)) == expected_next, variant + "/" + name + ": next")
		var loop: bool = name in ["idle", "idle_loop", "relaxed", "relaxed_loop"]
		require((animation.loop_mode != Animation.LOOP_NONE) == loop, variant + "/" + name + ": loop mode")
	for name in names:
		for path in union:
			require(paths_by_clip[name].has(path), variant + "/" + name + ": default track present " + path)

func sample_times(animation: Animation) -> Array:
	var times: Dictionary = {0.0: true, animation.length: true}
	for track in animation.get_track_count():
		for key in animation.track_get_key_count(track):
			var time := animation.track_get_key_time(track, key)
			times[time] = true
			if key > 0:
				var previous := animation.track_get_key_time(track, key - 1)
				for quarter in [1, 2, 3]:
					times[lerpf(previous, time, quarter / 4.0)] = true
	var sorted := times.keys()
	sorted.sort()
	return sorted

func check_layers(rig: Node2D, label: String, time: float) -> void:
	for node in rig.find_children("*", "CanvasItem", true, false) + [rig]:
		require(node.z_index == 0 and not node.is_set_as_top_level(),
			label + ": zero Z/no top_level " + str(rig.get_path_to(node)), "at %s" % time)
	var skeleton := rig.get_node("Skeleton2D")
	var glove := rig.get_node("hand_l")
	for child in rig.get_children():
		if child is Polygon2D and child.name != "Slash" and child != glove or child.name == "DeathFull" or str(child.name).begins_with("DeathSeq"):
			require(child.get_index() < skeleton.get_index(), label + ": body behind weapon " + str(child.name), "at %s" % time)
	var held: bool = rig.get("weapon_gripped")
	require((glove.get_index() > skeleton.get_index()) == held, label + ": held/released glove order", "at %s" % time)
	for effect in ["Slash", "CastFx"]:
		var index := rig.get_node(effect).get_index()
		require(index > skeleton.get_index() and index > glove.get_index(), label + ": effect foreground " + effect)

func weapon_hulls(rig: Node2D, variant: String) -> Dictionary:
	var hulls: Dictionary = {}
	var weapon := rig.get_node("%Weapon")
	for sprite: Sprite2D in weapon.find_children("*", "Sprite2D", true, false):
		var image := sprite.texture.get_image()
		if image.is_compressed():
			var error := image.decompress()
			require(error == OK, variant + ": readable texture " + str(sprite.name))
		var edges := PackedVector2Array()
		var origin := sprite.get_rect().position
		for y in image.get_height():
			var left := image.get_width()
			var right := -1
			for x in image.get_width():
				if image.get_pixel(x, y).a > 0.0:
					left = mini(left, x)
					right = maxi(right, x)
			if right >= left:
				# Row extrema cover all pixel edges under any affine transform.
				edges.append(origin + Vector2(left, y))
				edges.append(origin + Vector2(right + 1, y))
				edges.append(origin + Vector2(left, y + 1))
				edges.append(origin + Vector2(right + 1, y + 1))
		require(not edges.is_empty(), variant + ": nonempty weapon part " + str(sprite.name))
		hulls[sprite] = Geometry2D.convex_hull(edges)
	require(not hulls.is_empty(), variant + ": weapon sprites")
	return hulls

func inherited_alpha(sprite: Sprite2D, rig: Node2D) -> float:
	var alpha := sprite.self_modulate.a
	var node: Node = sprite
	while node != rig.get_parent():
		if node is CanvasItem:
			alpha *= node.modulate.a
		node = node.get_parent()
	return alpha

func check_completion(rig: Node2D, player: AnimationPlayer, variant: String, settled: float) -> void:
	var idle := "relaxed_loop" if variant == "merchant" else "idle_loop"
	for action in ACTIONS:
		player.stop()
		player.play(action, 0)
		player.advance(player.get_animation(action).length + 0.02)
		player.advance(0.02)
		require(str(player.current_animation) == idle, variant + "/" + action + ": automatic return")
	player.stop()
	player.play("die", 0)
	player.seek(settled, true)
	var body: Node2D = rig.get_node("DeathFull")
	var weapon: Node2D = rig.get_node("%Weapon")
	var ribbon: Node2D = rig.get_node("%Ribbon")
	var settled_transforms := [body.global_transform, weapon.global_transform, ribbon.global_transform]
	player.seek(player.get_animation("die").length, true)
	require(settled_transforms == [body.global_transform, weapon.global_transform, ribbon.global_transform],
		variant + ": visible death pose settled by DeathAnimTime")
	player.advance(0.02)
	require(not player.is_playing() and player.get_queue().is_empty(), variant + ": death holds without next")
