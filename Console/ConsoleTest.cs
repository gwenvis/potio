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
            Console.Console.RegisterCommand("Garbo" + i, TestCommand);
            Console.Console.Run("Garbo" + i + " text 1");
        }

        Console.Console.IsDebug = true;
        Console.Console.RegisterCommand("testcommand", TestCommand);
        Console.Console.RegisterCommand("testcommand2", TestCommand);
        Console.Console.RegisterCommand("testcommand3", TestCommand);
        Console.Console.RegisterCommand("testcommand4", TestCommand);
        Console.Console.RegisterCommand("testcommand5", TestCommand);
        Console.Console.RegisterCommand("testcommand6", TestCommand);
        Console.Console.RegisterCommand("testcommand7", TestCommand);
        Console.Console.RegisterCommand("testcommand9", TestCommand);
        Console.Console.RegisterCommand("testcommand8", TestCommand);
        Console.Console.RegisterCommand("testcommand10", TestCommand);
        Console.Console.RegisterCommand("testcommand11", TestCommand);
        Console.Console.RegisterCommand("testcommand12", TestCommand);
        Console.Console.Run("testcommand thisistext 2");
        Console.Console.Run("testcommand \"this is text\" 25");
        Console.Console.Run("testcommand (this is also text) -20");
        Console.Console.Run("testcommand (this is also text) -20.15");
    }

    public static void TestCommand(string text, float number)
    {
        //GD.Print(nameof(TestCommand), " ", text, " ", number);
    }
}