using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Godot;

namespace Potio.Player;

public class Movement(CharacterBody2D body) 
{
    public float MaxSpeed { get; set; } = 160f;
    public float Acceleration { get; set; } = 2400f;
    public float Friction { get; set; } = 1600f;
    
    public void ProcessMovement(
        Vector2 directionInput, 
        double delta)
    {
        if (directionInput.IsZeroApprox())
        {
            ApplyFriction((float)delta);
            return;
        }

        var direction = directionInput.Normalized();
        body.Velocity += direction * Acceleration * (float)delta;
        body.Velocity = body.Velocity.LimitLength(MaxSpeed);
    }
    
    private void ApplyFriction(float delta)
    {
        var friction = Friction * delta;
        if (body.Velocity.LengthSquared() > friction * friction)
        {
            body.Velocity -= body.Velocity.Normalized() * friction;
        }
        else
        {
            body.Velocity = Vector2.Zero;
        }
    }
}

public partial class Player : CharacterBody2D, IBasicHealth
{
    private PlayerInput _playerInput = null!;
    private Movement _movement = null!;
    
    // --- HEALTH ---
    private const double StartingHealth = 20;
    public BasicHealth Health { get; private set; } = null!;
    
    // --- ATTACK ---
    private const int AttackSpriteOffset = 24;
    private const float AttackActiveTime = 0.15f;
    private const float AttackSpriteActiveTime = 0.2f;
    private const float AttackCooldownTime = 0.3f;
    
    [Export] private Sprite2D _attackSprite = null!;
    [Export] private Area2D _attackArea = null!;
    private float  _attackActiveTime = 0;
    private float _attackSpriteActiveTime;
    private float _attackCooldownTimer;
    private readonly HashSet<Node> _recentlyAttacked = new();
    
    public override void _EnterTree()
    {
        Debug.Assert(_attackSprite != null);
        Debug.Assert(_attackArea != null);
        MotionMode = MotionModeEnum.Floating;
        CollisionLayer = PhysicsLayers.Player;
        
        _movement = new Movement(this);
        Health = new BasicHealth(StartingHealth);
        _playerInput = new PlayerInput();
        AddChild(_playerInput);
    }

    public override void _Ready()
    {
        _playerInput.MousePressed += OnMousePressed;
        _attackSprite.Visible = false;

        Health.Damaged += OnHealthDamaged;
        Health.Depleted += OnHealthDepleted;
    }

    public override void _PhysicsProcess(double delta)
    {
        _movement.ProcessMovement(_playerInput.MovementInput, delta);
        ProcessAttack((float)delta);
        MoveAndSlide();
    }
        
    private void ProcessAttack(float delta)
    {
        if (DecrementTimer(ref _attackCooldownTimer, delta))
        {
            _recentlyAttacked.Clear();
            GlobalLogger.Info("Clered list");
        }

        if (_attackActiveTime > Mathf.Epsilon 
            && !DecrementTimer(ref _attackActiveTime, delta))
        {
            DamageAllInAttackArea();
        }

        if (DecrementTimer(ref _attackSpriteActiveTime, delta))
        {
            _attackSprite.Visible = false;
        }
    }
    
    private void DamageAllInAttackArea()
    {
        var bodies = _attackArea.GetOverlappingBodies();
        foreach (var body in bodies)
        {
            GlobalLogger.Info(body.GetTree().GetRoot().Name);
            var root = body.GetOwner();
            if (!_recentlyAttacked.Contains(root) 
                && root is IHurtable hurtable)
            {
                hurtable.Hurt(this, 1);
            }

            if (!_recentlyAttacked.Contains(root))
                GlobalLogger.Info(root.Name);

            _recentlyAttacked.Add(root);
        }
    }

    private void PerformAttack(Vector2 mousePos)
    {
        if (_attackCooldownTimer > Mathf.Epsilon)
        {
            return;
        }

        _attackCooldownTimer = AttackCooldownTime;
        _attackActiveTime = AttackActiveTime;
        _attackSpriteActiveTime = AttackSpriteActiveTime;
        _attackSprite.Visible = true;

        var center = GlobalPosition;
        var vec = (mousePos - center).Normalized();
        var rot = Atan2(vec.Y, vec.X) + DegToRad(45f);
        _attackSprite.Position = vec * AttackSpriteOffset;
        _attackSprite.Rotation = rot;
    }

    private void OnMousePressed(Vector2 mousePos)
    {
        PerformAttack(mousePos);
    }

    /// <summary>
    /// Decrement timer and returns true if zero has been reached
    /// </summary>
    private bool DecrementTimer(ref float timer, float delta)
    {
        if (timer < Mathf.Epsilon)
        {
            return false;
        }
        timer -= delta;
        return timer < Mathf.Epsilon;
    }

    private void OnHealthDamaged(Node2D who, double amount)
    {
        GlobalLogger.Info($"Youch! {who.Name} {amount} left: {Health.CurrentHealth}");
    }

    private void OnHealthDepleted(Node2D who)
    {
        GlobalLogger.Info($"I deaded! {who.Name}");
    }
}