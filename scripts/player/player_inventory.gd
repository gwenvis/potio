class_name PlayerInventory
extends Node

var capacity := 30

var _items: Array[Item] = []

func _ready() -> void:
	_items.resize(capacity)

func is_inventory_full() -> bool:
	return _items.size() >= capacity;

func add_item(item: Item) -> void:
	_items.push_back(item)
	print('added item ', item.name)
