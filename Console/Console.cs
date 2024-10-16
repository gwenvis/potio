using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Gwenvis.DeveloperConsole.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Gwenvis.DeveloperConsole;

public class Console
{
    private Console(ILogger? logger = null) 
    {
        _logger = logger ?? NullLogger.Instance;
    }
    
    internal delegate void RunSetCommand(CommandToken commandToken);

    public static Console? SingletonInstance { get; private set; }

    public ConsoleMessages Messages { get; } = new ();
    
    private readonly ILogger _logger;
    
    private readonly Dictionary<string, ConsoleCommand> _commands = new();
    private readonly Tokenizer _tokenizer = new();

    private Stopwatch? _stopwatch = null;
    
    public bool IsDebug { get; set; }
    
    public static void RegisterCommand(string command, Delegate method)
    {
        EnsureConsoleCreated();
        Debug.Assert(SingletonInstance != null, "Console instance is null. One should have been created before using this...");
        SingletonInstance._RegisterCommand(command, method);
    }

    private void _RegisterCommand(string command, Delegate method)
    {
        var info = method.Method;

        if (!info.IsStatic || info.IsGenericMethod || _commands.ContainsKey(command))
        {
            return;
        }

        if (IsDebug) 
            StartStopwatch();
        var call = BuildStaticCallExpression(info);
        if (IsDebug) 
            StopAndPrintStopwatch("Building command took ");
        var consoleCommand = new ConsoleCommand(command, method, info.GetParameters().Length, call);
        _commands.Add(command, consoleCommand);
    }

    public void Run(string rawInput)
    {
        var tokenizer = new Tokenizer();
        
        if (IsDebug)
            StartStopwatch();
        
        using var commandToken = tokenizer.Tokenize(rawInput);
        
        if (IsDebug)
            StopAndPrintStopwatch("Tokenizing took ");

        if (!commandToken.HasValue)
        {
            _logger.LogError("Tokenizing failed");
            return;
        }

        var invocationToken = commandToken.Value.CommandInvocationToken;
        
        if (!_commands.TryGetValue(invocationToken.Value, out var command))
        {
            _logger.LogError("Command {Command} not found", invocationToken.Value);
            return;
        }

        var expectedParameters = command.ArgumentCount;
        if (expectedParameters != (commandToken.Value.Parameters?.Length ?? 0))
        {
            _logger.LogError("Incorrect number of parameters. Expected {Expected}, got {Actual}", expectedParameters, commandToken.Value.Parameters?.Length ?? 0);
            return;
        }

        if (IsDebug)
            StartStopwatch();
        command.runCommand(commandToken.Value);
        if (IsDebug)
            StopAndPrintStopwatch("Running command took ");
    }

    public void Clear() => _commands.Clear();

    public static Console CreateConsole(ILogger? logger = default)
    {
        Debug.Assert(SingletonInstance == null, "Console instance already exists!");
        SingletonInstance = new Console(logger);
        return SingletonInstance;
    }
    
    private static RunSetCommand BuildStaticCallExpression(
        MethodInfo methodInfo)
    {
        var parameters = methodInfo.GetParameters();
        var commandTokenExpression = Expression.Parameter(typeof(CommandToken), "ct");
        var callExpression = parameters.Length == 0
            ? BuildStaticCallWithoutArguments(methodInfo)
            : BuildStaticCallWithArguments(methodInfo, commandTokenExpression, parameters);
        var lambdaExpression = Expression.Lambda<RunSetCommand>(callExpression, commandTokenExpression);
        return lambdaExpression.Compile();
    }

    private static void EnsureConsoleCreated()
    {
        if (SingletonInstance == null!)
        {
            SingletonInstance = new Console();
        }
    }

    private static readonly Dictionary<Type, Delegate> _convertTokenMethods = new()
    {
        { typeof(int), GetInteger },
        { typeof(double), GetDouble },
        { typeof(float), GetSingle },
        { typeof(string), GetText },
    };

    private static Expression BuildStaticCallWithArguments(
        MethodInfo methodInfo,
        ParameterExpression commandTokenExpr,
        ParameterInfo[] parameters)
    {
        using var paramExps = PooledList<ParameterExpression>.GetList();
        using var blockExps = PooledList<Expression>.GetList();
        var span = parameters.AsSpan();

        for (int i = 0; i < span.Length; i++)
        {
            var param = span[i];
            var indexExpr = Expression.Constant(i, typeof(int));
            var type = param.ParameterType;

            if (!_convertTokenMethods.TryGetValue(type, out var convertDelegate))
            {
                throw new NotSupportedException($"{type.Name} is not supported :(");
            }

            var getMethodInfo = convertDelegate.Method;
            var convertExpression = Expression.Call(getMethodInfo, commandTokenExpr, indexExpr);
            var variableExpr = Expression.Variable(type);
            var assignExpr = Expression.Assign(variableExpr, convertExpression);
            paramExps.Add(variableExpr);
            blockExps.Add(assignExpr);
        }

        var callExpression = Expression.Call(methodInfo, paramExps);
        blockExps.Add(callExpression);
        var blockExpr = Expression.Block(paramExps, blockExps);
        return blockExpr;
    }

    private static int GetInteger(CommandToken commandToken, int index)
    {
        var parameter = commandToken.GetParameter<NumberToken>(index);
        var value = int.Parse(parameter.Value);
        return parameter.IsNegative ? -value : value;
    }

    private static string GetText(CommandToken commandToken, int index)
    {
        var parameter = commandToken.GetParameter<TextToken>(index);
        return parameter.Value;
    }

    private static float GetSingle(CommandToken commandToken, int index)
    {
        var parameter = commandToken.GetParameter<NumberToken>(index);
        var value = float.Parse(parameter.Value);
        return parameter.IsNegative ? -value : value;
    }

    private static double GetDouble(CommandToken commandToken, int index)
    {
        var parameter = commandToken.GetParameter<NumberToken>(index);
        var value = double.Parse(parameter.Value);
        return parameter.IsNegative ? -value : value;
    }

    private static MethodCallExpression BuildStaticCallWithoutArguments(MethodInfo methodInfo) => Expression.Call(methodInfo);

    private void StartStopwatch()
    {
        _stopwatch ??= new Stopwatch();
        _stopwatch.Restart();
    }

    private void StopAndPrintStopwatch(string action = "")
    {
        _stopwatch?.Stop();
        _logger.LogInformation(action, _stopwatch.Elapsed.TotalNanoseconds, " nanoseconds");
    }
}

internal record ConsoleCommand(string Command, Delegate Method, int ArgumentCount, Console.RunSetCommand runCommand);
