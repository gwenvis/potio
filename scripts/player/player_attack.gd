class_name PlayerAttack
extends Node2D

@export var attack_sprite: Sprite2D
@export var attack_collision: Area2D
@export var _offset := 12
@export var visible_time := 0.2

var current_visible_time: float
var is_frozen: bool

func _physics_process(delta: float) -> void:
	if not attack_sprite: return
	attack_sprite.visible = current_visible_time > 0
	if current_visible_time > 0:
		current_visible_time = max(0, current_visible_time - delta)

func on_click_handler(click_position: Vector2) -> void:
	if (is_frozen): return
	if current_visible_time > 0: return
	var center := global_position
	var vec := (click_position - center).normalized()
	var rot := atan2(vec.y, vec.x) + deg_to_rad(45)
	attack_sprite.position = vec * _offset	
	attack_sprite.rotation = rot
	current_visible_time = visible_time
	
	var overlapping_bodies := attack_collision.get_overlapping_bodies()
	for body in overlapping_bodies:
		if body is Enemy:
			(body as Enemy).damage(self, 1)

func freeze(is_frozen: bool) -> void:
	self.is_frozen = is_frozen
