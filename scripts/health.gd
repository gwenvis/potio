class_name Health
extends Node

signal health_changed(amount: int)
signal health_depleted

@export var starting_health := 3

var health: int

func _ready() -> void:
	health = starting_health 

func damage(amount: int) -> void:
	health = max(0, health - amount)
	health_changed.emit(health)
	if health == 0:
		health_depleted.emit()
