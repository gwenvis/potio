using Godot;

namespace Potio.Player;

public partial class PlayerInput : Node2D
{
    public event System.Action<Vector2>? MousePressed;
    public event System.Action? InteractPressed;
    public Vector2 MovementInput => _movementInput;

    private Vector2 _movementInput = Vector2.Zero;
    
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsEcho())
        {
            return;
        }
        
        bool handledEvent = false;
        switch (@event)
        {
            case InputEventMouseButton mouseEvent:
                handledEvent = HandleMouseEvent(mouseEvent);
                break;
            case InputEventKey keyEvent:
                handledEvent = HandleKeyEvent(keyEvent);
                break;
        }
    }

    private bool HandleKeyEvent(InputEventKey keyEvent)
    {
        var delta = keyEvent.Pressed ? 1f : -1f;
        switch (keyEvent.Keycode)
        {
            case Key.A:
                _movementInput.X -= delta;
                return true;
            case Key.D:
                _movementInput.X += delta;
                return true;
            case Key.W:
                _movementInput.Y -= delta;
                return true;
            case Key.S:
                _movementInput.Y += delta;
                return true;
            case Key.E:
                InteractPressed?.Invoke();
                return true;
            default:
                return false;
        }
    }

    private bool HandleMouseEvent(InputEventMouseButton mouseEvent)
    {
        if (mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (mouseEvent.Pressed)
            {
                MousePressed?.Invoke(GetGlobalMousePosition());
            }
            return true;
        }

        return false;
    }
}