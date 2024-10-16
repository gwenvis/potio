using Godot;
using Gwenvis.DeveloperConsole;
using ImGuiNET;

namespace Potio.Developer.ui;

public partial class ImGuiConsole : Node
{
    private Console _console;

    public ImGuiConsole()
    {
        _console = Console.CreateConsole(new GodotLogger());
    }
    
    public override void _Ready()
    {
        base._Ready();
        
        ImGui.
    }
}