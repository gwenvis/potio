using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Environment = System.Environment;

namespace potio.scripts.developer.console;

[GlobalClass]
public partial class ConsoleTest : Node
{
	public override void _Ready()
	{
		base._Ready();

		for (int i = 0; i < 200; i++)
		{
			Console.RegisterCommand("Garbo" + i, TestCommand);
			Console.Run("Garbo" + i + " text 1");
		}

		Console.IsDebug = true;
		Console.RegisterCommand("testcommand", TestCommand);
		Console.RegisterCommand("testcommand2", TestCommand);
		Console.RegisterCommand("testcommand3", TestCommand);
		Console.RegisterCommand("testcommand4", TestCommand);
		Console.RegisterCommand("testcommand5", TestCommand);
		Console.RegisterCommand("testcommand6", TestCommand);
		Console.RegisterCommand("testcommand7", TestCommand);
		Console.RegisterCommand("testcommand9", TestCommand);
		Console.RegisterCommand("testcommand8", TestCommand);
		Console.RegisterCommand("testcommand10", TestCommand);
		Console.RegisterCommand("testcommand11", TestCommand);
		Console.RegisterCommand("testcommand12", TestCommand);
		Console.Run("testcommand thisistext 2");
		Console.Run("testcommand \"this is text\" 25");
		Console.Run("testcommand (this is also text) -20");
		Console.Run("testcommand (this is also text) -20.15");
	}

	public static void TestCommand(string text, float number)
	{
		//GD.Print(nameof(TestCommand), " ", text, " ", number);
	}
}
