extends Node2D
## All three layers share a top-left origin so mirrored seats keep their light masks registered.
const CAST_SHADOW_ALPHA := 0.50
const LIGHT_OFF_DIM := 0.88
const FIRE_BASE_LIFT := 0.035
const FIRE_RIM_SWING := 0.28

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

@onready var painting: Sprite2D = $Painting
@onready var cast_shadow: Sprite2D = $CastShadow
@onready var rim: Sprite2D = $RimHighlight

var act_tint: Color = ACT_LIGHTING.overgrowth_loop.tint
var act_rim: Color = ACT_LIGHTING.overgrowth_loop.rim
var fire_light := true

## Shared wall-clock wave keeps co-op seats around the same campfire synchronized without a singleton.
static func sample_fire_wave() -> float:
	var t := Time.get_ticks_msec() * 0.001
	return 0.55 * sin(t * 2.1) + 0.30 * sin(t * 5.3 + 1.1) + 0.15 * sin(t * 9.7 + 0.4)

static func resolve_base_tint(tint: Color, lit: bool, wave: float) -> Color:
	if not lit:
		return Color(tint.r * LIGHT_OFF_DIM, tint.g * LIGHT_OFF_DIM, tint.b * LIGHT_OFF_DIM, 1.0)
	var lift := 1.0 + FIRE_BASE_LIFT * wave
	return Color(clampf(tint.r * lift, 0.0, 1.0), clampf(tint.g * lift, 0.0, 1.0), clampf(tint.b * lift, 0.0, 1.0), 1.0)

static func resolve_rim_color(rim_base: Color, wave: float) -> Color:
	return Color(rim_base.r, rim_base.g, rim_base.b, clampf(rim_base.a * (1.0 + FIRE_RIM_SWING * wave), 0.0, 1.0))

func _ready() -> void:
	cast_shadow.self_modulate.a = CAST_SHADOW_ALPHA
	play_variant("overgrowth_loop")

func _process(_delta: float) -> void:
	if not fire_light:
		return
	_apply_lighting(sample_fire_wave())

func play_variant(clip: String) -> void:
	if not ACT_LIGHTING.has(clip):
		return
	var lighting: Dictionary = ACT_LIGHTING[clip]
	act_tint = lighting.tint
	act_rim = lighting.rim
	set_light_off(not fire_light)

func set_light_off(disabled: bool) -> void:
	fire_light = not disabled
	set_process(fire_light)
	_apply_lighting(sample_fire_wave() if fire_light else 0.0)

func _apply_lighting(wave: float) -> void:
	painting.self_modulate = resolve_base_tint(act_tint, fire_light, wave)
	rim.self_modulate = resolve_rim_color(act_rim, wave)
	rim.visible = fire_light
