extends SceneTree

# Screen coordinates from the native 1920×1080 rest room. A shared crop preserves
# the empty second seat and keeps the two characters at their in-game scale.
const SCREEN := Vector2i(1920, 1080)
const SEAT_CROP := Rect2i(486, 500, 500, 368)
const ROOM := "res://scenes/rest_site/overgrowth_rest_site.tscn"
const CHARACTER := "res://scenes/rest_site/characters/%s_rest_site.tscn"

func _initialize():
	call_deferred("capture")

func capture():
	var args := OS.get_cmdline_user_args()
	if GDExtensionManager.load_extension("res://local_spine.gdextension") != OK or not ProjectSettings.load_resource_pack(args[0]):
		quit(1)
		return
	var view := SubViewport.new()
	view.size = SCREEN
	view.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(view)
	var background := Control.new()
	background.position = Vector2(26, 74)
	background.size = Vector2(0.450012, 0.450012)
	view.add_child(background)
	var room: PackedScene = load(ROOM)
	if room == null:
		quit(1)
		return
	background.add_child(room.instantiate())
	var seat := Node2D.new()
	seat.position = Vector2(625, 642)
	seat.scale = Vector2(0.5, 0.5)
	background.add_child(seat)
	var frames := []
	for name_ in ["empty", "ironclad", "silent"]:
		var actor: Node2D
		if name_ != "empty":
			var scene: PackedScene = load(CHARACTER % name_)
			if scene == null:
				quit(1)
				return
			actor = scene.instantiate()
			seat.add_child(actor)
			actor.position = Vector2.ZERO
			actor.get_node("ControlRoot").hide()
			var posed := false
			for child in actor.get_children():
				if child.get_class() == "SpineSprite":
					child.get_animation_state().set_animation("overgrowth_loop", true, 0)
					child.update_skeleton(0.0)
					child.set_time_scale(0.0)
					posed = true
			if not posed:
				push_error("Missing seated SpineSprite: " + name_)
				quit(1)
				return
		await process_frame
		await process_frame
		RenderingServer.force_draw()
		var image := view.get_texture().get_image()
		if image == null or image.is_empty():
			quit(1)
			return
		if name_ == "ironclad" and image.save_png(args[1].path_join("room.png")) != OK:
			quit(1)
			return
		var crop := image.get_region(SEAT_CROP)
		if crop.save_png(args[1].path_join(name_ + "-seat.png")) != OK:
			quit(1)
			return
		frames.append(crop)
		if actor:
			actor.free()
	var board := Image.create(SEAT_CROP.size.x * frames.size(), SEAT_CROP.size.y, false, frames[0].get_format())
	for i in frames.size():
		board.blit_rect(frames[i], Rect2i(Vector2i.ZERO, SEAT_CROP.size), Vector2i(i * SEAT_CROP.size.x, 0))
	if board.save_png(args[1].path_join("seats.png")) != OK:
		quit(1)
		return
	print("CAPTURE_COMPLETE failures=0")
	quit()
