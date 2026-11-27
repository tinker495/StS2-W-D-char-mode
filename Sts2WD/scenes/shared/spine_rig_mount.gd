extends Node
## Mounts native libspine_godot SpineSprite + SpineSkeletonDataResource during
## PackedScene.instantiate() (NOTIFICATION_SCENE_INSTANTIATED, before add_child or
## BaseLib NodeFactory conversion) while keeping tracked .tscn files free of
## editor-unregistered GDExtension class names.

static var _skeleton_data_cache: Dictionary = {}

@export var spatlas_path := ""
@export var spjson_path := ""
@export var default_mix := 0.10
@export var initial_animation := "idle_loop"
@export var initial_loop := true
@export var blink_animation := ""
@export var blink_base_animation := "idle_loop"
@export_enum("replace_self", "aspect_cover") var mount_mode := "replace_self"
@export var design_size := Vector2(1920.0, 1080.0)
@export var content_offset := Vector2(0.0, 540.0)
@export var attach_auto_player := false

var _mounted := false


static func get_or_load_skeleton_data(atlas_path: String, json_path: String, mix: float) -> Resource:
	var cache_key := "%s|%s|%.4f" % [atlas_path, json_path, mix]
	if _skeleton_data_cache.has(cache_key):
		var cached: Resource = _skeleton_data_cache[cache_key]
		if is_instance_valid(cached):
			return cached
	if not ClassDB.class_exists("SpineSkeletonDataResource"):
		return null
	var atlas_res := ResourceLoader.load(atlas_path)
	var skel_file := ResourceLoader.load(json_path)
	if atlas_res == null or skel_file == null:
		push_error("Failed to load Spine resources: atlas=%s skel=%s" % [atlas_path, json_path])
		return null
	var data: Resource = ClassDB.instantiate("SpineSkeletonDataResource")
	data.set_atlas_res(atlas_res)
	data.set_skeleton_file_res(skel_file)
	data.set_default_mix(mix)
	if not data.is_skeleton_data_loaded():
		push_error("SpineSkeletonDataResource failed to initialize for %s" % json_path)
		return null
	_skeleton_data_cache[cache_key] = data
	return data


static func _connect_blink_overlay(spine_sprite: Node2D, base_anim: String, blink_anim: String) -> void:
	spine_sprite.animation_started.connect(func(_sprite: Object, anim_state: Object, track_entry: Object) -> void:
		if track_entry == null or track_entry.get_track_index() != 0:
			return
		var anim_obj: Object = track_entry.get_animation()
		var started_name: String = anim_obj.get_name() if anim_obj != null else ""
		if started_name == base_anim:
			var current_overlay: Object = anim_state.get_current(1)
			var current_name := ""
			if current_overlay != null and current_overlay.get_animation() != null:
				current_name = current_overlay.get_animation().get_name()
			if current_name != blink_anim:
				var overlay_entry: Object = anim_state.set_animation(blink_anim, true, 1)
				if overlay_entry != null:
					overlay_entry.set_mix_duration(0.0)
		else:
			anim_state.clear_track(1)
	)


static func _connect_aspect_cover(host: Control, spine_sprite: Node2D, target_size: Vector2, offset: Vector2) -> void:
	var fit_callable := func() -> void:
		if not is_instance_valid(host) or not is_instance_valid(spine_sprite):
			return
		var area := host.size
		if area.x <= 0.0 or area.y <= 0.0:
			return
		var k := maxf(area.x / target_size.x, area.y / target_size.y)
		spine_sprite.scale = Vector2.ONE * k
		spine_sprite.position = area * 0.5 + offset * k
	host.resized.connect(fit_callable)
	fit_callable.call()


static func _apply_initial_playback(
	spine_sprite: Node2D,
	init_anim: String,
	init_loop: bool,
	blink_anim: String,
	blink_base: String
) -> void:
	if init_anim == "" or not is_instance_valid(spine_sprite):
		return
	var anim_state: Object = spine_sprite.get_animation_state()
	var skeleton: Object = spine_sprite.get_skeleton()
	if anim_state == null or skeleton == null:
		spine_sprite.ready.connect(func() -> void:
			_apply_initial_playback(spine_sprite, init_anim, init_loop, blink_anim, blink_base),
			CONNECT_ONE_SHOT
		)
		return
	skeleton.set_to_setup_pose()
	var entry: Object = anim_state.set_animation(init_anim, init_loop, 0)
	if entry != null:
		entry.set_mix_duration(0.0)
	if blink_anim != "" and init_anim == blink_base:
		var blink_entry: Object = anim_state.set_animation(blink_anim, true, 1)
		if blink_entry != null:
			blink_entry.set_mix_duration(0.0)
	spine_sprite.update_skeleton(0.0)


func _notification(what: int) -> void:
	if spatlas_path != "" or what != NOTIFICATION_SCENE_INSTANTIATED:
		return
	for child in get_children():
		if child != null and child.has_method("mount_spine_placeholder"):
			child.call("mount_spine_placeholder")


func mount_spine_placeholder() -> void:
	if _mounted or spatlas_path == "" or not ClassDB.class_exists("SpineSprite"):
		return
	_mounted = true
	var self_any: Variant = self
	var self_2d := self_any as Node2D
	if self_2d == null:
		return
	var data := get_or_load_skeleton_data(spatlas_path, spjson_path, default_mix)
	if data == null:
		return

	var spine_sprite := ClassDB.instantiate("SpineSprite") as Node2D
	spine_sprite.set_skeleton_data_res(data)
	spine_sprite.texture_filter = self_2d.texture_filter
	spine_sprite.modulate = self_2d.modulate
	spine_sprite.self_modulate = self_2d.self_modulate
	spine_sprite.visible = self_2d.visible
	spine_sprite.z_index = self_2d.z_index
	spine_sprite.z_as_relative = self_2d.z_as_relative
	spine_sprite.show_behind_parent = self_2d.show_behind_parent

	if blink_animation != "":
		_connect_blink_overlay(spine_sprite, blink_base_animation, blink_animation)

	if mount_mode == "aspect_cover":
		_mount_aspect_cover(spine_sprite)
		return

	var orig_name := String(name)
	var saved_owner := owner
	var was_unique := unique_name_in_owner
	spine_sprite.name = orig_name
	spine_sprite.transform = self_2d.transform

	var children_to_move := get_children()
	for child in children_to_move:
		remove_child(child)
		spine_sprite.add_child(child)

	unique_name_in_owner = false
	replace_by(spine_sprite)
	if saved_owner != null:
		spine_sprite.owner = saved_owner
		spine_sprite.unique_name_in_owner = was_unique
		for child in spine_sprite.get_children():
			child.owner = saved_owner

	_apply_initial_playback(spine_sprite, initial_animation, initial_loop, blink_animation, blink_base_animation)
	queue_free()


func _mount_aspect_cover(spine_sprite: Node2D) -> void:
	var host := get_parent() as Control
	if host == null:
		push_error("aspect_cover mount_mode requires a Control parent.")
		return
	spine_sprite.name = "SpineSprite"

	if attach_auto_player and ResourceLoader.exists("res://src/Core/Nodes/Animation/NSpineAutoPlayer.cs"):
		var auto_script := ResourceLoader.load("res://src/Core/Nodes/Animation/NSpineAutoPlayer.cs") as Script
		if auto_script != null:
			var auto_node := Node.new()
			auto_node.name = "NSpineAutoPlayer"
			auto_node.set_script(auto_script)
			spine_sprite.add_child(auto_node)

	var target_size := design_size
	var offset := content_offset
	var saved_owner := owner
	replace_by(spine_sprite)
	if saved_owner != null:
		spine_sprite.owner = saved_owner
	_connect_aspect_cover(host, spine_sprite, target_size, offset)
	_apply_initial_playback(spine_sprite, initial_animation, initial_loop, blink_animation, blink_base_animation)
	queue_free()
