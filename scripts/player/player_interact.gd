class_name PlayerInteract
extends Area2D

@export var input: PlayerInput
@export var inventory: PlayerInventory

var is_frozen: bool

var current_node: Node2D

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	area_entered.connect(_on_body_entered)
	area_exited.connect(_on_body_exited)
	input.interact_pressed.connect(_interact_pressed)

func freeze(is_frozen: bool) -> void:
	self.is_frozen = is_frozen
	
func _interact_pressed() -> void:
	if (is_frozen or not current_node): return
	if (current_node is Harvest):
		if (inventory.is_inventory_full()):
			print("inventory full")
			return
		inventory.add_item((current_node as Harvest).item.duplicate(true) as Item)
		current_node = null
	
func _on_body_entered(body: Node2D) -> void:
	var body_root := body.owner if body.owner else body
	if (not body_root is Harvest): return
	print('got ', body_root.name)
	current_node = body_root

func _on_body_exited(body: Node2D) -> void:
	if (current_node == body):
		current_node = null
	
