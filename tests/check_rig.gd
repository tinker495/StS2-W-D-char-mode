extends SceneTree
## Explicit human authorization: wjdrb, 2026-10-20 (retrospective item 2);
## modernized 2026-11-21 for unified rig-c combat/merchant rig;
## modernized 2026-11-27 by KyuSeok Jung (wjdrb) (GitHub Issue #46 / Q6=A,
## per docs/agents/test-ownership.md) for native libspine_godot SpineSprite
## (.spjson / .spatlas), guard clip, Track 1 blink_loop overlay, FlipX
## mirroring, and char_select_bg_realta.tscn single-animation contract.

const COMBAT_CLIPS := ["attack", "blink_loop", "cast", "die", "guard", "hurt", "idle_loop", "relaxed_loop"]
const ACTION_CLIPS := ["attack", "guard", "hurt", "cast"]
const FLIP_CHECK_CLIPS := ["idle_loop", "attack", "guard", "cast", "die"]
const EXPECTED_ATTACK_DELAY := 0.54
const EXPECTED_DEATH_TIME := 1.66
const EXPECTED_DIE_LENGTH := 2.20
const EXPECTED_SELECT_LENGTH := 8.00
const EXPECTED_CUES := {
	"attack": {"cue": "attack_hit", "time": EXPECTED_ATTACK_DELAY},
	"guard": {"cue": "guard_brace", "time": 0.18},
	"cast": {"cue": "cast_release", "time": 0.25},
	"die": {"cue": "weapon_release", "time": 0.12},
}
const EXPECTED_TRIGGERS := {
	"Idle": "idle_loop",
	"Relaxed": "relaxed_loop",
	"Dead": "die",
	"Attack": "attack",
	"Guard": "guard",
	"Hit": "hurt",
	"Cast": "cast",
	"PowerUp": "cast",
}
const FPS := 24.0

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
	require(ClassDB.class_exists("SpineSprite"), "SpineSprite GDExtension loaded")
	require(ClassDB.class_exists("SpineSkeletonDataResource"), "SpineSkeletonDataResource GDExtension loaded")
	if not ClassDB.class_exists("SpineSprite"):
		printerr("FAIL: SpineSprite GDExtension not loaded")
		quit(1)
		return

	var realta_cs := FileAccess.get_file_as_string("res://Sts2WDCode/Characters/Realta/Realta.cs")
	var attack_delay = parse_cs_float(realta_cs, "AttackAnimDelay")
	require(attack_delay != null, "AttackAnimDelay declaration", "Expected explicit float in Realta.cs")
	if attack_delay != null:
		require(is_equal_approx(attack_delay, EXPECTED_ATTACK_DELAY), "AttackAnimDelay equals 0.54", "got %s" % attack_delay)

	var death_time = parse_cs_float(realta_cs, "DeathAnimTime")
	require(death_time != null, "DeathAnimTime declaration", "Expected explicit float in Realta.cs")
	if death_time != null:
		require(is_equal_approx(death_time, EXPECTED_DEATH_TIME), "DeathAnimTime equals 1.66", "got %s" % death_time)

	check_realta_cs_animator(realta_cs)
	check_card_guard_patch()

	var effective_attack_delay: float = attack_delay if attack_delay != null else EXPECTED_ATTACK_DELAY
	var effective_death_time: float = death_time if death_time != null else EXPECTED_DEATH_TIME

	for variant in ["combat", "merchant"]:
		await check_character_scene(variant, effective_attack_delay, effective_death_time)

	check_merchant_root_promotion()
	await check_select_scene()

	for rule in failures:
		printerr("FAIL: ", rule, " ", failures[rule])
	if failures.is_empty():
		print("PASS: Realta rig checks (%d native samples)" % sample_count)
		quit(0)
	else:
		printerr("FAIL: Realta rig checks (%d rules)" % failures.size())
		quit(1)


func check_realta_cs_animator(source: String) -> void:
	require(source.find("GenerateAnimator(MegaSprite controller, Creature creature)") != -1,
		"Realta.cs overrides GenerateAnimator")
	require(source.find("low_health_loop") == -1,
		"Realta.cs omits non-existent low_health_loop")
	for trigger in EXPECTED_TRIGGERS:
		var clip: String = EXPECTED_TRIGGERS[trigger]
		var has_clip := source.find("\"%s\"" % clip) != -1
		var has_trigger := source.find("\"%s\"" % trigger) != -1
		require(has_clip and has_trigger, "Realta.cs trigger %s -> %s" % [trigger, clip])


func check_card_guard_patch() -> void:
	var patch_cs := FileAccess.get_file_as_string("res://Sts2WDCode/Cards/CardPlayAnimationPatch.cs")
	require(patch_cs != "", "CardPlayAnimationPatch.cs present")
	if patch_cs != "":
		require(patch_cs.find("GuardTrigger = \"Guard\"") != -1, "CardPlayAnimationPatch defines GuardTrigger")
		require(patch_cs.find("CardType.Skill") != -1, "CardPlayAnimationPatch restricts auto-guard to Skill")
		require(patch_cs.find("GainsBlock") != -1 and patch_cs.find("CardTag.Defend") != -1,
			"CardPlayAnimationPatch checks GainsBlock and Defend tag")
		require(patch_cs.find("CustomPlayAnimTrigger") != -1,
			"CardPlayAnimationPatch honors Sts2WDCard.CustomPlayAnimTrigger")


func check_character_scene(variant: String, attack_delay: float, death_time: float) -> void:
	var path := "res://Sts2WD/scenes/realta/%s.tscn" % variant
	var scene := load(path) as PackedScene
	require(scene != null, variant + ": scene loads", path)
	if scene == null:
		return

	var wrapper := scene.instantiate() as Node2D
	root.add_child(wrapper)
	await process_frame

	check_canvas_layers(wrapper, variant + "/mount")

	var visuals := wrapper.get_node_or_null("%Visuals") as Node2D
	require(visuals != null and visuals.get_class() == "SpineSprite",
		variant + ": %Visuals mounted as SpineSprite",
		str(visuals.get_class() if visuals != null else "null"))
	if visuals == null or visuals.get_class() != "SpineSprite":
		wrapper.free()
		return

	var shadow := visuals.get_node_or_null("GroundShadow") as CanvasItem
	require(shadow != null and shadow.show_behind_parent,
		variant + ": GroundShadow retained behind SpineSprite")

	var skel_data: Resource = visuals.get_skeleton_data_res()
	require(skel_data != null and skel_data.is_skeleton_data_loaded(),
		variant + ": SpineSkeletonDataResource loaded")
	if skel_data == null or not skel_data.is_skeleton_data_loaded():
		wrapper.free()
		return

	var anim_names: Array[String] = []
	for anim_obj in skel_data.get_animations():
		anim_names.append(str(anim_obj.get_name()))
	anim_names.sort()
	require(anim_names == COMBAT_CLIPS, variant + ": eight Spine clips", str(anim_names))

	var anim_state: Object = visuals.get_animation_state()
	var skeleton: Object = visuals.get_skeleton()
	var expected_idle := "relaxed_loop" if variant == "merchant" else "idle_loop"
	var cur0: Object = anim_state.get_current(0)
	require(cur0 != null and cur0.get_animation() != null and cur0.get_animation().get_name() == expected_idle and cur0.get_loop(),
		variant + ": initial Track 0 loop is " + expected_idle)

	var cur1: Object = anim_state.get_current(1)
	if variant == "combat":
		require(cur1 != null and cur1.get_animation() != null and cur1.get_animation().get_name() == "blink_loop" and cur1.get_loop(),
			"combat: initial Track 1 blink_loop active")
	else:
		require(cur1 == null, "merchant: Track 1 empty on relaxed_loop")

	visuals.set_update_mode(2) # SpineConstant.UpdateMode_Manual

	if variant == "combat":
		check_blink_overlay_routing(visuals, anim_state)

	for clip in COMBAT_CLIPS:
		sample_clip(wrapper, visuals, skel_data, anim_state, skeleton, variant, clip, attack_delay)

	check_action_return_and_death(visuals, skel_data, anim_state, skeleton, variant, expected_idle, death_time)
	if variant == "combat":
		check_flip_x(visuals, skel_data, anim_state, skeleton)

	wrapper.free()


func check_blink_overlay_routing(visuals: Node2D, anim_state: Object) -> void:
	for action in ["guard", "attack", "hurt", "cast", "die"]:
		var e: Object = anim_state.set_animation(action, false, 0)
		if e != null:
			e.set_mix_duration(0.0)
		visuals.update_skeleton(0.02)
		require(anim_state.get_current(1) == null,
			"combat: Track 1 blink_loop cleared during " + action)
		var back: Object = anim_state.set_animation("idle_loop", true, 0)
		if back != null:
			back.set_mix_duration(0.0)
		visuals.update_skeleton(0.02)
		var overlay: Object = anim_state.get_current(1)
		require(overlay != null and overlay.get_animation() != null and overlay.get_animation().get_name() == "blink_loop",
			"combat: Track 1 blink_loop restored after " + action)


func sample_clip(
	wrapper: Node2D,
	visuals: Node2D,
	skel_data: Resource,
	anim_state: Object,
	skeleton: Object,
	variant: String,
	clip: String,
	attack_delay: float
) -> void:
	var anim_res: Object = skel_data.find_animation(clip)
	if anim_res == null:
		require(false, "%s/%s: animation present" % [variant, clip])
		return

	var recorded_events: Array[Dictionary] = []
	var on_event := func(_sprite: Object, _state: Object, _entry: Object, event: Object) -> void:
		recorded_events.append({
			"name": str(event.get_data().get_event_name()),
			"time": float(event.get_time()),
		})
	visuals.animation_event.connect(on_event)

	anim_state.clear_tracks()
	skeleton.set_to_setup_pose()
	skeleton.set_scale_x(1.0)
	skeleton.set_scale_y(1.0)
	var entry: Object = anim_state.set_animation(clip, false, 0)
	if entry != null:
		entry.set_mix_duration(0.0)
	visuals.update_skeleton(0.0)

	var duration: float = anim_res.get_duration()
	var steps := maxi(1, int(ceil(duration * FPS)) + 1)
	for step in steps:
		if step > 0:
			visuals.update_skeleton(1.0 / FPS)
		sample_count += 1
		check_canvas_layers(wrapper, "%s/%s" % [variant, clip])
		for bone in skeleton.get_bones():
			var wx: float = bone.get_world_x()
			var wy: float = bone.get_world_y()
			if is_nan(wx) or is_inf(wx) or is_nan(wy) or is_inf(wy):
				require(false, "%s/%s: finite bone world transform" % [variant, clip], "step %d" % step)
				break

	visuals.animation_event.disconnect(on_event)

	if EXPECTED_CUES.has(clip):
		var expected: Dictionary = EXPECTED_CUES[clip]
		var expected_name: String = expected["cue"]
		var expected_time: float = attack_delay if clip == "attack" else float(expected["time"])
		var matched := false
		for item in recorded_events:
			if item["name"] == expected_name and is_equal_approx(float(item["time"]), expected_time):
				matched = true
				break
		require(matched, "%s/%s: Spine event %s" % [variant, clip, expected_name],
			"expected %s at %.2fs, got %s" % [expected_name, expected_time, str(recorded_events)])

	if clip == "guard":
		anim_state.clear_tracks()
		skeleton.set_to_setup_pose()
		var ge: Object = anim_state.set_animation("guard", false, 0)
		if ge != null:
			ge.set_mix_duration(0.0)
		visuals.update_skeleton(0.181)
		var sigil_slot: Object = skeleton.find_slot("sigil_inner")
		require(sigil_slot != null and sigil_slot.get_attachment() != null and sigil_slot.get_color().a > 0.3,
			variant + "/guard: arcane ward sigil_inner active at guard_brace")

	print("%s/%s: %d samples" % [variant, clip, steps])


func check_action_return_and_death(
	visuals: Node2D,
	skel_data: Resource,
	anim_state: Object,
	skeleton: Object,
	variant: String,
	expected_idle: String,
	death_time: float
) -> void:
	for action in ACTION_CLIPS:
		var anim_res: Object = skel_data.find_animation(action)
		if anim_res == null:
			continue
		anim_state.clear_tracks()
		skeleton.set_to_setup_pose()
		var first_entry: Object = anim_state.set_animation(action, false, 0)
		if first_entry != null:
			first_entry.set_mix_duration(0.0)
		var queued: Object = anim_state.add_animation(expected_idle, 0.0, true, 0)
		if queued != null:
			queued.set_mix_duration(0.10)
		visuals.update_skeleton(anim_res.get_duration() + 0.05)
		visuals.update_skeleton(0.10)
		var current: Object = anim_state.get_current(0)
		require(current != null and current.get_animation() != null and current.get_animation().get_name() == expected_idle,
			"%s/%s: queued transition returns to %s" % [variant, action, expected_idle])

	var die_anim: Object = skel_data.find_animation("die")
	require(die_anim != null, variant + ": die animation exists")
	if die_anim == null:
		return
	var die_len: float = die_anim.get_duration()
	require(is_equal_approx(die_len, EXPECTED_DIE_LENGTH), variant + ": die length equals 2.20", "got %s" % die_len)
	require(death_time <= die_len, variant + ": DeathAnimTime within die length")

	anim_state.clear_tracks()
	skeleton.set_to_setup_pose()
	var die_entry: Object = anim_state.set_animation("die", false, 0)
	if die_entry != null:
		die_entry.set_mix_duration(0.0)
	visuals.update_skeleton(death_time)

	var pelvis: Object = skeleton.find_bone("pelvis")
	var chest: Object = skeleton.find_bone("chest")
	require(pelvis != null and chest != null, variant + ": pelvis and chest SpineBones present")
	if pelvis != null and chest != null:
		var p0 := Vector2(pelvis.get_world_x(), pelvis.get_world_y())
		var pr0: float = pelvis.get_world_rotation_x()
		var c0 := Vector2(chest.get_world_x(), chest.get_world_y())
		var cr0: float = chest.get_world_rotation_x()

		visuals.update_skeleton(die_len - death_time)
		var p1 := Vector2(pelvis.get_world_x(), pelvis.get_world_y())
		var pr1: float = pelvis.get_world_rotation_x()
		var c1 := Vector2(chest.get_world_x(), chest.get_world_y())
		var cr1: float = chest.get_world_rotation_x()

		var pelvis_settled := p0.distance_to(p1) <= 0.5 and absf(pr1 - pr0) <= 0.5
		var chest_settled := c0.distance_to(c1) <= 0.5 and absf(cr1 - cr0) <= 0.5
		require(pelvis_settled and chest_settled,
			variant + ": core body bones settled between DeathAnimTime and die end",
			"pelvis_d=%.4f, chest_d=%.4f, chest_rot_d=%.4f" % [p0.distance_to(p1), c0.distance_to(c1), absf(cr1 - cr0)])

		visuals.update_skeleton(0.10)
		var p2 := Vector2(pelvis.get_world_x(), pelvis.get_world_y())
		require(die_entry.is_complete() and p1.distance_to(p2) <= 0.01,
			variant + ": non-looping die holds settled endpoint")


func check_flip_x(visuals: Node2D, skel_data: Resource, anim_state: Object, skeleton: Object) -> void:
	var orig_scale := visuals.scale
	for clip in FLIP_CHECK_CLIPS:
		var anim_res: Object = skel_data.find_animation(clip)
		if anim_res == null:
			continue
		var probe_t := minf(0.54, anim_res.get_duration() * 0.5)

		anim_state.clear_tracks()
		skeleton.set_to_setup_pose()
		skeleton.set_scale_x(1.0)
		visuals.scale = orig_scale
		var e_pos: Object = anim_state.set_animation(clip, false, 0)
		if e_pos != null:
			e_pos.set_mix_duration(0.0)
		visuals.update_skeleton(probe_t)
		var hand_pos_x: float = skeleton.find_bone("hand_r").get_world_x()

		anim_state.clear_tracks()
		skeleton.set_to_setup_pose()
		skeleton.set_scale_x(-1.0)
		visuals.scale = Vector2(-absf(orig_scale.x), orig_scale.y)
		var e_neg: Object = anim_state.set_animation(clip, false, 0)
		if e_neg != null:
			e_neg.set_mix_duration(0.0)
		visuals.update_skeleton(probe_t)
		var hand_neg_x: float = skeleton.find_bone("hand_r").get_world_x()

		var all_finite := true
		for bone in skeleton.get_bones():
			if is_nan(bone.get_world_x()) or is_nan(bone.get_world_y()):
				all_finite = false
				break
		require(all_finite and is_equal_approx(hand_neg_x, -hand_pos_x),
			"combat/%s: FlipX mirrors world X cleanly without NaN" % clip,
			"pos_x=%.3f, neg_x=%.3f" % [hand_pos_x, hand_neg_x])

	skeleton.set_scale_x(1.0)
	visuals.scale = orig_scale


func check_merchant_root_promotion() -> void:
	var patch_cs := FileAccess.get_file_as_string("res://Sts2WDCode/Characters/SpineMerchantScenePatch.cs")
	require(patch_cs != "" and patch_cs.find("PromoteSpineMerchantChild") != -1,
		"merchant: SpineMerchantScenePatch unwraps %Visuals SpineSprite to NMerchantCharacter child(0)")


func check_select_scene() -> void:
	var scene := load("res://Sts2WD/scenes/realta/char_select_bg_realta.tscn") as PackedScene
	require(scene != null, "select: char_select_bg_realta.tscn loads")
	if scene == null:
		return
	var host := Control.new()
	host.size = Vector2(1920.0, 1080.0)
	var select_bg := scene.instantiate() as Control
	host.add_child(select_bg)
	select_bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	root.add_child(host)
	await process_frame

	check_canvas_layers(select_bg, "select")
	var bg_tex := select_bg.get_node_or_null("Background") as TextureRect
	require(bg_tex != null and bg_tex.texture != null, "select: Background TextureRect loaded")

	var spine_node := select_bg.get_node_or_null("SpineSprite") as Node2D
	require(spine_node != null and spine_node.get_class() == "SpineSprite",
		"select: SpineSprite overlay mounted")
	if spine_node != null and spine_node.get_class() == "SpineSprite":
		var skel_data: Resource = spine_node.get_skeleton_data_res()
		var anims: Array = skel_data.get_animations() if skel_data != null else []
		require(anims.size() == 1 and str(anims[0].get_name()) == "animation",
			"select: single 'animation' clip for NSpineAutoPlayer",
			"count=%d" % anims.size())
		if anims.size() == 1:
			require(is_equal_approx(float(anims[0].get_duration()), EXPECTED_SELECT_LENGTH),
				"select: 'animation' duration equals 8.00s")
		require(spine_node.scale.is_equal_approx(Vector2.ONE) and spine_node.position.is_equal_approx(Vector2(960.0, 1080.0)),
			"select: 1920x1080 aspect_cover transform",
			"pos=%s scale=%s" % [spine_node.position, spine_node.scale])

		host.size = Vector2(1440.0, 960.0)
		await process_frame
		var expected_k := 960.0 / 1080.0
		require(spine_node.scale.is_equal_approx(Vector2.ONE * expected_k) and spine_node.position.is_equal_approx(Vector2(720.0, 480.0 + 540.0 * expected_k)),
			"select: 1440x960 (3:2) aspect_cover transform",
			"pos=%s scale=%s" % [spine_node.position, spine_node.scale])

	host.free()


func check_canvas_layers(container: Node, label: String) -> void:
	var nodes := container.find_children("*", "CanvasItem", true, false)
	if container is CanvasItem:
		nodes.append(container)
	for node in nodes:
		var item := node as CanvasItem
		require(item.z_index == 0 and not item.is_set_as_top_level(),
			label + ": zero Z/no top_level " + str(container.get_path_to(item)))
