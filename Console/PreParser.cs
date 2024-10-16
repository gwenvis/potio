using Range = System.Range;

namespace Gwenvis.DeveloperConsole;

internal ref struct PreParser
{
    private const string AllNormalCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVXYZ0123456789!@#$%^&*+-.,/:;";
    
    public PreParser(
        ReadOnlySpan<char> source, 
        Span<TextGroup> destination, 
        string startSpecialToken = "\"({[<",
        string endSpecialToken = "\")}]>")
    {
        _source = source;
        _destination = destination;
        _startSpecialTokens = startSpecialToken;
        _endSpecialTokens = endSpecialToken;
        var length = AllNormalCharacters.Length + _startSpecialTokens.Length + _endSpecialTokens.Length;
        Span<char> allValidCharacters = stackalloc char[length];
        AllNormalCharacters.CopyTo(allValidCharacters);
        _startSpecialTokens.CopyTo(allValidCharacters[AllNormalCharacters.Length..]);
        _endSpecialTokens.CopyTo(allValidCharacters[AllNormalCharacters.Length..]);
        _normalCharacters = AllNormalCharacters.AsSpan();
    }
    
    private State _state = State.Default;
    private int _destinationHead = 0;
    private int _index = 0;
    
    private readonly ReadOnlySpan<char> _source;
    private readonly Span<TextGroup> _destination;
    private readonly ReadOnlySpan<char> _startSpecialTokens;
    private readonly ReadOnlySpan<char> _endSpecialTokens;
    private readonly ReadOnlySpan<char> _normalCharacters;

    internal ParseResult Parse()
    {
        var result = ParseResult.Success;
        while (_index < _source.Length && !result.IsFailure)
        {
            result = _state switch
            {
                State.Default => ParseDefault(),
                State.AtLetter => ParseAtNormal(),
                State.AtSpecialToken => ParseAtSpecial(),
                _ => ParseResult.Failed($"{_state.ToString()} is not handled.")
            };
        }

        return result.IsFailure ? result with { Count = _destinationHead } : ParseResult.Completed(_destinationHead);
    }

    private ParseResult ParseAtNormal()
    {
        int start = _index;
        int head = start + 1;
        bool success = false;
        for (; head < _source.Length && !success; head++)
        {
            var characterType = GetCharacterType(_source[head]);
            switch (characterType)
            {
                case CharacterType.Invalid:
                case CharacterType.Special:
                    success = true;
                    head -= 1;
                    break;
                case CharacterType.Normal: break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (success || head == _source.Length)
        {
            _state = State.Default;
            _index = head + 1;
            bool result = AddTextGroup(new Range(start, head), GroupType.None);
            return ParseResult.Success;
        }
        
        return ParseResult.Failed("Could not find end of normal text group.");
    }

    private ParseResult ParseAtSpecial()
    {
        char startCharacter = _source[_index];
        int specialCharacterIndex = _startSpecialTokens.IndexOf(_source[_index]);
        char endCharacter = _endSpecialTokens[specialCharacterIndex];
        int start = _index + 1;
        var isIdentical = startCharacter == endCharacter;
        int insideCounter = 0;

        for (int i = start; i < _source.Length; i++)
        {
            var currentChar = _source[i];

            if (endCharacter == currentChar)
            {
                if (insideCounter > 0)
                {
                    insideCounter -= 1;
                    continue;
                }
                
                _index = i + 1;
                _state = State.Default;
                AddTextGroup(new Range(start, i), GetGroupType(startCharacter));
                return ParseResult.Success;
            }
            
            if (currentChar == startCharacter)
            {
                insideCounter += 1;
            }
        }
        
        return ParseResult.Failed($"Could not find end of special character {startCharacter} (expected '{endCharacter}')");
    }

    private ParseResult ParseDefault()
    {
        for (int i = _index; i < _source.Length; i++)
        {
            var characterType = GetCharacterType(_source[i]);
            switch (characterType)
            {
                case CharacterType.Normal:
                    _state = State.AtLetter;
                    _index = i;
                    return ParseResult.Success;
                case CharacterType.Special:
                    _state = State.AtSpecialToken;
                    _index = i;
                    return ParseResult.Success;
                case CharacterType.Invalid: break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return ParseResult.Failed("Could not find start.");
    }

    private bool AddTextGroup(Range range, GroupType groupType)
    {
        if (_destinationHead >= _destination.Length)
        {
            return false;
        }

        _destination[_destinationHead] = new TextGroup(range, groupType);
        _destinationHead++;
        return true;
    }

    private CharacterType GetCharacterType(char character)
    {
        return _normalCharacters.Contains(character)
            ? CharacterType.Normal
            : _startSpecialTokens.Contains(character) || _endSpecialTokens.Contains(character)
                ? CharacterType.Special
                : CharacterType.Invalid;
    }

    private GroupType GetGroupType(char startGroup) => startGroup switch
    {
        '{' => GroupType.CurlyBrace,
        '"' => GroupType.DoubleQuote,
        '(' => GroupType.Parenthesis,
        '[' => GroupType.SquareBrace,
        '\'' => GroupType.SingleQuote,
        _ => throw new ArgumentOutOfRangeException(nameof(startGroup), startGroup, null)
    };

    private enum State
    {
        Default,
        AtSpecialToken,
        AtLetter,
    }
}

internal record struct TextGroup(Range Range, GroupType GroupType);

internal record struct ParseResult(bool IsFailure = false, string? ErrorMessage = default, int Count = 0)
{
    public static ParseResult Success => new();
    public static ParseResult Completed(int parsedCount) => new(Count: parsedCount);
    public static ParseResult Failed(string errorMessage) => new(true, errorMessage);
}

internal enum GroupType
{
    None,
    DoubleQuote,
    SingleQuote,
    CurlyBrace,
    SquareBrace,
    Parenthesis
}

internal enum CharacterType
{
    Invalid,
    Normal,
    Special
}