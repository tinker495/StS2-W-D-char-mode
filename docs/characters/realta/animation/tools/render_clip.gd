extends SceneTree
## Native observation tool. Inputs: variant clip|all output_directory [absolute_pck].
## Uses the whole fixed viewport; captures do not define acceptance criteria.

const CLIPS := ["idle_loop", "attack", "hurt", "cast", "die", "relaxed_loop", "revive"]
const FPS := 24.0

func _initialize() -> void:
	capture.call_deferred()

func capture() -> void:
	var args := OS.get_cmdline_user_args()
	if args.size() < 3 or args[0] not in ["combat", "merchant"]:
		push_error("Arguments: combat|merchant clip|all output_directory [absolute_pck]")
		quit(1)
		return
	if args.size() > 3 and not ProjectSettings.load_resource_pack(args[3], true):
		push_error("Could not mount the supplied package")
		quit(1)
		return
	var viewport := SubViewport.new()
	viewport.size = Vector2i(640, 420)
	viewport.transparent_bg = true
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var scene := load("res://Sts2WD/animations/realta/combat/realta_%s_rig.tscn" % args[0]) as PackedScene
	if scene == null:
		quit(1)
		return
	var clips: Array = CLIPS if args[1] == "all" else [args[1]]
	for clip in clips:
		var rig := scene.instantiate()
		rig.position = Vector2(120, 32)
		rig.scale = Vector2.ONE * 0.625
		viewport.add_child(rig)
		var player: AnimationPlayer = rig.get_node("AnimationPlayer")
		player.callback_mode_process = AnimationMixer.ANIMATION_CALLBACK_MODE_PROCESS_MANUAL
		player.stop()
		player.animation_started.disconnect(Callable(rig, "_on_animation_started"))
		await process_frame
		if not player.has_animation(clip):
			push_error("Missing clip: " + str(clip))
			quit(1)
			return
		if clip == "revive":
			player.play("die", 0)
			player.advance(player.get_animation("die").length)
		player.play(clip, 0)
		player.advance(0)
		var folder: String = args[2].path_join(args[0]).path_join(clip)
		var directory_error := DirAccess.make_dir_recursive_absolute(folder)
		if directory_error != OK:
			push_error(error_string(directory_error))
			quit(1)
			return
		var count := int(ceil(player.get_animation(clip).length * FPS))
		for frame in count:
			if frame > 0:
				player.advance(1.0 / FPS)
			await process_frame
			await RenderingServer.frame_post_draw
			var error := viewport.get_texture().get_image().save_png(folder.path_join("%04d.png" % frame))
			if error != OK:
				push_error("Frame save failed: " + error_string(error))
				quit(1)
				return
		var manifest := FileAccess.open(folder.path_join("capture.json"), FileAccess.WRITE)
		if manifest == null:
			push_error("Could not write capture metadata")
			quit(1)
			return
		manifest.store_string(JSON.stringify({
			"variant": args[0], "clip": clip, "frame_count": count, "fps": FPS,
			"size": [640, 420], "origin": [120, 32], "scale": 0.625,
			"engine": Engine.get_version_info().string,
			"renderer": "gl_compatibility", "package": args.size() > 3,
		}, "\t"))
		manifest.close()
		print("%s/%s: %d frames" % [args[0], clip, count])
		rig.free()
	print("CAPTURE_COMPLETE")
	quit(0)
