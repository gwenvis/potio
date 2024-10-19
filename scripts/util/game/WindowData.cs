using Godot;

namespace Potio.Util.Window;

public partial class WindowData : Resource
{
    [Export]
    public Vector2I Position { get; private set; }
    [Export]
    public Vector2I Size { get; private set; }

    public WindowData()
    { }

    public WindowData(Vector2I position, Vector2I size)
    {
        Position = position;
        Size = size;
    }
}