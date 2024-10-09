class_name Knockback
extends Node2D

@export var knockback_strength := 200
@export var friction := 500

var _knockback: Vector2

func add(direction: Vector2) -> void:
	_knockback += direction * knockback_strength

func process(delta: float, body: CharacterBody2D) -> void:
	var friction := self.friction * delta
	if (_knockback.length_squared() > friction * friction):
		_knockback -= _knockback.normalized() * friction
	else:
		_knockback = Vector2.ZERO
	body.move_and_collide(_knockback * delta, false, body.safe_margin)
	
func should_process() -> bool:
	return not _knockback.is_zero_approx();
