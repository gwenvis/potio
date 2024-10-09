class_name GridInventoryUI
extends GridContainer

@export var cols := 5
@export var grid_element_size := 32
@export var grid_element_scene: PackedScene
@export var player_ref: Player

var inventory: PlayerInventory

func _ready() -> void:
	inventory = player_ref.inventory
	var rows := inventory.capacity / cols
	print('rows: ', rows, '. cols: ', cols, ' capacity: ', inventory.capacity)
	(get_parent() as Control).size = Vector2(cols * grid_element_size, rows * grid_element_size)
	columns = cols
	for col in cols:
		for row in rows:
			var grid_element := grid_element_scene.instantiate() as Control
			add_child(grid_element)
			
	
