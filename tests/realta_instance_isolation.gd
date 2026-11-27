extends SceneTree
## Human requester: wjdrb, 2026-10-20.
## Request: "위 회고 수정하고 깃 이그노어에 pycache 추가하기."
## Modernized 2026-11-21 for unified rig-c combat/merchant rig.
## Modernization authorized by human requester KyuSeok Jung (wjdrb) on 2026-11-27
## (GitHub Issue #46 / Q6=A, per docs/agents/test-ownership.md):
## Verify two-instance native SpineSprite state isolation across combat and
## merchant scenes (both actor orders = 4 cases), checking animation_event cues,
## slot attachments & colors (sigil_core, sigil_inner, weapon_head), and SpineBone
## world transforms (pelvis, chest, upper_arm_r) across cast, guard, and attack
## while the second SpineSprite advances independently despite sharing cached
## SpineSkeletonDataResource.

const VARIANTS := ["combat", "merchant"]
const TRACKED_BONES := ["pelvis", "chest", "upper_arm_r"]

var failures: Array[String] = []
var completed_cases := 0


func _initialize() -> void:
	run.call_deferred()


func run() -> void:
	if not ClassDB.class_exists("SpineSprite"):
		failures.append("SpineSprite GDExtension is not loaded")
	else:
		for variant in VARIANTS:
			var path := "res://Sts2WD/scenes/realta/%s.tscn" % variant
			var scene := load(path) as PackedScene
			if scene == null:
				failures.append("Could not load " + path)
				continue
			var first := scene.instantiate()
			var second := scene.instantiate()
			root.add_child(first)
			root.add_child(second)
			await process_frame

			var first_spine := first.get_node_or_null("%Visuals") as Node2D
			var second_spine := second.get_node_or_null("%Visuals") as Node2D
			if first_spine == null or second_spine == null or first_spine.get_class() != "SpineSprite" or second_spine.get_class() != "SpineSprite":
				failures.append("%s: missing %%Visuals SpineSprite" % variant)
				first.free()
				second.free()
				continue

			if first_spine.get_skeleton_data_res() != second_spine.get_skeleton_data_res():
				failures.append("%s: expected cached SpineSkeletonDataResource to be shared" % variant)

			first_spine.set_update_mode(2) # SpineConstant.UpdateMode_Manual
			second_spine.set_update_mode(2)

			await check_isolation(first_spine, second_spine, "%s: first acts" % variant, variant)
			await check_isolation(second_spine, first_spine, "%s: second acts" % variant, variant)
			first.free()
			second.free()

	for failure in failures:
		printerr("FAIL: ", failure)
	if failures.is_empty() and completed_cases == VARIANTS.size() * 2:
		print("PASS: Realta instance isolation (%d cases)" % completed_cases)
		quit(0)
	else:
		printerr("FAIL: Realta instance isolation (%d cases completed)" % completed_cases)
		quit(1)


func _snapshot_bones(skeleton: Object, label: String) -> Dictionary:
	var snapshot: Dictionary = {}
	for bone_name in TRACKED_BONES:
		var bone: Object = skeleton.find_bone(bone_name)
		if bone == null:
			failures.append("%s: missing SpineBone %s" % [label, bone_name])
			return {}
		snapshot[bone_name] = Vector3(bone.get_world_x(), bone.get_world_y(), bone.get_world_rotation_x())
	return snapshot


func _verify_bones_unchanged(skeleton: Object, before: Dictionary, label: String, phase: String) -> void:
	for bone_name in before.keys():
		var bone: Object = skeleton.find_bone(bone_name)
		var actual := Vector3(bone.get_world_x(), bone.get_world_y(), bone.get_world_rotation_x())
		if actual != before[bone_name]:
			failures.append("%s (%s): idle rig mutated %s world state from %s to %s" % [
				label, phase, bone_name, before[bone_name], actual
			])


func _slot_attachment_name(skeleton: Object, slot_name: String) -> String:
	var slot: Object = skeleton.find_slot(slot_name)
	if slot == null:
		return "<missing-slot>"
	var att: Object = slot.get_attachment()
	return str(att.get_attachment_name()) if att != null else ""


func _slot_alpha(skeleton: Object, slot_name: String) -> float:
	var slot: Object = skeleton.find_slot(slot_name)
	return float(slot.get_color().a) if slot != null else 0.0


func _start_clean(spine: Node2D, clip: String, loop: bool) -> void:
	var state: Object = spine.get_animation_state()
	var skel: Object = spine.get_skeleton()
	state.clear_tracks()
	skel.set_to_setup_pose()
	var entry: Object = state.set_animation(clip, loop, 0)
	if entry != null:
		entry.set_mix_duration(0.0)
	spine.update_skeleton(0.0)


func check_isolation(acting: Node2D, idle_rig: Node2D, label: String, variant: String) -> void:
	var acting_skel: Object = acting.get_skeleton()
	var idle_skel: Object = idle_rig.get_skeleton()
	var skel_data: Resource = acting.get_skeleton_data_res()
	var idle_clip := "relaxed_loop" if variant == "merchant" else "idle_loop"

	for clip in ["cast", "guard", "attack", idle_clip]:
		if skel_data.find_animation(clip) == null:
			failures.append("%s: missing Spine animation '%s'" % [label, clip])
			return

	var acting_cues: Array[StringName] = []
	var idle_cues: Array[StringName] = []
	var on_acting_event := func(_s: Object, _st: Object, _te: Object, ev: Object) -> void:
		acting_cues.append(StringName(ev.get_data().get_event_name()))
	var on_idle_event := func(_s: Object, _st: Object, _te: Object, ev: Object) -> void:
		idle_cues.append(StringName(ev.get_data().get_event_name()))
	acting.animation_event.connect(on_acting_event)
	idle_rig.animation_event.connect(on_idle_event)

	# --- Phase 1: cast isolation (0.25s cast_release & sigil_core) ---
	_start_clean(idle_rig, idle_clip, true)
	_start_clean(acting, "cast", false)
	acting_cues.clear()
	idle_cues.clear()
	acting.update_skeleton(0.251)
	await process_frame

	if not (&"cast_release" in acting_cues):
		failures.append("%s (cast): acting rig did not emit cast_release (got %s)" % [label, acting_cues])
	if not idle_cues.is_empty():
		failures.append("%s (cast): idle rig unexpectedly emitted %s" % [label, idle_cues])

	var cast_sigil_att := _slot_attachment_name(acting_skel, "sigil_core")
	var cast_sigil_alpha := _slot_alpha(acting_skel, "sigil_core")
	if cast_sigil_att == "" or cast_sigil_alpha <= 0.5:
		failures.append("%s (cast): expected sigil_core active with alpha > 0.5 (att=%s, a=%.3f)" % [
			label, cast_sigil_att, cast_sigil_alpha
		])
	if _slot_attachment_name(idle_skel, "sigil_core") != "":
		failures.append("%s (cast): idle rig sigil_core should remain unattached" % label)

	var cast_bones := _snapshot_bones(acting_skel, label + " (cast)")
	if cast_bones.is_empty():
		acting.animation_event.disconnect(on_acting_event)
		idle_rig.animation_event.disconnect(on_idle_event)
		return

	idle_rig.update_skeleton(0.25)
	idle_rig.update_skeleton(0.50)
	await process_frame

	if _slot_attachment_name(acting_skel, "sigil_core") != cast_sigil_att or _slot_alpha(acting_skel, "sigil_core") != cast_sigil_alpha:
		failures.append("%s (cast): idle rig mutated acting sigil_core state" % label)
	if _slot_attachment_name(idle_skel, "sigil_core") != "":
		failures.append("%s (cast): idle rig sigil_core attached during %s" % [label, idle_clip])
	if not idle_cues.is_empty():
		failures.append("%s (cast): idle rig emitted %s during %s" % [label, idle_cues, idle_clip])
	_verify_bones_unchanged(acting_skel, cast_bones, label, "cast")

	# --- Phase 2: guard isolation (0.18s guard_brace & sigil_inner) ---
	_start_clean(idle_rig, idle_clip, true)
	_start_clean(acting, "guard", false)
	acting_cues.clear()
	idle_cues.clear()
	acting.update_skeleton(0.181)
	await process_frame

	if not (&"guard_brace" in acting_cues):
		failures.append("%s (guard): acting rig did not emit guard_brace (got %s)" % [label, acting_cues])
	if not idle_cues.is_empty():
		failures.append("%s (guard): idle rig unexpectedly emitted %s" % [label, idle_cues])

	var guard_ward_att := _slot_attachment_name(acting_skel, "sigil_inner")
	var guard_ward_alpha := _slot_alpha(acting_skel, "sigil_inner")
	if guard_ward_att == "" or guard_ward_alpha <= 0.3:
		failures.append("%s (guard): expected sigil_inner active with alpha > 0.3 (att=%s, a=%.3f)" % [
			label, guard_ward_att, guard_ward_alpha
		])
	var guard_bones := _snapshot_bones(acting_skel, label + " (guard)")
	if guard_bones.is_empty():
		acting.animation_event.disconnect(on_acting_event)
		idle_rig.animation_event.disconnect(on_idle_event)
		return

	idle_rig.update_skeleton(0.25)
	idle_rig.update_skeleton(0.50)
	await process_frame

	if _slot_attachment_name(acting_skel, "sigil_inner") != guard_ward_att or _slot_alpha(acting_skel, "sigil_inner") != guard_ward_alpha:
		failures.append("%s (guard): idle rig mutated acting sigil_inner state" % label)
	if not idle_cues.is_empty():
		failures.append("%s (guard): idle rig emitted %s during %s" % [label, idle_cues, idle_clip])
	_verify_bones_unchanged(acting_skel, guard_bones, label, "guard")

	# --- Phase 3: attack isolation (0.54s attack_hit & weapon_head swap) ---
	_start_clean(idle_rig, idle_clip, true)
	_start_clean(acting, "attack", false)
	acting_cues.clear()
	idle_cues.clear()
	acting.update_skeleton(0.541)
	await process_frame

	if not (&"attack_hit" in acting_cues):
		failures.append("%s (attack): acting rig did not emit attack_hit (got %s)" % [label, acting_cues])
	if not idle_cues.is_empty():
		failures.append("%s (attack): idle rig unexpectedly emitted %s" % [label, idle_cues])

	var attack_blade_att := _slot_attachment_name(acting_skel, "weapon_head")
	if attack_blade_att == "":
		failures.append("%s (attack): expected weapon_head mirrored blade active at 0.54s" % label)
	if _slot_attachment_name(idle_skel, "weapon_head") != "":
		failures.append("%s (attack): idle rig weapon_head should remain unattached" % label)

	var attack_bones := _snapshot_bones(acting_skel, label + " (attack)")
	if attack_bones.is_empty():
		acting.animation_event.disconnect(on_acting_event)
		idle_rig.animation_event.disconnect(on_idle_event)
		return

	idle_rig.update_skeleton(0.25)
	idle_rig.update_skeleton(0.50)
	await process_frame

	if _slot_attachment_name(acting_skel, "weapon_head") != attack_blade_att:
		failures.append("%s (attack): idle rig mutated acting weapon_head attachment" % label)
	if _slot_attachment_name(idle_skel, "weapon_head") != "":
		failures.append("%s (attack): idle rig weapon_head attached during %s" % [label, idle_clip])
	if not idle_cues.is_empty():
		failures.append("%s (attack): idle rig emitted %s during %s" % [label, idle_cues, idle_clip])
	_verify_bones_unchanged(acting_skel, attack_bones, label, "attack")

	acting.animation_event.disconnect(on_acting_event)
	idle_rig.animation_event.disconnect(on_idle_event)

	completed_cases += 1
	print("%s: cast(sigil_a=%.3f) guard(ward_a=%.3f) attack(blade=%s) isolated against %s" % [
		label, cast_sigil_alpha, guard_ward_alpha, attack_blade_att, idle_clip
	])
