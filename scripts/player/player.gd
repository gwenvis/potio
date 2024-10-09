class_name Player
extends CharacterBody2D

@export var player_movement: PlayerMovement = PlayerMovement.new()
@export var player_attack: PlayerAttack
@export var knockback: Knockback
@export var inventory: PlayerInventory

var _input_direction: Vector2

func _physics_process(delta: float) -> void:
	if(knockback and knockback.should_process()): 
		knockback.process(delta, self)
		if (not knockback.should_process()):
			freeze(false)
		return
	player_movement.process(delta, self, _input_direction)
	
func _set_input_direction(input_direction: Vector2) -> void:
	_input_direction = input_direction

func damage(who: Node2D, amount: int) -> void:
	knockback.add(who.global_position.direction_to(global_position))
	freeze(true)

func freeze(is_frozen: bool) -> void:
	player_attack.freeze(is_frozen)
