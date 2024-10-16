using Godot;
using Gwenvis.DeveloperConsole;

namespace Potio.Developer;

[GlobalClass]
public partial class ConsoleTest : Node
{
	public override void _Ready()
	{
		base._Ready();
		var logger = new GodotLogger();
		var console = Console.CreateConsole(logger);

	}

	public static void TestCommand(string text, float number)
	{
		//GD.Print(nameof(TestCommand), " ", text, " ", number);
	}
}
