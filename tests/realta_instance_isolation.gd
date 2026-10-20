extends SceneTree
## Human requester: wjdrb, 2026-10-20.
## Request: "위 회고 수정하고 깃 이그노어에 pycache 추가하기."
## The selected retrospective explicitly proposed an automated two-instance
## isolation test. In the review reproduction, starting idle on a second rig
## changed the first rig's attack reveal from about .941 to 0 at .15 seconds.
## Each character must retain its own animated effect state. Compare the first
## rig's value before/after the other rig advances, rather than fixing a sampled
## reveal value as the expected artwork. Cover both variants and actor orders.

const VARIANTS := ["combat", "merchant"]
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
		check_isolation(first, second, "%s: first attacks" % variant)
		check_isolation(second, first, "%s: second attacks" % variant)
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

func check_isolation(acting: Node, other: Node, label: String) -> void:
	var acting_player: AnimationPlayer = acting.get_node("AnimationPlayer")
	var other_player: AnimationPlayer = other.get_node("AnimationPlayer")
	var slash := acting.get_node_or_null("Slash") as Polygon2D
	if slash == null or not slash.material is ShaderMaterial:
		failures.append(label + ": missing Slash ShaderMaterial")
		return
	if not acting_player.has_animation("attack") or not other_player.has_animation("idle"):
		failures.append(label + ": missing attack or idle")
		return
	acting_player.stop()
	other_player.stop()
	acting_player.play("attack", 0)
	acting_player.advance(0.15)
	var material := slash.material as ShaderMaterial
	var before: float = material.get_shader_parameter("reveal")
	if before <= 0.0 or slash.modulate.a <= 0.0:
		failures.append(label + ": attack must have a visible trail at .15 seconds")
	other_player.play("idle", 0)
	other_player.advance(0.01)
	var after: float = material.get_shader_parameter("reveal")
	if after != before:
		failures.append("%s: other rig's idle changed reveal from %s to %s" % [label, before, after])
	completed_cases += 1
	print("%s: reveal before=%s, after=%s" % [label, before, after])
