using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace potio.scripts.developer.console;

internal class Tokenizer
{
    public IReadOnlyList<Result> Errors => _errors.AsReadOnly();
    
    private readonly List<Result> _errors = new();
    
    public CommandToken? Tokenize(in string input)
    {
        _errors.Clear();
        
        Span<TextGroup> destination = stackalloc TextGroup[16];
        var preParser = new PreParser(input, destination);
        var result = preParser.Parse();

        if (result.IsFailure)
        {
            _errors.Add(Result.Error(description: result.ErrorMessage ?? "No error message."));
            return null;
        }

        if (result.Count == 0)
        {
            _errors.Add(Result.Error(description: "No error found but count was 0."));
            return null;
        }

        return Tokenize(input, destination[..result.Count]);
    }

    private CommandToken? Tokenize(in ReadOnlySpan<char> input, in ReadOnlySpan<TextGroup> textGroup)
    {
        var invocationToken = new TextToken(input[textGroup[0].Range].ToString());

        if (textGroup.Length == 1)
        {
            return new CommandToken(invocationToken);
        }

        var owner = MemoryPool<IToken>.Shared.Rent();
        var parameterTextGroups = textGroup[1..];
        var tokenMemory = owner.Memory[..parameterTextGroups.Length];
        var tokenSpan = tokenMemory.Span;
        
        for (int i = 0; i < tokenSpan.Length; i++)
        {
            var group = parameterTextGroups[i];
            tokenSpan[i] = Parse(input[group.Range], group);
        }

        return new CommandToken(invocationToken, tokenMemory, owner);
    }

    private IToken Parse(in ReadOnlySpan<char> value, in TextGroup textGroup)
    {
        if (TryParseNumberToken(value, out var numberToken))
        {
            return numberToken;
        }

        return GetTextToken(value);
    }

    private IToken GetTextToken(in ReadOnlySpan<char> value)
    {
        return new TextToken(value.ToString());
    }

    private bool TryParseNumberToken(in ReadOnlySpan<char> value, [NotNullWhen(true)] out IToken? numberToken)
    {
        bool isDecimal = false;
        bool isNegative = false;
        bool success = true;
        int start = 0;
        
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];

            switch (character)
            {
                case '-':
                    isNegative = true;
                    success &= i == 0;
                    start = 1;
                    continue;
                case '.':
                    success &= !isDecimal;
                    isDecimal = true;
                    continue;
                case '0'
                  or '1' 
                  or '2' 
                  or '3' 
                  or '4'
                  or '5'
                  or '6'
                  or '7'
                  or '8'
                  or '9'
                  or ',':
                    continue;
                default:
                    success = false;
                    break;
            }
        }

        numberToken = success ? new NumberToken(value[start..].ToString(), isNegative, isDecimal) : default;
        return success;
    }
}


internal readonly record struct CommandToken : IDisposable
{
    internal readonly TextToken CommandInvocationToken;
    internal readonly Memory<IToken>? Parameters;
    private readonly IMemoryOwner<IToken>? _owner;
    

    public CommandToken(
        TextToken commandInvocationToken, 
        Memory<IToken>? parameters = null, 
        IMemoryOwner<IToken>? owner = null)
    {
        CommandInvocationToken = commandInvocationToken;
        Parameters = parameters;
        _owner = owner;
    }

    public void Dispose()
    {
        _owner?.Dispose();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CommandInvocationToken: {CommandInvocationToken.ToString()}");

        if (Parameters is not null)
        {
            sb.AppendLine($"Parameter.Count: {Parameters.Value.Length}");
            for (var index = 0; index < Parameters.Value.Span.Length; index++)
            {
                var parameter = Parameters.Value.Span[index];
                sb.AppendLine($"  {index}: {parameter.ToString()}");
            }
        }

        return sb.ToString();
    }
}

internal readonly record struct NumberToken(string Value, bool IsNegative, bool IsDecimal) : INumberToken
{
    public override string ToString()
    {
        return $$"""{NumberToken} {{Value}}{{(IsNegative ? " Negative": string.Empty)}}{{(IsDecimal ? " Decimal": string.Empty)}}""";
    }
}

internal readonly record struct TextToken(string Value) : ITextToken
{
    public override string ToString()
    {
        return $$"""{TextToken} {{Value}}""";
    }
}
internal readonly record struct SimpleMathToken(INumberToken Lhs, ISimpleMathOperationToken Operator, INumberToken Rhs) : ISimpleMathToken;

interface ITextToken : IToken
{
    string Value { get; }

    ReadOnlySpan<char> AsSpan() => Value.AsSpan();
}

interface INumberToken : ITextToken
{
    bool IsNegative { get; }
    bool IsDecimal { get; }
}

interface ISimpleMathToken : IToken
{
    INumberToken Lhs { get; }
    ISimpleMathOperationToken Operator { get; }
    INumberToken Rhs { get; }
}

interface ISimpleMathOperationToken
{
    
}

interface IToken;