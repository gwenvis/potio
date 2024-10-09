class_name EnemyManager
extends Node

@export var player: Player

func _ready() -> void:
	call_deferred("_yuppers")
	
func _yuppers() -> void:
	for enemy in _find_enemies():
		enemy.notify_player(player)

func _find_enemies() -> Array[Enemy]:
	var enemies: Array[Enemy] = []
	for node in get_parent().get_children():
		_find_enemies_recursive(node, enemies)
	return enemies
	
func _find_enemies_recursive(node: Node, array: Array[Enemy]) -> void:
	if node is Enemy:
		array.push_back(node as Enemy)
		return
	for child in node.get_children():
		_find_enemies_recursive(child, array)


func _on_tile_map_layer_changed() -> void:
	print("hello!")
