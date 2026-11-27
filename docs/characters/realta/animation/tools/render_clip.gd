extends SceneTree
## Observation-only native SpineSprite capture; retains the full fixed viewport and never defines acceptance criteria.

const CLIPS := ["idle_loop", "attack", "guard", "hurt", "cast", "die", "relaxed_loop"]
const ALIASES := {
	"idle_combat": "idle_loop",
	"idle_merchant": "relaxed_loop",
	"relaxed": "relaxed_loop",
	"powerup": "cast",
}
const FPS := 24.0
const VIEWPORT_ORIGIN := Vector2(240.0, 391.375)
const VIEWPORT_SCALE := Vector2(5.0 / 24.0, 5.0 / 24.0)


func _initialize() -> void:
	capture.call_deferred()


func _resolve_clip(raw_clip: String, variant: String) -> String:
	if raw_clip == "idle":
		return "relaxed_loop" if variant == "merchant" else "idle_loop"
	return ALIASES.get(raw_clip, raw_clip)


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
	if not ClassDB.class_exists("SpineSprite"):
		push_error("SpineSprite GDExtension is not loaded in this runtime host.")
		quit(1)
		return

	var viewport := SubViewport.new()
	viewport.size = Vector2i(640, 420)
	viewport.transparent_bg = true
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)

	var scene_path := "res://Sts2WD/scenes/realta/%s.tscn" % args[0]
	var scene := load(scene_path) as PackedScene
	if scene == null:
		push_error("Could not load scene: " + scene_path)
		quit(1)
		return

	var requested: Array = CLIPS if args[1] == "all" else [args[1]]
	for raw_clip in requested:
		var target_clip := _resolve_clip(str(raw_clip), args[0])
		var wrapper := scene.instantiate() as Node2D
		viewport.add_child(wrapper)
		await process_frame

		var visuals := wrapper.get_node_or_null("%Visuals") as Node2D
		if visuals == null or visuals.get_class() != "SpineSprite":
			push_error("Expected SpineSprite at %%Visuals in %s" % scene_path)
			quit(1)
			return

		var shadow := visuals.get_node_or_null("GroundShadow") as CanvasItem
		if shadow != null:
			shadow.visible = false

		visuals.position = VIEWPORT_ORIGIN - wrapper.position
		visuals.scale = VIEWPORT_SCALE
		visuals.set_update_mode(2) # SpineConstant.UpdateMode_Manual

		var skel_data: Resource = visuals.get_skeleton_data_res()
		var anim_res: Object = skel_data.find_animation(target_clip) if skel_data != null else null
		if anim_res == null:
			push_error("Missing Spine clip '%s' in %s" % [target_clip, scene_path])
			quit(1)
			return

		var anim_state: Object = visuals.get_animation_state()
		var skeleton: Object = visuals.get_skeleton()
		anim_state.clear_tracks()
		skeleton.set_to_setup_pose()
		var entry: Object = anim_state.set_animation(target_clip, target_clip.ends_with("_loop"), 0)
		if entry != null:
			entry.set_mix_duration(0.0)
		if args[0] == "combat" and target_clip == "idle_loop" and skel_data.find_animation("blink_loop") != null:
			var blink_entry: Object = anim_state.set_animation("blink_loop", true, 1)
			if blink_entry != null:
				blink_entry.set_mix_duration(0.0)
		visuals.update_skeleton(0.0)

		var folder: String = args[2].path_join(args[0]).path_join(str(raw_clip))
		var directory_error := DirAccess.make_dir_recursive_absolute(folder)
		if directory_error != OK:
			push_error(error_string(directory_error))
			quit(1)
			return

		var duration: float = anim_res.get_duration()
		var count := maxi(1, int(ceil(duration * FPS)))
		for frame in count:
			if frame > 0:
				visuals.update_skeleton(1.0 / FPS)
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
			"variant": args[0], "clip": raw_clip, "resolved_clip": target_clip,
			"frame_count": count, "fps": FPS, "duration": duration,
			"size": [640, 420], "origin": [VIEWPORT_ORIGIN.x, VIEWPORT_ORIGIN.y], "scale": VIEWPORT_SCALE.x,
			"engine": Engine.get_version_info().string,
			"renderer": "gl_compatibility", "runtime": "SpineSprite", "package": args.size() > 3,
		}, "\t"))
		manifest.close()
		print("%s/%s: %d frames" % [args[0], raw_clip, count])
		wrapper.free()

	print("CAPTURE_COMPLETE")
	quit(0)
