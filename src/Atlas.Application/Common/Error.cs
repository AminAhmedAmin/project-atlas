namespace Atlas.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
}

/// <param name="Code">Stable machine-readable code.</param>
/// <param name="Message">Human-readable message suitable for the UI.</param>
/// <param name="Type">Error category.</param>
/// <param name="Field">Name of the offending input field, when applicable.</param>
public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Validation, string? Field = null)
{
    public static Error NotFound(string what) => new($"{what}.NotFound", $"{what} was not found.", ErrorType.NotFound);

    public static Error Validation(string field, string message) => new("Validation", message, ErrorType.Validation, field);
}
