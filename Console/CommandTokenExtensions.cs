using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Gwenvis.DeveloperConsole;

internal static class CommandTokenExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IToken GetParameter(this CommandToken commandToken, int index)
    {
        Debug.Assert(commandToken.Parameters.HasValue);
        return commandToken.Parameters.Value.Span[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetParameter<T>(this CommandToken commandToken, int index) where T : IToken
    {
        var parameterToken = GetParameter(commandToken, index);
        Debug.Assert(parameterToken is T);
        return (T)parameterToken;
    }
}