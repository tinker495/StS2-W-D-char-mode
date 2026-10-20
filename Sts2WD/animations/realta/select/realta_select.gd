extends Node2D
## Realta's character-select background: the unchanged 1920x1080 painting on a UV-fixed mesh with
## local cape/ribbon/hat-tip motion, a pulse on the existing cyan magic, a few magic motes and blinks.
## The face, both hands, the blade and the canvas border stay fixed. The parent Control is laid out
## full-rect; this node covers it like TextureRect KEEP_ASPECT_COVERED (the previous static scene).

const DURATION := 8.0
const COLUMNS := 128
const ROWS := 72
const BLINKS := [[3.1, 3.19]]
const MOTES := 5
## Source-space cloth interiors. A fixed border around each silhouette keeps triangles
## touching the painted background still; the fine mesh lets the cloth flex inside it.
const RIBBON_UPPER := [Vector2(1005, 632), Vector2(947, 728), Vector2(861, 800), Vector2(817, 830), Vector2(789, 806), Vector2(750, 801), Vector2(830, 749), Vector2(913, 718)]
const RIBBON_LOWER := [Vector2(1016, 645), Vector2(988, 756), Vector2(970, 837), Vector2(914, 944), Vector2(898, 967), Vector2(867, 930), Vector2(804, 915), Vector2(895, 849), Vector2(931, 774)]
const HAT_TIP := [Vector2(1730, 231), Vector2(1787, 263), Vector2(1822, 277), Vector2(1838, 341), Vector2(1885, 419), Vector2(1796, 368), Vector2(1745, 326), Vector2(1712, 275)]
const CAPE_HEM := [Vector2(0, 916), Vector2(165, 883), Vector2(313, 904), Vector2(458, 942), Vector2(628, 968), Vector2(755, 992), Vector2(895, 970), Vector2(1023, 964), Vector2(935, 1065), Vector2(170, 1065), Vector2(0, 1065)]

@export var master: Texture2D
@export var blink_patch: Texture2D
@export var blink_offset := Vector2(1287, 334)

## Keyed control values (source pixels; magic is the glow gain).
@export var cape := 0.0
@export var ribbon := 0.0
@export var hat_tip := 0.0
@export var magic := 0.0
@export var mote_time := 0.0
@export var blinking := false

var setup := PackedVector2Array()
var cloth_weights := PackedVector3Array()
var moving_vertices := PackedInt32Array()
var mesh_art: Polygon2D
var eyes: Polygon2D
var pulse: ShaderMaterial
var motes: Node2D

func _ready() -> void:
	var size := master.get_size()
	for y in ROWS + 1:
		for x in COLUMNS + 1:
			var p := Vector2(x * size.x / COLUMNS, y * size.y / ROWS)
			setup.append(p)
			var edge := smoothstep(0.0, 80.0, p.x) * (1.0 - smoothstep(1840.0, 1920.0, p.x)) * smoothstep(0.0, 60.0, p.y) * (1.0 - smoothstep(1010.0, 1080.0, p.y))
			var weights := edge * Vector3(
				_cloth_weight(p, CAPE_HEM, 18.0, 52.0),
				maxf(_cloth_weight(p, RIBBON_UPPER, 16.0, 30.0), _cloth_weight(p, RIBBON_LOWER, 16.0, 30.0)),
				_cloth_weight(p, HAT_TIP, 18.0, 38.0))
			cloth_weights.append(weights)
			if weights != Vector3.ZERO:
				moving_vertices.append(setup.size() - 1)
	var faces: Array[PackedInt32Array] = []
	var eye_faces: Array[PackedInt32Array] = []
	var eye_rect := Rect2(blink_offset, blink_patch.get_size()).grow(size.x / COLUMNS)
	for y in ROWS:
		for x in COLUMNS:
			var a := y * (COLUMNS + 1) + x
			for tri in [PackedInt32Array([a, a + 1, a + COLUMNS + 2]), PackedInt32Array([a, a + COLUMNS + 2, a + COLUMNS + 1])]:
				faces.append(tri)
				if eye_rect.has_point(setup[tri[0]]) or eye_rect.has_point(setup[tri[1]]) or eye_rect.has_point(setup[tri[2]]):
					eye_faces.append(tri)
	mesh_art = _mesh("Painting", master, faces, Vector2.ZERO)
	var shader := Shader.new()
	shader.code = """shader_type canvas_item;
uniform float glow = 0.0;
void fragment() {
	vec4 source = texture(TEXTURE, UV);
	float cyan = smoothstep(0.10, 0.32, min(source.g - source.r, source.b - source.r));
	float region = smoothstep(0.37, 0.41, UV.x) * (1.0 - smoothstep(0.55, 0.58, UV.x)) * (1.0 - smoothstep(0.32, 0.42, UV.y));
	COLOR = vec4(source.rgb * (1.0 + glow * cyan * region), source.a);
}"""
	pulse = ShaderMaterial.new()
	pulse.shader = shader
	mesh_art.material = pulse
	eyes = _mesh("BlinkPatch", blink_patch, eye_faces, blink_offset)
	eyes.visible = false
	motes = Node2D.new()
	motes.name = "MagicMotes"
	motes.draw.connect(_draw_motes)
	add_child(motes)
	var player := AnimationPlayer.new()
	player.name = "AnimationPlayer"
	var library := AnimationLibrary.new()
	library.add_animation("animation", _loop())
	player.add_animation_library("", library)
	player.autoplay = "animation"
	add_child(player)
	player.play("animation")
	get_parent().resized.connect(_fit)
	_fit()
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
	for property in ["cape", "ribbon", "hat_tip", "magic"]:
		var track := animation.add_track(Animation.TYPE_VALUE)
		animation.track_set_path(track, NodePath(".:" + property))
		animation.track_set_interpolation_type(track, Animation.INTERPOLATION_CUBIC)
		for i in 33:
			var phase := TAU * i / 32.0
			var value := 0.0
			match property:
				"cape": value = sin(phase) * 5.0
				"ribbon": value = sin(phase - 0.65) * 3.5
				"hat_tip": value = (sin(phase - 0.9) + 0.18 * sin(2.0 * phase)) * 4.0
				"magic": value = 0.04 + 0.035 * sin(phase * 2.0 - 0.25)
			animation.track_insert_key(track, DURATION * i / 32.0, value)
	var mote_track := animation.add_track(Animation.TYPE_VALUE)
	animation.track_set_path(mote_track, NodePath(".:mote_time"))
	animation.track_insert_key(mote_track, 0.0, 0.0)
	animation.track_insert_key(mote_track, DURATION, DURATION)
	var blink := animation.add_track(Animation.TYPE_VALUE)
	animation.track_set_path(blink, NodePath(".:blinking"))
	animation.value_track_set_update_mode(blink, Animation.UPDATE_DISCRETE)
	animation.track_insert_key(blink, 0.0, false)
	for b in BLINKS:
		animation.track_insert_key(blink, b[0], true)
		animation.track_insert_key(blink, b[1], false)
	return animation

func _fit() -> void:
	var area: Vector2 = get_parent().size
	var art := master.get_size()
	var k := maxf(area.x / art.x, area.y / art.y)
	scale = Vector2.ONE * k
	position = (area - art * k) * 0.5

func _process(_delta: float) -> void:
	apply_pose()

func _cloth_weight(p: Vector2, outline: PackedVector2Array, inset: float, falloff: float) -> float:
	if not Geometry2D.is_point_in_polygon(p, outline):
		return 0.0
	var distance := INF
	for i in outline.size():
		distance = minf(distance, p.distance_to(Geometry2D.get_closest_point_to_segment(p, outline[i], outline[(i + 1) % outline.size()])))
	return smoothstep(inset, falloff, distance)

func apply_pose() -> void:
	# Only the precomputed cloth interiors need per-frame work; the background is copied verbatim.
	var vertices := setup.duplicate()
	for i in moving_vertices:
		var p := setup[i]
		var c := cloth_weights[i].x
		var r := cloth_weights[i].y
		var h := cloth_weights[i].z
		vertices[i] = p + Vector2(c * cape * 0.3 - r * ribbon * 0.4 + h * hat_tip * 0.2, c * cape + r * ribbon + h * hat_tip)
	mesh_art.polygon = vertices
	eyes.polygon = vertices
	eyes.visible = blinking
	pulse.set_shader_parameter("glow", magic)
	motes.queue_redraw()

func _draw_motes() -> void:
	# Small deterministic motes rising from the existing cyan magic, tied to the player's timeline.
	for i in MOTES:
		var age := fposmod(mote_time / 4.0 + float(i) / MOTES, 1.0)
		var opacity := sin(PI * age) * 0.34
		var point := Vector2(924.0 + 39.0 * sin(age * TAU + i * 0.8), 310.0 - 250.0 * age)
		motes.draw_circle(point, 6.4, Color(0.3, 0.88, 1.0, opacity * 0.13), true, -1, true)
		motes.draw_circle(point, 2.6, Color(0.72, 0.98, 1.0, opacity), true, -1, true)
