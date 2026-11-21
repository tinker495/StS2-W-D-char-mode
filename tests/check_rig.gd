extends SceneTree
## Explicit human authorization: wjdrb, 2026-11-21 (per docs/agents/test-ownership.md)
## to modernize the rig contract test for the unified 7-clip rigc combat/merchant rig
## and remove legacy cutout / death-sequence.json / FLOOR_Y assumptions.
##
## Verifies structural invariants across both combat and merchant rig variants:
## - Scene loading, ten dispatch names, autoplay, aliases, next transitions, loop modes
## - Value-track uniqueness, time-0 defaults, and cross-clip track union
## - Method-track notify_cue contracts on "." matching Realta.cs timings
## - Zero z_index / non-top_level on all CanvasItem descendants
## - Visuals/Skeleton2D at child index 0 and Visuals slot order matching orders[draw_order]
## - Automatic return for one-shot actions and settled core bones + hold for die

const NAMES := ["attack", "cast", "die", "hurt", "idle", "idle_loop", "powerup", "relaxed", "relaxed_loop", "revive"]
const CLIPS := ["idle_loop", "attack", "hurt", "cast", "die", "relaxed_loop", "revive"]
const ACTIONS := ["attack", "hurt", "cast", "powerup", "revive"]
const EXPECTED_ATTACK_DELAY := 0.54
const EXPECTED_DEATH_TIME := 1.66
const EXPECTED_DIE_LENGTH := 2.20
const EXPECTED_CUES := {
	"attack": {"cue": &"attack_hit", "time": EXPECTED_ATTACK_DELAY},
	"cast": {"cue": &"cast_release", "time": 0.25},
	"powerup": {"cue": &"cast_release", "time": 0.25},
	"die": {"cue": &"weapon_release", "time": 0.12},
}

var failures: Dictionary = {}
var sample_count := 0

func _initialize() -> void:
	run.call_deferred()

func require(condition: bool, rule: String, detail: String = "") -> void:
	if not condition and not failures.has(rule):
		failures[rule] = detail

func parse_cs_float(source: String, property_name: String) -> Variant:
	var expression := RegEx.new()
	expression.compile(property_name + "\\s*=>\\s*([0-9.]+)f\\s*;")
	var match := expression.search(source)
	if match == null:
		return null
	return float(match.get_string(1))

func run() -> void:
	var realta_cs := FileAccess.get_file_as_string("res://Sts2WDCode/Characters/Realta/Realta.cs")
	var attack_delay = parse_cs_float(realta_cs, "AttackAnimDelay")
	require(attack_delay != null, "AttackAnimDelay declaration", "Expected an explicit float duration in Realta.cs")
	if attack_delay != null:
		require(is_equal_approx(attack_delay, EXPECTED_ATTACK_DELAY), "AttackAnimDelay equals 0.54",
			"got %s" % attack_delay)
	var death_time = parse_cs_float(realta_cs, "DeathAnimTime")
	require(death_time != null, "DeathAnimTime declaration", "Expected an explicit float duration in Realta.cs")
	if death_time != null:
		require(is_equal_approx(death_time, EXPECTED_DEATH_TIME), "DeathAnimTime equals 1.66",
			"got %s" % death_time)

	var effective_attack_delay: float = attack_delay if attack_delay != null else EXPECTED_ATTACK_DELAY
	var effective_death_time: float = death_time if death_time != null else EXPECTED_DEATH_TIME

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
		check_library(player, variant, effective_attack_delay)
		for clip in CLIPS:
			if not player.has_animation(clip):
				continue
			player.stop()
			player.play(clip, 0)
			player.advance(0)
			var times := sample_times(player.get_animation(clip))
			for time in times:
				player.seek(time, true)
				sample_count += 1
				check_layers(rig, variant + "/" + clip, time)
			print("%s/%s: %d samples" % [variant, clip, times.size()])
		check_completion(rig, player, variant, effective_death_time)
		rig.free()

	for rule in failures:
		printerr("FAIL: ", rule, " ", failures[rule])
	if failures.is_empty():
		print("PASS: Realta rig checks (%d native samples)" % sample_count)
		quit(0)
	else:
		printerr("FAIL: Realta rig checks (%d rules)" % failures.size())
		quit(1)

func check_library(player: AnimationPlayer, variant: String, attack_delay: float) -> void:
	var names := Array(player.get_animation_list())
	names.sort()
	require(names == NAMES, variant + ": ten dispatch names", str(names))
	var idle := "relaxed_loop" if variant == "merchant" else "idle_loop"
	require(player.autoplay == idle, variant + ": autoplay")
	var aliases := {"idle": idle, "relaxed": "relaxed_loop", "powerup": "cast"}
	for alias in aliases:
		var target: String = aliases[alias]
		require(player.has_animation(alias) and player.has_animation(target), variant + ": alias " + alias)
		if player.has_animation(alias) and player.has_animation(target):
			require(player.get_animation(alias) == player.get_animation(target), variant + ": alias resource " + alias)
	var union: Dictionary = {}
	var paths_by_clip: Dictionary = {}
	for name in names:
		var animation := player.get_animation(name)
		var paths: Dictionary = {}
		var found_cues: Array[Dictionary] = []
		for track in animation.get_track_count():
			var track_type := animation.track_get_type(track)
			var path := str(animation.track_get_path(track))
			if track_type == Animation.TYPE_VALUE:
				require(not paths.has(path), variant + "/" + name + ": unique track " + path)
				paths[path] = true
				union[path] = true
				require(animation.track_get_key_count(track) > 0, variant + "/" + name + ": keys " + path)
				if animation.track_get_key_count(track) > 0:
					require(animation.track_get_key_time(track, 0) == 0.0, variant + "/" + name + ": time-zero default " + path)
			elif track_type == Animation.TYPE_METHOD and path == ".":
				for key in animation.track_get_key_count(track):
					if animation.method_track_get_name(track, key) == &"notify_cue":
						found_cues.append({
							"time": animation.track_get_key_time(track, key),
							"params": animation.method_track_get_params(track, key),
						})
		paths_by_clip[name] = paths
		if EXPECTED_CUES.has(name):
			var expected: Dictionary = EXPECTED_CUES[name]
			var expected_cue: StringName = expected["cue"]
			var expected_time: float = attack_delay if name == "attack" else expected["time"]
			var matched := false
			for entry in found_cues:
				var params: Array = entry["params"]
				if params.size() == 1 and params[0] == expected_cue and is_equal_approx(entry["time"], expected_time):
					matched = true
					break
			require(matched, variant + "/" + name + ": notify_cue " + str(expected_cue),
				"expected notify_cue(&\"%s\") on \".\" at %.2fs, found %s" % [expected_cue, expected_time, str(found_cues)])
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
	var visuals := rig.get_node_or_null("Visuals") as Node2D
	require(visuals != null, label + ": Visuals node present", "at %s" % time)
	if visuals == null:
		return
	var skeleton := visuals.get_node_or_null("Skeleton2D")
	require(skeleton != null and skeleton.get_index() == 0,
		label + ": Visuals/Skeleton2D at index 0", "at %s" % time)
	var draw_order: int = visuals.get("draw_order")
	var orders: Array = visuals.get("orders")
	var slot_names: PackedStringArray = visuals.get("slot_names")
	var valid_order := draw_order >= 0 and draw_order < orders.size()
	require(valid_order, label + ": Visuals.draw_order in range",
		"draw_order=%s, orders=%s at %s" % [draw_order, orders.size(), time])
	if not valid_order:
		return
	var expected_order: PackedInt32Array = orders[draw_order]
	var children := visuals.get_children()
	require(children.size() == expected_order.size() + 1,
		label + ": Visuals slot child count",
		"children=%s, expected=%s at %s" % [children.size(), expected_order.size() + 1, time])
	if children.size() != expected_order.size() + 1:
		return
	for i in expected_order.size():
		var expected_name := slot_names[expected_order[i]]
		var actual_name := str(children[i + 1].name)
		if actual_name != expected_name:
			require(false, label + ": Visuals slot order matches draw_order",
				"slot %d expected %s got %s (draw_order=%d) at %s" % [i, expected_name, actual_name, draw_order, time])
			break

func check_completion(rig: Node2D, player: AnimationPlayer, variant: String, death_time: float) -> void:
	var idle := "relaxed_loop" if variant == "merchant" else "idle_loop"
	for action in ACTIONS:
		player.stop()
		player.play(action, 0)
		player.advance(player.get_animation(action).length + 0.02)
		player.advance(0.02)
		require(str(player.current_animation) == idle, variant + "/" + action + ": automatic return")
	player.stop()
	player.play("die", 0)
	var die_anim := player.get_animation("die")
	require(is_equal_approx(die_anim.length, EXPECTED_DIE_LENGTH), variant + ": die length equals 2.20",
		"got %s" % die_anim.length)
	require(death_time <= die_anim.length, variant + ": DeathAnimTime within die length",
		"DeathAnimTime=%s, die.length=%s" % [death_time, die_anim.length])
	player.seek(death_time, true)
	var pelvis := rig.get_node_or_null("Visuals/Skeleton2D/root/pelvis") as Node2D
	var chest := rig.get_node_or_null("Visuals/Skeleton2D/root/pelvis/chest") as Node2D
	require(pelvis != null and chest != null, variant + ": core body bones present")
	if pelvis != null and chest != null:
		var settled_pelvis := pelvis.global_transform
		var settled_chest := chest.global_transform
		player.seek(die_anim.length, true)
		var pelvis_settled := pelvis.global_transform.is_equal_approx(settled_pelvis)
		var chest_origin_settled := chest.global_transform.origin.is_equal_approx(settled_chest.origin)
		var chest_rot_delta := absf(angle_difference(settled_chest.get_rotation(), chest.global_transform.get_rotation()))
		require(pelvis_settled and chest_origin_settled and chest_rot_delta <= deg_to_rad(0.5),
			variant + ": core body bones settled between DeathAnimTime and die end",
			"pelvis_settled=%s, chest_origin_settled=%s, chest_rot_delta=%.5f rad" % [pelvis_settled, chest_origin_settled, chest_rot_delta])
	else:
		player.seek(die_anim.length, true)
	player.advance(0.02)
	require(not player.is_playing() and player.get_queue().is_empty(), variant + ": death holds without next")
