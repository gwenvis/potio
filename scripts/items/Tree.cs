using Godot;

namespace Potio.Objects;

public partial class Tree : Node2D, IBasicHealth
{
    [Export] private PackedScene _dropOnDestroy = null!;
    
    public BasicHealth Health => _basicHealth;
    private const int StartHealth = 4;
    private readonly BasicHealth _basicHealth = new(StartHealth);

    private int _currentHealth = StartHealth;

    public override void _Ready()
    {
        base._Ready();
        Health.IsDebug = true;
        Health.Depleted += who =>
        {
            var node = _dropOnDestroy.Instantiate<Node2D>();
            node.GlobalPosition = GlobalPosition;
            GetParent().AddChild(node);
            QueueFree();
        };
    }
}