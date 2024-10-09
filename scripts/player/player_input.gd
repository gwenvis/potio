class_name PlayerInput
extends Node2D

signal movement_input(direction: Vector2)
signal interact_pressed
signal left_mouse_pressed(position: Vector2)

var _input: Vector2

func _unhandled_input(event: InputEvent) -> void:
	_handle_move_input(event as InputEventKey)
	_handle_mouse(event as InputEventMouseButton)
	_handle_interact_input(event as InputEventKey)
	
func _handle_interact_input(event: InputEventKey) -> void:
	if (not event or not event.pressed or event.echo): 
		return
	if (event.keycode == KEY_E):
		interact_pressed.emit()

func _handle_mouse(event: InputEventMouseButton) -> void:
	if (not event or not event.pressed): return
	if (event.button_index == MOUSE_BUTTON_LEFT):
		var pos := to_global(event.global_position)
		left_mouse_pressed.emit(get_global_mouse_position())
	
func _handle_move_input(keyEvent: InputEventKey) -> void:
	if (not keyEvent or keyEvent.echo): return
	if (keyEvent.pressed): _wahoo(keyEvent, 1)
	else: _wahoo(keyEvent, -1)
	
func _wahoo(event: InputEventKey, add: float) -> void:
	if (event.keycode == KEY_A): _input.x -= add
	if (event.keycode == KEY_D): _input.x += add
	if (event.keycode == KEY_W): _input.y -= add
	if (event.keycode == KEY_S): _input.y += add
	movement_input.emit(_input)
