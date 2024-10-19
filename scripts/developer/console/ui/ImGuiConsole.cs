using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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
    private string? _applyCommandInput = null;
    private bool _isAtBottom = true;
    private bool _isOpen = false;

    private readonly List<string> _commandHistory = new();
    private int _currentHistoryIndex = -1;

    public override void _Input(InputEvent inputEvent)
    {
        if ((inputEvent is InputEventKey eventKey && HandleInputEventKey(eventKey)))
        {
            GetViewport().SetInputAsHandled();
        }
    }

    private bool HandleInputEventKey(InputEventKey eventKey)
    {
        if (eventKey is { Keycode: Key.F1, Pressed: true, Echo: false })
        {
            _isOpen = !_isOpen;
            return true;
        }

        return false;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!_isOpen)
        {
            return;
        }

        if (!ImGui.Begin("Console", ref _isOpen))
        {
            ImGui.End();
            return;
        }

        DrawLog();
        DrawCommandLine(); 
        ImGui.End();
    }

    private unsafe void DrawCommandLine()
    {
        const ImGuiInputTextFlags inputTextFlags = ImGuiInputTextFlags.EnterReturnsTrue
                                                   | ImGuiInputTextFlags.CallbackHistory
                                                   | ImGuiInputTextFlags.CallbackCompletion;

        if (_applyCommandInput != null)
        {
            _commandInput = _applyCommandInput;
            _applyCommandInput = null;
        }
        
        unsafe
        {
            var submit = ImGui.InputText("command", ref _commandInput, 1024, inputTextFlags, OnInputCallback);
            if (submit)
            {
                SubmitCommandLine(_commandInput);
                _commandInput = string.Empty;
                ImGui.SetKeyboardFocusHere(-1);
            }
        }
    }

    private void DrawLog()
    {
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
    }

    private void SubmitCommandLine(string input)
    {
        _console.Run(input);
        _commandHistory.Add(input);
    }

    private unsafe int OnInputCallback(ImGuiInputTextCallbackData* data)
    {
        switch (data->EventFlag)
        {
            case ImGuiInputTextFlags.CallbackHistory:
                if (_commandHistory.Count == 0)
                {
                    return 1;
                }
                
                var up = data->EventKey == ImGuiKey.UpArrow;
                var first = _currentHistoryIndex == -1;
                if (up)
                {
                    _currentHistoryIndex = int.Max((first ? _commandHistory.Count : _currentHistoryIndex) - 1, 0);
                    SetInputBuffer(_commandHistory[_currentHistoryIndex]);
                    return 1;
                }

                if (_currentHistoryIndex == -1)
                {
                    SetInputBuffer(string.Empty);
                    return 1;
                }
                
                _currentHistoryIndex += 1;
                if (_currentHistoryIndex >= _commandHistory.Count)
                {
                    _currentHistoryIndex = -1;
                    SetInputBuffer(string.Empty);
                    return 1;
                }
                
                SetInputBuffer(_commandHistory[_currentHistoryIndex]);

                void SetInputBuffer(string input)
                {
                    fixed (char* inputPtr = input)
                        Encoding.UTF8.GetBytes(inputPtr, input.Length, data->Buf, input.Length);
                    data->BufSize = input.Length;
                    data->BufTextLen = input.Length;
                    data->BufDirty = 1;
                    data->CursorPos = input.Length;
                }
                return 1;
            case ImGuiInputTextFlags.CallbackCompletion:
                return 1;
            default: return 1;
        }
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