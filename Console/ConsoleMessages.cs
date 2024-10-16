using System.Text;
using Microsoft.Extensions.Logging;

namespace Gwenvis.DeveloperConsole;

public class ConsoleMessages
{
    public IReadOnlyList<ConsoleMessage> Messages => _messages;
    
    private readonly List<ConsoleMessage> _messages = [];

    public void AddMessage(string message, LogLevel logLevel = LogLevel.None)
    {
        _messages.Add(new ConsoleMessage(message, logLevel));
    }
}

public record struct ConsoleMessage(string Message, LogLevel LogLevel = LogLevel.None);