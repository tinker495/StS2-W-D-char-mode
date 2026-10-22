@tool
extends Node2D
## Native Godot combat/merchant rig, edited directly in the saved .tscn.
## Shared presentation runs in the editor; loop lifecycle is runtime-only.
## Every canvas item retains zero Z and uses sibling tree order.

const LOOPS := ["idle_loop", "idle", "relaxed_loop", "relaxed"]

## Incoming clips hold the outgoing slot until their authored clear-arm frame.
## A combat entry retains the front slot; a shop entry retains the back slot.
@export var free_arm_depth_hold := false
## Shop arm (sleeve, cuff, vambrace and glove) sits behind the entire body.
## Both polygons retain their original skin/bone paths when changing draw slots.
@export var free_arm_back := false
## The weapon sleeve crosses in front of the chest during attack, returning behind it
## outside that interval. Move the existing mesh; retain z_index=0 and its rigid cap weights.
@export var weapon_arm_front := false:
	set(value):
		weapon_arm_front = value
		if is_node_ready():
			_apply_weapon_arm_depth()
## Only a held shaft may be covered by the gripping glove. Release puts that glove
## below the prop as well; revive restores the exception while the rig is hidden.
@export var weapon_gripped := true:
	set(value):
		weapon_gripped = value
		if is_node_ready():
			_apply_grip_depth()
## Keyed by die/revive: 0 = articulated body, 3 = final lying cel, 4 = full-body collapse sequence.
@export var lower_mode := 0:
	set(value):
		lower_mode = value
		if is_node_ready():
			_apply_lower_mode()

@onready var animator: AnimationPlayer = $AnimationPlayer
@onready var final_cel: Sprite2D = %DeathFull

var _last_started := ""
var _phase_initialized := false
var _last_loop_position := 0.0
var _start_serial := 0

func _ready() -> void:
	_apply_presentation()
	animator.mixer_applied.connect(_sync_discrete_state)
	if Engine.is_editor_hint():
		return
	animator.animation_started.connect(_on_animation_started)
	if animator.is_playing() and String(animator.current_animation) in LOOPS:
		_on_animation_started(animator.current_animation)

func _sync_discrete_state() -> void:
	if Engine.is_editor_hint():
		_apply_presentation()
		return
	if animator.current_animation.is_empty():
		return
	# An outgoing discrete time-zero key can overwrite an incoming key during a
	# native blend. Resolve discrete state from the CURRENT clip after mixing, so
	# slots, expressions and release/reset cannot retain the outgoing loop's values.
	# Continuous bones/prop tracks still use the live blend; aliases share resources.
	var clip := animator.get_animation(animator.current_animation)
	for track in clip.get_track_count():
		if clip.track_get_type(track) != Animation.TYPE_VALUE or clip.value_track_get_update_mode(track) != Animation.UPDATE_DISCRETE:
			continue
		var value = clip.track_get_key_value(track, 0)
		for key in clip.track_get_key_count(track):
			# Animation key times are float32; tolerate their sub-microsecond storage
			# rounding at exact display-frame boundaries (1/30, 11/60, .2, .25).
			if clip.track_get_key_time(track, key) > animator.current_animation_position + .000001:
				break
			value = clip.track_get_key_value(track, key)
		var path := clip.track_get_path(track)
		var target := get_node(NodePath(path.get_concatenated_names()))
		var property := NodePath(path.get_concatenated_subnames())
		if str(property) in ["polygon", "uv", "polygons", "bones", "texture"] and target.get_indexed(property) == value:
			continue
		target.set_indexed(property, value)
	_apply_presentation()

func _apply_presentation() -> void:
	# These semantic state tracks are the only owners of composition. Static
	# mesh/UV/weight data lives on the Polygon2D nodes, not in animation copies.
	_apply_lower_mode()
	_apply_free_arm_depth()
	_apply_weapon_arm_depth()
	_apply_grip_depth()

func _apply_free_arm_depth() -> void:
	if free_arm_depth_hold and not Engine.is_editor_hint():
		return
	# Use stable anchors: the weapon sleeve itself has an animated draw slot.
	var anchor := get_node("coat" if free_arm_back else "coat-front")
	var upper := get_node("upper_arm_r")
	var forearm := get_node("forearm_r")
	var upper_index := anchor.get_index() + (-2 if free_arm_back else 1)
	if upper.get_index() == upper_index and forearm.get_index() == upper_index + 1:
		return
	if not free_arm_back:
		for part in ["forearm_r", "upper_arm_r"]:
			var mesh := get_node(part)
			var target := anchor.get_index()
			if mesh.get_index() < target:
				target -= 1
			move_child(mesh, target + 1)
		return
	for part in ["upper_arm_r", "forearm_r"]:
		var mesh := get_node(part)
		var target := anchor.get_index()
		if mesh.get_index() < target:
			target -= 1
		move_child(mesh, target)

func _apply_weapon_arm_depth() -> void:
	var sleeve := get_node("upper_arm_l")
	var anchor := get_node("torso" if weapon_arm_front else "neck-overlap")
	var target := anchor.get_index()
	if sleeve.get_index() < target:
		target -= 1
	if weapon_arm_front:
		target += 1
	if sleeve.get_index() != target:
		move_child(sleeve, target)

func _apply_grip_depth() -> void:
	var hand := get_node("hand_l")
	var target := $Skeleton2D.get_index()
	if hand.get_index() < target:
		target -= 1
	if weapon_gripped:
		target += 1
	if hand.get_index() != target:
		move_child(hand, target)

func _on_animation_started(clip: StringName) -> void:
	_start_serial += 1
	if String(clip) in LOOPS:
		if not _phase_initialized:
			_phase_initialized = true
			_seek_loop.call_deferred(clip, randf() * animator.get_animation(clip).length, _start_serial)
		elif _last_started in LOOPS and animator.get_animation(_last_started) == animator.get_animation(clip):
			# BaseLib restarts with Stop(); Play(). Stop clears the player's position, so retain
			# the last rendered phase ourselves. Resource identity also covers native aliases.
			_seek_loop.call_deferred(clip, _last_loop_position, _start_serial)
	_last_started = clip

func _process(_delta: float) -> void:
	if Engine.is_editor_hint():
		_apply_presentation()
		return
	if animator.is_playing() and String(animator.current_animation) in LOOPS:
		_last_loop_position = animator.current_animation_position

func _seek_loop(clip: StringName, position: float, serial: int) -> void:
	if serial == _start_serial and animator.is_playing() and animator.current_animation == clip:
		animator.seek(position, true)
		_last_loop_position = position

func _apply_lower_mode() -> void:
	for child in get_children():
		if child is Polygon2D:
			child.visible = lower_mode == 0
	final_cel.visible = lower_mode in [3,4]
