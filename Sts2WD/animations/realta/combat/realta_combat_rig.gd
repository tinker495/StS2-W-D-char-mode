extends Node2D
## Receives the authored AnimationPlayer cues and keeps per-character loop phase across BaseLib restarts.

signal animation_cue(cue: StringName)

const LOOPS: Array[StringName] = [&"idle", &"idle_loop", &"relaxed", &"relaxed_loop"]

@onready var animator: AnimationPlayer = $AnimationPlayer
var _previous: StringName
var _last_phase := 0.0
var _initial_loop_played := false
var _start_count := 0
var _blend_until := 0.0
var _blend_tracks: Array[int] = []

func _ready() -> void:
	animator.animation_started.connect(_on_animation_started)
	animator.mixer_applied.connect(_after_mixing)
	if animator.is_playing():
		_on_animation_started(animator.current_animation)

func notify_cue(cue: StringName) -> void:
	animation_cue.emit(cue)

func _on_animation_started(clip: StringName) -> void:
	_start_count += 1
	_blend_until = animator.get_blend_time(_previous, clip) if _previous != &"" else 0.0
	_blend_tracks.clear()
	var motion := animator.get_animation(clip)
	if _blend_until > 0.0:
		for track in motion.get_track_count():
			if motion.track_get_type(track) == Animation.TYPE_VALUE and motion.track_get_interpolation_type(track) == Animation.INTERPOLATION_NEAREST:
				_blend_tracks.append(track)
	if clip in LOOPS:
		if not _initial_loop_played:
			_initial_loop_played = true
			_restore_phase.call_deferred(clip, randf() * motion.length, _start_count)
		elif _previous in LOOPS and animator.get_animation(_previous) == motion:
			_restore_phase.call_deferred(clip, _last_phase, _start_count)
		else:
			_last_phase = 0.0
	_previous = clip

func _restore_phase(clip: StringName, phase: float, serial: int) -> void:
	if serial == _start_count and animator.is_playing() and animator.current_animation == clip:
		animator.seek(phase, true)

func _after_mixing() -> void:
	if not animator.is_playing():
		return
	var clip := animator.current_animation
	var time := animator.current_animation_position
	# AnimationPlayer.stop() applies time zero while is_playing() still reports true.
	# Keep the last displayed phase until the incoming loop is known.
	if clip in LOOPS and time > 0.0:
		_last_phase = time
	if _blend_tracks.is_empty():
		return
	if time > _blend_until:
		_blend_tracks.clear()
		return
	# Smooth the live bones across stances. Attachments and draw slots follow the incoming pose
	# as soon as it starts, so colors/visibility cannot flicker under a weighted Boolean blend.
	var motion := animator.get_animation(clip)
	for track in _blend_tracks:
		var value = motion.value_track_interpolate(track, time)
		var path := motion.track_get_path(track)
		var target := get_node(NodePath(path.get_concatenated_names()))
		target.set_indexed(NodePath(path.get_concatenated_subnames()), value)
