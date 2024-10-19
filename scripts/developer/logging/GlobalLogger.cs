using Gwenvis.DeveloperConsole;
using Microsoft.Extensions.Logging;
using Potio.Developer;

namespace Potio;

public static class GlobalLogger
{
    public static void Info(string message)
    {
        Console.SingletonInstance?.Message.AddMessage(message, LogLevel.Information);
        Print(message);
    }
}