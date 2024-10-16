using System.Diagnostics.CodeAnalysis;

namespace Gwenvis.DeveloperConsole;

public record struct Result(bool IsFailure, string? Name, string? Description)
{
    public static Result Error(string? description = null) =>
        new(true, "Error", description);

    public static Result Success(string? description = null) =>
        new(false, "Success", description);
}