using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Console.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using potio.scripts.developer.console;

namespace Console;

public class Console
{
    private static Console _instance = null!;
    private readonly ILogger _logger;
    
    public Console(ILogger? logger = null) 
    {
        _instance = this;
        _logger = logger ?? NullLogger.Instance;
    }
    
    internal delegate void RunSetCommand(CommandToken commandToken);
    
    private static readonly Dictionary<string, ConsoleCommand> _commands = new();
    private static readonly Tokenizer _tokenizer = new();

    private static Stopwatch? _stopwatch = null;
    
    public static bool IsDebug { get; set; }
    
    
    public static void RegisterCommand(string command, Delegate method)
    {
        Debug.Assert(_instance != null, "Console instance is null. One should have been created before using this...");
        
        var info = method.Method;

        if (!info.IsStatic || info.IsGenericMethod || _commands.ContainsKey(command))
        {
            return;
        }

        if (IsDebug) 
            _instance.StartStopwatch();
        var call = BuildStaticCallExpression(info);
        if (IsDebug) 
            _instance.StopAndPrintStopwatch("Building command took ");
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
            GD.PrintErr("Tokenizing failed");
            return;
        }

        var invocationToken = commandToken.Value.CommandInvocationToken;
        
        if (!_commands.TryGetValue(invocationToken.Value, out var command))
        {
            GD.PrintErr("Command ", invocationToken.Value, " not found.");
            return;
        }

        var expectedParameters = command.ArgumentCount;
        if (expectedParameters != (commandToken.Value.Parameters?.Length ?? 0))
        {
            GD.PrintErr("Incorrect number of parameters");
            return;
        }

        if (IsDebug)
            StartStopwatch();
        command.runCommand(commandToken.Value);
        if (IsDebug)
            StopAndPrintStopwatch("Running command took ");
    }

    public static void Clear() => _commands.Clear();
    
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

    private static Dictionary<Type, Delegate> ConvertTokenMethods = new()
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

            if (!ConvertTokenMethods.TryGetValue(type, out var convertDelegate))
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
        
        GD.Print(action, _stopwatch.Elapsed.TotalNanoseconds, " nanoseconds");
    }
}

internal record ConsoleCommand(string Command, Delegate Method, int ArgumentCount, Console.RunSetCommand runCommand);
