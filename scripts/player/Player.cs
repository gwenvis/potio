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

public partial class Player : CharacterBody2D, IHurtable
{
    private PlayerInput _playerInput = null!;
    private Movement _movement = null!;

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
        _playerInput = new PlayerInput();
        _movement = new Movement(this);
        AddChild(_playerInput);
        MotionMode = MotionModeEnum.Floating;
    }

    public override void _Ready()
    {
        _playerInput.MousePressed += OnMousePressed;
        _attackSprite.Visible = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        _movement.ProcessMovement(_playerInput.MovementInput, delta);
        ProcessAttack((float)delta);
        MoveAndSlide();
    }

    public void Hurt(Node2D who, double amount)
    {
        GlobalLogger.Info($"Ouch, {who.Name} for {amount}");
    }

    private void ProcessAttack(float delta)
    {
        if (DecrementTimer(ref _attackCooldownTimer, delta))
        {
            _recentlyAttacked.Remove(_playerInput);
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
            if (!_recentlyAttacked.Contains(body) 
                && body is IHurtable hurtable)
            {
                hurtable.Hurt(this, 1);
            }

            _recentlyAttacked.Add(body);
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
}