class_name PlayerMovement
extends Node2D

@export var max_speed: float = 16*10
@export var acceleration: float = 1200
@export var deacceleration: float = 700

var body: CharacterBody2D

func process(delta: float, body: CharacterBody2D, input: Vector2) -> void:
	self.body = body
	_process_movement(delta, input)
	body.move_and_slide()

func _process_movement(delta: float, input: Vector2) -> void:
	if (is_zero_approx(input.x) && is_zero_approx(input.y)):
		_friction(delta)
		return
	
	var direction := input.normalized() * acceleration
	body.velocity += direction * delta
	body.velocity = body.velocity.limit_length(max_speed)
	
func _friction(delta: float) -> void:
	var friction := deacceleration * delta
	if (body.velocity.length_squared() > friction * friction):
		body.velocity -= body.velocity.normalized() * deacceleration * delta
	else:
		body.velocity = Vector2.ZERO
