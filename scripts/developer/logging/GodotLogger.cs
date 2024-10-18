using System;
using System.Collections.Generic;
using Godot;
using Microsoft.Extensions.Logging;
using Gwenvis.DeveloperConsole;
using Console = Gwenvis.DeveloperConsole.Console;

namespace Potio.Developer;

public class GodotLogger : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var formatted = formatter(state, exception);
        Console.SingletonInstance?.Message.AddMessage(formatted, logLevel);
        switch (logLevel)
        {
            case LogLevel.Trace:
                PrintRich($"Trace: {formatted}");
                break;
            case LogLevel.Debug:
                PrintRich($"[color=green]Debug:[/color] {formatted}");
                break;
            case LogLevel.Information:
                PrintRich($"[color=cyan]Debug:[/color] {formatted}");
                break;
            case LogLevel.Warning:
                PushWarning($"Warning: {formatted}");
                break;
            case LogLevel.Error:
                PushError($"Error: {formatted}");
                break;
            case LogLevel.Critical:
                PushError($"!!! CRITICAL: {formatted} !!!");
                break;
            case LogLevel.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return default!;
    }
}