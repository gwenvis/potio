class_name Enemy
extends CharacterBody2D

@export var health: Health
@export var movement: PlayerMovement = PlayerMovement.new()
@export var knockback: Knockback
@export var enable_debug := true
@export var nav_agent: NavigationAgent2D

const KnockbackForce := 200
const KNOCKBACK_FRICTION := 500
const MIN_PLAYER_DISTANCE := 320
const DAMAGE_TIMEOUT := 0.75

var player: Player
var is_following_player: bool
var input: Vector2
var damage_timeout: float
var alive := true

func damage(who: Node2D, amount: int) -> void:
	if (health): 
		health.damage(amount)
	if (knockback): knockback.add(who.global_position.direction_to(global_position))

func _process(delta: float) -> void:
	if (enable_debug and alive): queue_redraw()
	
func _physics_process(delta: float) -> void:
	if (knockback and knockback.should_process()):
		knockback.process(delta, self)
		return
	if (not alive): return
	if (damage_timeout > 0):
		damage_timeout = max(damage_timeout - delta, 0)
		movement.process(delta, self, input)
		return
		
	input = Vector2.ZERO
	if not is_following_player: _handle_player_detection(delta)
	else: _handle_player_follow(delta)
	movement.process(delta, self, input)

func _handle_player_follow(delta: float) -> void:
	if nav_agent.target_position.distance_squared_to(player.global_position) > 16*16:
		nav_agent.target_position = player.global_position
	next_pos = nav_agent.get_next_path_position()
	input = global_position.direction_to(next_pos)

func _handle_player_detection(delta: float) -> void:
	if not player: 
		return
	var distance := global_position.distance_squared_to(player.global_position)
	if distance < MIN_PLAYER_DISTANCE * MIN_PLAYER_DISTANCE:
		is_following_player = true

func _on_health_health_depleted() -> void: 
	set_alive(false)
	
func set_alive(alive: bool) -> void:
	self.alive = alive
	($Sprite2D as Sprite2D).modulate = Color.RED
	($CollisionShape2D as CollisionShape2D).disabled = !alive
	($Area2D as Area2D).monitoring = alive

# Notifies the enemy of the given player
func notify_player(player: Player) -> void:
	self.player = player
	
var next_pos: Vector2
func _draw() -> void:
	if (not enable_debug): return
	draw_line(Vector2.ZERO, input * 16, Color.BROWN, 2)
	draw_line(Vector2.ZERO, next_pos - global_position, Color.RED)
	if (knockback.should_process()): 
		draw_line(Vector2.ZERO, knockback._knockback, Color.YELLOW, 3)

func _on_area_2d_body_entered(body: Node2D) -> void:
	if (damage_timeout > 0):
		return
		 
	var player := body as Player
	if (player):
		player.damage(self, 1)
		damage_timeout = DAMAGE_TIMEOUT
		input = player.global_position.direction_to(global_position)
		print("BOOOM BITCH")
