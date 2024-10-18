using System;
using Godot;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Console = Gwenvis.DeveloperConsole.Console;

namespace Potio.Developer.ui;

public partial class ImGuiConsole : Node
{
    private Console _console = Console.CreateConsole(new GodotLogger());
    private static ILogger _logger = new GodotLogger();

    private string _commandInput = string.Empty;
    private bool _isAtBottom = true;

    public override void _Ready()
    {
        base._Ready();
        Console.RegisterCommand("debug", PrintDebug);
        Console.RegisterCommand("trace", PrintTrace);
        Console.RegisterCommand("information", PrintInformation);
        Console.RegisterCommand("warning", PrintWarning);
        Console.RegisterCommand("error", PrintError);
        Console.RegisterCommand("critical", PrintCritical);

    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (!ImGui.Begin("Console"))
        {
            ImGui.End();
            return;
        }

        var height = ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().ItemSpacing.Y;
        if (ImGui.BeginChild("##Log", new System.Numerics.Vector2(0, -height), ImGuiChildFlags.None))
        {
            ImGui.PushTextWrapPos();
            foreach (var message in _console.Message.Messages)
            {
                ImGui.TextColored(GetColor(message.LogLevel), $"({message.LogLevel})");
                ImGui.SameLine();
                ImGui.TextUnformatted(message.Message);
            }
            ImGui.PopTextWrapPos();
            
            if (_isAtBottom && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
            {
                ImGui.SetScrollHereY(1.0f);
            }
            
            ImGui.EndChild();
        }


        const ImGuiInputTextFlags inputTextFlags = ImGuiInputTextFlags.EnterReturnsTrue
                                                   | ImGuiInputTextFlags.CallbackHistory
                                                   | ImGuiInputTextFlags.CallbackCompletion;
        unsafe
        {
            var submit = ImGui.InputText("command", ref _commandInput, 1024, inputTextFlags, OnInputCallback);
            if (submit)
            {
                _console.Run(_commandInput);
                _commandInput = string.Empty;
                ImGui.SetKeyboardFocusHere(-1);
            }
        }

        ImGui.End();
    }

    private unsafe int OnInputCallback(ImGuiInputTextCallbackData* data)
    {
        _logger.LogTrace("{Event}", data->EventFlag);
        return 0;
    }

    private static System.Numerics.Vector4 GetColor(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => new System.Numerics.Vector4(0.7f, 0.7f, 0.7f, 1),
            LogLevel.Debug => new System.Numerics.Vector4(0.9f, 0.9f, 0.9f, 1),
            LogLevel.Information => new System.Numerics.Vector4(1, 1, 1, 1),
            LogLevel.Warning => new System.Numerics.Vector4(0.99f, 1, 0.58f, 1),
            LogLevel.Error => new System.Numerics.Vector4(0.8f, 0, 0.1f, 1),
            LogLevel.Critical => new System.Numerics.Vector4(1f, 0, 0, 1),
            LogLevel.None => new System.Numerics.Vector4(1, 1, 1, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
    }

    public static void PrintTrace() => _logger.LogTrace("Hello Trace");
    public static void PrintDebug() => _logger.LogDebug("Hello debug");
    public static void PrintInformation() => _logger.LogInformation("Hello informataion");
    public static void PrintWarning() => _logger.LogWarning("Hello warning");
    public static void PrintError() => _logger.LogError("Hello error");
    public static void PrintCritical() => _logger.LogCritical("Hello critical");
}