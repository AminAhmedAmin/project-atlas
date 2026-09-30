using Atlas.Domain.Common;

namespace Atlas.Application.Common;

/// <summary>Validates an input before a handler runs. Implementations are registered automatically.</summary>
public interface IValidator<in T>
{
    IReadOnlyList<Error> Validate(T instance);
}

/// <summary>Small fluent helper for collecting field errors.</summary>
public sealed class ValidationErrors
{
    private readonly List<Error> _errors = [];

    public IReadOnlyList<Error> Errors => _errors;

    public ValidationErrors Required(string? value, string field, string label, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Add(field, $"{label} is required.");
        }

        return MaxLength(value, field, label, maxLength);
    }

    public ValidationErrors Optional(string? value, string field, string label, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? this : MaxLength(value, field, label, maxLength);

    public ValidationErrors Email(string? value, string field, string label, bool required)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return required ? Add(field, $"{label} is required.") : this;
        }

        return EmailAddress.TryCreate(value, out _) ? this : Add(field, $"{label} is not a valid e-mail address.");
    }

    public ValidationErrors Must(bool condition, string field, string message) =>
        condition ? this : Add(field, message);

    private ValidationErrors MaxLength(string value, string field, string label, int maxLength) =>
        value.Trim().Length > maxLength ? Add(field, $"{label} must be at most {maxLength} characters.") : this;

    private ValidationErrors Add(string field, string message)
    {
        _errors.Add(Error.Validation(field, message));
        return this;
    }
}
