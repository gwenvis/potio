using Godot;

namespace Potio.Util.Window;

public partial class RememberWindowSize : Node
{
    private const string DataPath = "user://window_data.res";
    
    public override void _EnterTree()
    {
        base._EnterTree();
        LoadWindowData();
    }

    public override void _Notification(int what)
    {
        base._Notification(what);
        switch (what)
        {
            case (int)NotificationWMCloseRequest:
                SaveWindowData();
                break;
        }
    }

    private void SaveWindowData()
    {
        var windowData = new WindowData(DisplayServer.WindowGetPosition(), DisplayServer.WindowGetSize());
        var result = ResourceSaver.Save(windowData, DataPath);
        if (result != Error.Ok)
        {
            PushError("Could not save window data. Error: ", result);
            return;
        }

        Print("Saved window size");
    }

    private void LoadWindowData()
    {
        var windowData = ResourceLoader.Load<WindowData>(DataPath);
        if (windowData is null)
        {
            return;
        }

        var screenSize = DisplayServer.ScreenGetSize();
        var windowPosition = windowData.Position;
        windowPosition.X = Mathf.Clamp(windowPosition.X, 0, screenSize.X - windowData.Size.X);
        windowPosition.Y = Mathf.Clamp(windowPosition.Y, 0, screenSize.Y - windowData.Size.Y);
        DisplayServer.WindowSetSize(windowData.Size);
        DisplayServer.WindowSetPosition(windowPosition);
#if DEBUG
        Print("Loaded window data");
#endif
    }
}