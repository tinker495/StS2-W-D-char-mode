extends Node2D
## The game only dispatches act clips and light_off to direct SpineSprite children, so
## Sts2WDCode/Characters/Realta/RealtaSceneBridge.cs calls play_variant()/set_light_off().
## Matches vanilla StS2 four-layer rest-site lighting: neutral base + contact shadow,
## Act ambient tint, persistent back-side cast shadow, and fire-toggled Act rim highlight.

const DURATION := 8.0
const GRID_X := 24
const GRID_Y := 36
const HEAD_PIVOT := Vector2(262.0, 290.0)
const HAT_PIVOT := Vector2(210.0, 125.0)
const CAST_SHADOW_ALPHA := 0.50
const LIGHT_OFF_DIM := 0.88
const BLINKS := [[2.6, 2.71], [6.2, 6.30]]

const ACT_LIGHTING := {
	"overgrowth_loop": {
		"tint": Color(0.89, 1.00, 0.84),
		"rim": Color(0.92, 1.00, 0.32, 0.42),
	},
	"underdocks_loop": {
		"tint": Color(0.80, 0.90, 1.00),
		"rim": Color(0.24, 0.91, 0.88, 0.40),
	},
	"hive_loop": {
		"tint": Color(1.00, 0.96, 0.79),
		"rim": Color(1.00, 0.84, 0.17, 0.44),
	},
	"glory_loop": {
		"tint": Color(0.82, 0.91, 1.00),
		"rim": Color(0.00, 0.87, 0.96, 0.42),
	},
}

@export var master: Texture2D
@export var blink_patch: Texture2D
@export var cast_shadow: Texture2D
@export var rim_highlight: Texture2D
## Top-left of the blink patch in master pixels (see docs/characters/realta/animation/blink-patches.json).
@export var blink_offset := Vector2(238, 207)

## Keyed control values (source pixels / radians).
@export var chest := 0.0
@export var head_nod := 0.0
@export var hat_tip := 0.0
@export var coat_hem := 0.0
@export var blinking := false

var fire_light := true
var act_tint: Color = ACT_LIGHTING.overgrowth_loop.tint
var act_rim: Color = ACT_LIGHTING.overgrowth_loop.rim
var setup := PackedVector2Array()
var mesh_art: Polygon2D
var eyes: Polygon2D
var cast_layer: Polygon2D
var rim_layer: Polygon2D
var player: AnimationPlayer

func _ready() -> void:
	var size := master.get_size()
	var cols := GRID_X + 1
	for y in GRID_Y + 1:
		for x in cols:
			setup.append(Vector2(x * size.x / GRID_X, y * size.y / GRID_Y))
	var faces: Array[PackedInt32Array] = []
	var eye_faces: Array[PackedInt32Array] = []
	var eye_margin := maxf(size.x / GRID_X, size.y / GRID_Y)
	var eye_rect := Rect2(blink_offset, blink_patch.get_size()).grow(eye_margin)
	for y in GRID_Y:
		for x in GRID_X:
			var a := y * cols + x
			for tri in [PackedInt32Array([a, a + 1, a + cols + 1]), PackedInt32Array([a, a + cols + 1, a + cols])]:
				faces.append(tri)
				if eye_rect.has_point(setup[tri[0]]) or eye_rect.has_point(setup[tri[1]]) or eye_rect.has_point(setup[tri[2]]):
					eye_faces.append(tri)
	mesh_art = _mesh("Painting", master, faces, Vector2.ZERO)
	eyes = _mesh("BlinkPatch", blink_patch, eye_faces, blink_offset)
	eyes.visible = false
	if cast_shadow != null:
		cast_layer = _mesh("CastShadow", cast_shadow, faces, Vector2.ZERO)
	if rim_highlight != null:
		rim_layer = _mesh("RimHighlight", rim_highlight, faces, Vector2.ZERO)
	player = AnimationPlayer.new()
	player.name = "AnimationPlayer"
	add_child(player)
	var library := AnimationLibrary.new()
	for clip in ACT_LIGHTING:
		library.add_animation(clip, _loop())
	player.add_animation_library("", library)
	play_variant("overgrowth_loop")
	apply_pose()

func _mesh(node_name: String, texture: Texture2D, faces: Array[PackedInt32Array], uv_offset: Vector2) -> Polygon2D:
	var poly := Polygon2D.new()
	poly.name = node_name
	poly.texture = texture
	poly.texture_repeat = CanvasItem.TEXTURE_REPEAT_DISABLED
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
	for property in ["chest", "head_nod", "hat_tip", "coat_hem"]:
		var track := animation.add_track(Animation.TYPE_VALUE)
		animation.track_set_path(track, NodePath(".:" + property))
		animation.track_set_interpolation_type(track, Animation.INTERPOLATION_CUBIC)
		for i in 17:
			var phase := TAU * i / 16.0
			var value := 0.0
			match property:
				"chest": value = sin(phase) * 2.4
				"head_nod": value = sin(phase - 0.25) * 0.008
				"hat_tip": value = (sin(phase - 0.65) + 0.18 * sin(phase * 2.0 - 0.3)) * 0.015
				"coat_hem": value = sin(phase - 0.90) * 1.4
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
	if not ACT_LIGHTING.has(clip):
		return
	var lighting: Dictionary = ACT_LIGHTING[clip]
	act_tint = lighting.tint
	act_rim = lighting.rim
	player.play(clip)
	player.seek(randf() * DURATION, true)
	apply_pose()

## Mirrors the official `_tracks/light_off` track: removes the campfire rim glow while keeping
## Act ambient tint, persistent back-side cast shadow, and seated breathing motion.
func set_light_off(disabled: bool) -> void:
	fire_light = not disabled
	apply_pose()

func _process(_delta: float) -> void:
	apply_pose()

func apply_pose() -> void:
	var vertices := PackedVector2Array()
	vertices.resize(setup.size())
	for i in setup.size():
		var p := setup[i]
		var q := p
		# Upper torso breath fades smoothly to zero by waist/wrists (y=445),
		# keeping hips, boots, and the log-bracing left hand strictly planted.
		var torso_weight := 1.0 - smoothstep(320.0, 445.0, p.y)
		q.y -= chest * torso_weight
		# Rigid head/hat nod over the fur collar so eyes and blink patch never shear.
		var head_weight := 1.0 - smoothstep(260.0, 325.0, p.y)
		q += ((p - HEAD_PIVOT).rotated(head_nod) - (p - HEAD_PIVOT)) * head_weight
		# Secondary follow-through on the curled left hat tip.
		var hat_weight := (1.0 - smoothstep(145.0, 215.0, p.x)) * (1.0 - smoothstep(135.0, 190.0, p.y))
		q += ((p - HAT_PIVOT).rotated(hat_tip) - (p - HAT_PIVOT)) * hat_weight
		# Gentle breeze on the free-hanging lower-left coat hem in front of the log
		# without disturbing the left-hand contact shadow or boots.
		var hem_x := smoothstep(145.0, 175.0, p.x) * (1.0 - smoothstep(225.0, 260.0, p.x))
		var hem_y := smoothstep(590.0, 650.0, p.y) * (1.0 - smoothstep(730.0, 765.0, p.y))
		q.x += coat_hem * hem_x * hem_y
		vertices[i] = q

	var base_color := act_tint if fire_light else Color(
		act_tint.r * LIGHT_OFF_DIM,
		act_tint.g * LIGHT_OFF_DIM,
		act_tint.b * LIGHT_OFF_DIM,
		1.0
	)
	mesh_art.polygon = vertices
	mesh_art.color = base_color
	eyes.polygon = vertices
	eyes.color = base_color
	eyes.visible = blinking
	if cast_layer != null:
		cast_layer.polygon = vertices
		cast_layer.color = Color(1.0, 1.0, 1.0, CAST_SHADOW_ALPHA)
	if rim_layer != null:
		rim_layer.polygon = vertices
		rim_layer.color = act_rim
		rim_layer.visible = fire_light
