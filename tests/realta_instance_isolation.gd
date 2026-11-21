extends SceneTree
## Human requester: wjdrb, 2026-10-20.
## Request: "위 회고 수정하고 깃 이그노어에 pycache 추가하기."
## Modernization authorized by human requester on 2026-11-21 (per docs/agents/test-ownership.md):
## Update the two-instance isolation test for the unified `rigc` combat/merchant rig,
## replacing legacy `Slash` `ShaderMaterial` checks with runtime state isolation checks
## (animation cues, Visuals.draw_order, Visuals/sigil_core visibility & modulate, and Skeleton2D Bone2D transforms).
##
## Each character instance must retain its own animated state while another instance
## advances independently. Compare the acting rig's sampled values before and after the
## idle rig advances, covering both variants (`combat`, `merchant`) and both actor orders.

const VARIANTS := ["combat", "merchant"]
const TRACKED_BONES := [
	"Visuals/Skeleton2D/root/pelvis",
	"Visuals/Skeleton2D/root/pelvis/chest",
	"Visuals/Skeleton2D/root/pelvis/chest/upper_arm_r",
]

var failures: Array[String] = []
var completed_cases := 0

func _initialize() -> void:
	run.call_deferred()

func run() -> void:
	for variant in VARIANTS:
		var path := "res://Sts2WD/animations/realta/combat/realta_%s_rig.tscn" % variant
		var scene := load(path) as PackedScene
		if scene == null:
			failures.append("Could not load " + path)
			continue
		var first := scene.instantiate()
		var second := scene.instantiate()
		root.add_child(first)
		root.add_child(second)
		var first_player := first.get_node_or_null("AnimationPlayer") as AnimationPlayer
		var second_player := second.get_node_or_null("AnimationPlayer") as AnimationPlayer
		if first_player == null or second_player == null:
			failures.append("%s: missing AnimationPlayer" % variant)
			first.free()
			second.free()
			continue
		for player in [first_player, second_player]:
			player.callback_mode_process = AnimationMixer.ANIMATION_CALLBACK_MODE_PROCESS_MANUAL
			player.stop()
		# Drain autoplay's deferred phase selection before deterministic playback.
		# Keep the controller's real animation/mixer signal connections intact.
		await process_frame
		await check_isolation(first, second, "%s: first acts" % variant, variant)
		await check_isolation(second, first, "%s: second acts" % variant, variant)
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

func _snapshot_bones(rig: Node, label: String) -> Dictionary:
	var snapshot: Dictionary = {}
	for bone_path in TRACKED_BONES:
		var bone := rig.get_node_or_null(bone_path) as Bone2D
		if bone == null:
			failures.append("%s: missing Bone2D %s" % [label, bone_path])
			return {}
		snapshot[bone_path] = bone.transform
	return snapshot

func _verify_bones_unchanged(rig: Node, before: Dictionary, label: String, phase: String) -> void:
	for bone_path in before.keys():
		var bone := rig.get_node(bone_path) as Bone2D
		if bone.transform != before[bone_path]:
			failures.append("%s (%s): idle rig mutated %s transform from %s to %s" % [
				label, phase, bone_path, before[bone_path], bone.transform
			])

func check_isolation(acting: Node, idle_rig: Node, label: String, variant: String) -> void:
	var acting_player := acting.get_node_or_null("AnimationPlayer") as AnimationPlayer
	var idle_player := idle_rig.get_node_or_null("AnimationPlayer") as AnimationPlayer
	var acting_visuals := acting.get_node_or_null("Visuals") as Node2D
	var idle_visuals := idle_rig.get_node_or_null("Visuals") as Node2D
	var acting_sigil := acting.get_node_or_null("Visuals/sigil_core") as Node2D
	var acting_sigil_poly := acting.get_node_or_null("Visuals/sigil_core/sigil_core") as Polygon2D
	var idle_sigil_poly := idle_rig.get_node_or_null("Visuals/sigil_core/sigil_core") as Polygon2D

	if acting_player == null or idle_player == null:
		failures.append(label + ": missing AnimationPlayer")
		return
	if acting_visuals == null or idle_visuals == null or not ("draw_order" in acting_visuals) or not ("draw_order" in idle_visuals):
		failures.append(label + ": missing Visuals or draw_order property")
		return
	if acting_sigil == null or acting_sigil_poly == null or idle_sigil_poly == null:
		failures.append(label + ": missing Visuals/sigil_core nodes")
		return

	var idle_clip := "relaxed" if variant == "merchant" else "idle"
	for clip in ["cast", "attack", idle_clip]:
		if not acting_player.has_animation(clip) or not idle_player.has_animation(clip):
			failures.append("%s: missing animation '%s'" % [label, clip])
			return

	var acting_cues: Array[StringName] = []
	var idle_cues: Array[StringName] = []
	var on_acting_cue := func(cue: StringName) -> void: acting_cues.append(cue)
	var on_idle_cue := func(cue: StringName) -> void: idle_cues.append(cue)
	acting.animation_cue.connect(on_acting_cue)
	idle_rig.animation_cue.connect(on_idle_cue)

	# --- Phase 1: cast isolation (0.25s spell release peak) ---
	acting_player.stop()
	idle_player.stop()
	acting_cues.clear()
	idle_cues.clear()

	idle_player.play(idle_clip, 0)
	idle_player.advance(0.0)
	acting_player.play("cast", 0)
	acting_player.advance(0.25)
	await process_frame

	if not (&"cast_release" in acting_cues):
		failures.append("%s (cast): acting rig did not emit cast_release cue at 0.25s (got %s)" % [label, acting_cues])
	if not idle_cues.is_empty():
		failures.append("%s (cast): idle rig unexpectedly emitted cues %s" % [label, idle_cues])

	var cast_draw_order: int = acting_visuals.draw_order
	var cast_sigil_alpha: float = acting_sigil.modulate.a
	var cast_sigil_visible: bool = acting_sigil_poly.visible
	if not cast_sigil_visible or cast_sigil_alpha <= 0.5:
		failures.append("%s (cast): expected sigil_core visible with modulate.a > 0.5 at 0.25s (visible=%s, a=%s)" % [
			label, cast_sigil_visible, cast_sigil_alpha
		])
	if idle_sigil_poly.visible:
		failures.append("%s (cast): idle rig sigil_core should remain hidden" % label)

	var cast_bones := _snapshot_bones(acting, label + " (cast)")
	if cast_bones.is_empty():
		acting.animation_cue.disconnect(on_acting_cue)
		idle_rig.animation_cue.disconnect(on_idle_cue)
		return

	idle_player.play(idle_clip, 0)
	idle_player.advance(0.25)
	idle_player.advance(0.5)
	await process_frame

	if acting_visuals.draw_order != cast_draw_order:
		failures.append("%s (cast): idle rig changed acting draw_order from %d to %d" % [
			label, cast_draw_order, acting_visuals.draw_order
		])
	if acting_sigil.modulate.a != cast_sigil_alpha:
		failures.append("%s (cast): idle rig changed acting sigil_core.modulate.a from %s to %s" % [
			label, cast_sigil_alpha, acting_sigil.modulate.a
		])
	if acting_sigil_poly.visible != cast_sigil_visible:
		failures.append("%s (cast): idle rig changed acting sigil_core visibility from %s to %s" % [
			label, cast_sigil_visible, acting_sigil_poly.visible
		])
	if idle_sigil_poly.visible:
		failures.append("%s (cast): idle rig sigil_core became visible during %s" % [label, idle_clip])
	if &"cast_release" in idle_cues or not idle_cues.is_empty():
		failures.append("%s (cast): idle rig emitted cues %s while advancing %s" % [label, idle_cues, idle_clip])
	_verify_bones_unchanged(acting, cast_bones, label, "cast")

	# --- Phase 2: attack isolation (0.54s attack_hit cue & draw_order shift) ---
	acting_player.stop()
	idle_player.stop()
	acting_cues.clear()
	idle_cues.clear()

	idle_player.play(idle_clip, 0)
	idle_player.advance(0.0)
	acting_player.play("attack", 0)
	acting_player.advance(0.54)
	await process_frame

	if not (&"attack_hit" in acting_cues):
		failures.append("%s (attack): acting rig did not emit attack_hit cue at 0.54s (got %s)" % [label, acting_cues])
	if not idle_cues.is_empty():
		failures.append("%s (attack): idle rig unexpectedly emitted cues %s" % [label, idle_cues])

	var attack_draw_order: int = acting_visuals.draw_order
	if attack_draw_order == idle_visuals.draw_order:
		failures.append("%s (attack): expected attack draw_order (%d) to differ from %s draw_order (%d)" % [
			label, attack_draw_order, idle_clip, idle_visuals.draw_order
		])
	var attack_bones := _snapshot_bones(acting, label + " (attack)")
	if attack_bones.is_empty():
		acting.animation_cue.disconnect(on_acting_cue)
		idle_rig.animation_cue.disconnect(on_idle_cue)
		return

	idle_player.advance(0.25)
	idle_player.advance(0.5)
	await process_frame

	if acting_visuals.draw_order != attack_draw_order:
		failures.append("%s (attack): idle rig changed acting draw_order from %d to %d" % [
			label, attack_draw_order, acting_visuals.draw_order
		])
	if not idle_cues.is_empty():
		failures.append("%s (attack): idle rig emitted cues %s while advancing %s" % [label, idle_cues, idle_clip])
	_verify_bones_unchanged(acting, attack_bones, label, "attack")

	acting.animation_cue.disconnect(on_acting_cue)
	idle_rig.animation_cue.disconnect(on_idle_cue)

	completed_cases += 1
	print("%s: cast(draw_order=%d, sigil_a=%.3f) attack(draw_order=%d) isolated against %s" % [
		label, cast_draw_order, cast_sigil_alpha, attack_draw_order, idle_clip
	])
