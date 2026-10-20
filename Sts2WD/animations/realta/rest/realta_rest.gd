extends Node2D
## Realta's seated rest-site animation: one unchanged painting on a UV-fixed mesh, deformed by five
## controls that a single AnimationPlayer keys (breathing, head + cheek-supporting hand, hat tip,
## both coat ends). The three Act loops share the motion and differ in ambient light, as observed in
## the official rest scenes. A small eye patch rides the same vertices for blinks.
##
## The game only dispatches act clips and light_off to direct SpineSprite children, so
## Sts2WDCode/Characters/Realta/RealtaSceneBridge.cs calls play_variant()/set_light_off().

const DURATION := 8.0
const GRID := 25
const ANCHOR := Vector2(795, 420)
const ACT_TINT := {
	"overgrowth_loop": Color(0.88, 1.0, 0.91),
	"hive_loop": Color(1.0, 0.94, 0.83),
	"glory_loop": Color(0.83, 0.96, 1.0),
}
## Two quick blinks per 8 s loop (open -> closed -> open), shared by every Act.
const BLINKS := [[2.6, 2.69], [6.2, 6.28]]

@export var master: Texture2D
@export var blink_patch: Texture2D
## Top-left of the blink patch in master pixels (see docs/characters/realta/animation/blink-patches.json).
@export var blink_offset := Vector2(718, 276)

## Keyed control values (source pixels / radians).
@export var chest := 0.0
@export var head_contact := 0.0
@export var hat_tip := 0.0
@export var coat_left := 0.0
@export var coat_right := 0.0
@export var blinking := false

var fire_light := true
var act_tint: Color = ACT_TINT.overgrowth_loop
var setup := PackedVector2Array()
var mesh_art: Polygon2D
var eyes: Polygon2D
var player: AnimationPlayer

func _ready() -> void:
	var size := master.get_size()
	for y in GRID + 1:
		for x in GRID + 1:
			setup.append(Vector2(x * size.x / GRID, y * size.y / GRID))
	var faces: Array[PackedInt32Array] = []
	var eye_faces: Array[PackedInt32Array] = []
	var eye_rect := Rect2(blink_offset, blink_patch.get_size()).grow(size.x / GRID)
	for y in GRID:
		for x in GRID:
			var a := y * (GRID + 1) + x
			for tri in [PackedInt32Array([a, a + 1, a + GRID + 2]), PackedInt32Array([a, a + GRID + 2, a + GRID + 1])]:
				faces.append(tri)
				if eye_rect.has_point(setup[tri[0]]) or eye_rect.has_point(setup[tri[1]]) or eye_rect.has_point(setup[tri[2]]):
					eye_faces.append(tri)
	mesh_art = _mesh("Painting", master, faces, Vector2.ZERO)
	eyes = _mesh("BlinkPatch", blink_patch, eye_faces, blink_offset)
	eyes.visible = false
	player = AnimationPlayer.new()
	player.name = "AnimationPlayer"
	add_child(player)
	var library := AnimationLibrary.new()
	for clip in ACT_TINT:
		library.add_animation(clip, _loop())
	player.add_animation_library("", library)
	play_variant("overgrowth_loop")
	apply_pose()

func _mesh(node_name: String, texture: Texture2D, faces: Array[PackedInt32Array], uv_offset: Vector2) -> Polygon2D:
	var poly := Polygon2D.new()
	poly.name = node_name
	poly.texture = texture
	poly.polygon = setup
	var uv := PackedVector2Array()
	for p in setup:
		uv.append(p - uv_offset)
	poly.uv = uv
	poly.polygons = faces
	add_child(poly)
	return poly

func _loop() -> Animation:
	var animation := Animation.new()
	animation.length = DURATION
	animation.loop_mode = Animation.LOOP_LINEAR
	for property in ["chest", "head_contact", "hat_tip", "coat_left", "coat_right"]:
		var track := animation.add_track(Animation.TYPE_VALUE)
		animation.track_set_path(track, NodePath(".:" + property))
		animation.track_set_interpolation_type(track, Animation.INTERPOLATION_CUBIC)
		for i in 17:
			var phase := TAU * i / 16.0
			var value := 0.0
			match property:
				"chest": value = sin(phase) * 4.0
				"head_contact": value = sin(phase - 0.22) * 0.009
				"hat_tip": value = (sin(phase - 0.65) + 0.18 * sin(phase * 2 - 0.3)) * 0.014
				"coat_left": value = sin(phase - 0.85) * 4.0
				"coat_right": value = sin(phase - 1.15) * 3.0
			animation.track_insert_key(track, DURATION * i / 16.0, value)
	var blink := animation.add_track(Animation.TYPE_VALUE)
	animation.track_set_path(blink, NodePath(".:blinking"))
	animation.value_track_set_update_mode(blink, Animation.UPDATE_DISCRETE)
	animation.track_insert_key(blink, 0.0, false)
	for b in BLINKS:
		animation.track_insert_key(blink, b[0], true)
		animation.track_insert_key(blink, b[1], false)
	return animation

## Act clip from the rest-site bridge; starts at a random phase like the official rest sprites.
func play_variant(clip: String) -> void:
	if not ACT_TINT.has(clip):
		return
	act_tint = ACT_TINT[clip]
	player.play(clip)
	player.seek(randf() * DURATION, true)

## Mirrors the official `_tracks/light_off` track: removes the campfire warmth, keeps act light and motion.
func set_light_off(disabled: bool) -> void:
	fire_light = not disabled
	apply_pose()

func _process(_delta: float) -> void:
	apply_pose()

func apply_pose() -> void:
	var vertices := PackedVector2Array()
	var colors := PackedColorArray()
	for p in setup:
		var q := p
		q.y -= chest * (1.0 - smoothstep(550.0, 740.0, p.y))
		# The face and the cheek-supporting hand share one continuous contact control.
		var head_weight := 1.0 - smoothstep(430.0, 590.0, p.y)
		q += ((p - ANCHOR).rotated(head_contact) - (p - ANCHOR)) * head_weight
		var hat_weight := (1.0 - smoothstep(530.0, 680.0, p.x)) * (1.0 - smoothstep(240.0, 335.0, p.y))
		q += ((p - Vector2(610, 150)).rotated(hat_tip) - (p - Vector2(610, 150))) * hat_weight
		# The trailing hem moves below the stone; the seat and boot contacts remain fixed.
		var left_weight := (1.0 - smoothstep(350.0, 430.0, p.x)) * smoothstep(1010.0, 1060.0, p.y) * (1.0 - smoothstep(1100.0, 1170.0, p.y))
		var right_hem_edge := lerpf(985.0, 1140.0, smoothstep(900.0, 1040.0, p.y))
		var right_weight := smoothstep(right_hem_edge, right_hem_edge + 110.0, p.x) * smoothstep(740.0, 900.0, p.y) * (1.0 - smoothstep(1040.0, 1140.0, p.y))
		q.x += coat_left * left_weight + coat_right * right_weight
		vertices.append(q)
		# Fire is on the face/glove side. Broad overlapping lobes carry its soft bounce
		# onto the cheek and chest, tapering toward the back and shaded hat crown.
		var face_light := exp(-0.5 * pow((p.x - 840.0) / 160.0, 2.0) - 0.5 * pow((p.y - 355.0) / 145.0, 2.0))
		var upper_light := exp(-0.5 * pow((p.x - 835.0) / 310.0, 2.0) - 0.5 * pow((p.y - 610.0) / 230.0, 2.0))
		var warmth: float = (0.10 * smoothstep(350.0, 1100.0, p.y) + 0.16 * face_light + 0.08 * upper_light) if fire_light else 0.0
		colors.append(act_tint.lerp(Color(1.0, 0.85, 0.70), warmth))
	mesh_art.polygon = vertices
	mesh_art.vertex_colors = colors
	eyes.polygon = vertices
	eyes.vertex_colors = colors
	eyes.visible = blinking
