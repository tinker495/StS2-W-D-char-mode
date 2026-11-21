extends Node2D
## Apply the authored slot order and grouped fade opacity within this character's place in the game canvas.

@export_storage var slot_names: PackedStringArray
@export_storage var orders: Array[PackedInt32Array]
var _slots: Array[Node] = []
var _applied := -1
var _grouped := false
var draw_order: int = 0:
	set(value):
		draw_order = value
		if not _slots.is_empty():
			_apply_order()
var group_alpha: float = 1.0:
	set(value):
		var clamped := clampf(value, 0.0, 1.0)
		if clamped <= 0.0001:
			clamped = 0.0
		elif clamped >= 0.9999:
			clamped = 1.0
		if clamped == group_alpha:
			return
		group_alpha = clamped
		_apply_group_alpha()

func _ready() -> void:
	for slot_name in slot_names:
		_slots.append(get_node(NodePath(slot_name)))
	_apply_order()
	_apply_group_alpha()

func _apply_order() -> void:
	if draw_order == _applied:
		return
	_applied = draw_order
	var order := orders[draw_order]
	# The Skeleton2D is the first child and stays in place. Slot children all keep Z = 0.
	for i in order.size():
		var slot := _slots[order[i]]
		if slot.get_index() != i + 1:
			move_child(slot, i + 1)

func _apply_group_alpha() -> void:
	var rid := get_canvas_item()
	if group_alpha <= 0.0:
		if _grouped:
			RenderingServer.canvas_item_set_canvas_group_mode(rid, RenderingServer.CANVAS_GROUP_MODE_DISABLED)
			_grouped = false
			queue_redraw()
		modulate = Color(1.0, 1.0, 1.0, 0.0)
		self_modulate = Color(1.0, 1.0, 1.0, 1.0)
	elif group_alpha >= 1.0:
		if _grouped:
			RenderingServer.canvas_item_set_canvas_group_mode(rid, RenderingServer.CANVAS_GROUP_MODE_DISABLED)
			_grouped = false
			queue_redraw()
		modulate = Color(1.0, 1.0, 1.0, 1.0)
		self_modulate = Color(1.0, 1.0, 1.0, 1.0)
	else:
		if not _grouped:
			RenderingServer.canvas_item_set_canvas_group_mode(
				rid, RenderingServer.CANVAS_GROUP_MODE_TRANSPARENT, 10.0, true, 10.0, false
			)
			_grouped = true
		modulate = Color(1.0, 1.0, 1.0, 1.0)
		self_modulate = Color(1.0, 1.0, 1.0, group_alpha)
