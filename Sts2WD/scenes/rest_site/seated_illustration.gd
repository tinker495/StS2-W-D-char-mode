extends Node2D
## All three layers share a top-left origin so mirrored seats keep their light masks registered.
const CAST_SHADOW_ALPHA := 0.50
const LIGHT_OFF_DIM := 0.88

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
var fire_light := true

func _ready() -> void:
	cast_shadow.self_modulate.a = CAST_SHADOW_ALPHA
	play_variant("overgrowth_loop")

func play_variant(clip: String) -> void:
	if not ACT_LIGHTING.has(clip):
		return
	var lighting: Dictionary = ACT_LIGHTING[clip]
	act_tint = lighting.tint
	rim.self_modulate = lighting.rim
	set_light_off(not fire_light)

func set_light_off(disabled: bool) -> void:
	fire_light = not disabled
	painting.self_modulate = act_tint if fire_light else Color(act_tint.r * LIGHT_OFF_DIM, act_tint.g * LIGHT_OFF_DIM, act_tint.b * LIGHT_OFF_DIM)
	rim.visible = fire_light
