extends SceneTree
# Renders the first frame of each official character's combat idle and merchant
# relaxed pose from the installed SlayTheSpire2.pck. Run through
# `asset-pipeline.ps1 official-poses`, which prepares the host project.
# User args: <pck path> <output directory>.

const CHARACTERS = ["ironclad", "silent", "defect", "necrobinder", "regent"]
const VIEWPORT_SIZE = 1400
const FIT_SIZE = 1200.0
const MARGIN_RATIO = 0.05

func _initialize():
	call_deferred("go")

func go():
	GDExtensionManager.load_extension("res://local_spine.gdextension")
	var args = OS.get_cmdline_user_args()
	if not ProjectSettings.load_resource_pack(args[0]):
		push_error("Cannot load " + args[0])
		quit(1)
		return
	DirAccess.make_dir_recursive_absolute(args[1])
	var poses = [
		["combat", "res://animations/characters/%s/%s_skel_data.tres", "idle_loop"],
		["merchant", "res://animations/merchant/%s/%s_merchant_skel_data.tres", "relaxed_loop"],
	]
	var failures = 0
	for c in CHARACTERS:
		for pose in poses:
			var filename = "%s-%s-%s-00.png" % [c, pose[0], pose[2]]
			var data = load(pose[1] % [c, c])
			var image = null
			if data and data.find_animation(pose[2]):
				image = await render_first_frame(data, pose[2])
			var saved = image.save_png(args[1].path_join(filename)) if image else ERR_CANT_CREATE
			if saved != OK: failures += 1
			print("CAPTURE ", filename, " ", saved, " ", image.get_size() if image else Vector2i.ZERO)
	print("CAPTURE_COMPLETE failures=", failures)
	quit(1 if failures > 0 else 0)

func render_first_frame(data, animation):
	var viewport = SubViewport.new()
	viewport.size = Vector2i(VIEWPORT_SIZE, VIEWPORT_SIZE)
	viewport.transparent_bg = true
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var sprite = ClassDB.instantiate("SpineSprite")
	viewport.add_child(sprite)
	sprite.set_skeleton_data_res(data)
	sprite.get_animation_state().set_animation(animation, false, 0)
	sprite.update_skeleton(0.0)
	sprite.set_time_scale(0.0)
	# Skeleton bounds only place the pose; the saved margins come from rendered alpha.
	var bounds = sprite.get_skeleton().get_bounds()
	if bounds.size.x <= 0 or bounds.size.y <= 0:
		viewport.queue_free()
		return null
	var scale_value = minf(FIT_SIZE / bounds.size.x, FIT_SIZE / bounds.size.y)
	sprite.scale = Vector2.ONE * scale_value
	sprite.position = (Vector2(VIEWPORT_SIZE, VIEWPORT_SIZE) - bounds.size * scale_value) * 0.5 - bounds.position * scale_value
	await process_frame
	await process_frame
	RenderingServer.force_draw()
	var capture = viewport.get_texture().get_image()
	viewport.queue_free()
	await process_frame
	var used = capture.get_used_rect()
	if used.size.x <= 0 or used.size.y <= 0:
		return null
	var subject = capture.get_region(used)
	subject.convert(Image.FORMAT_RGBA8)
	# A transparent viewport stores premultiplied color; restore straight alpha for PNG.
	for y in subject.get_height():
		for x in subject.get_width():
			var color = subject.get_pixel(x, y)
			if color.a > 0.0 and color.a < 1.0:
				subject.set_pixel(x, y, Color(color.r / color.a, color.g / color.a, color.b / color.a, color.a))
	var margin = int(ceil(maxf(used.size.x, used.size.y) * MARGIN_RATIO))
	var framed = Image.create(used.size.x + margin * 2, used.size.y + margin * 2, false, Image.FORMAT_RGBA8)
	framed.blit_rect(subject, Rect2i(Vector2i.ZERO, used.size), Vector2i(margin, margin))
	return framed
