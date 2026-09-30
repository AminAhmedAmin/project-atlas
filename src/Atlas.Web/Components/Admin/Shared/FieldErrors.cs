using Atlas.Application.Common;

namespace Atlas.Web.Components.Admin.Shared;

/// <summary>Maps handler validation errors onto form fields.</summary>
public sealed class FieldErrors
{
    private Dictionary<string, string> _errors = new(StringComparer.Ordinal);

    /// <summary>Errors that are not tied to a specific field.</summary>
    public string? General { get; private set; }

    public bool Has(string field) => _errors.ContainsKey(field);

    public string? For(string field) => _errors.GetValueOrDefault(field);

    public void Clear()
    {
        _errors = new(StringComparer.Ordinal);
        General = null;
    }

    public void Set(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);
        Clear();
        foreach (var error in result.Errors)
        {
            if (error.Field is { } field && !_errors.ContainsKey(field))
            {
                _errors[field] = error.Message;
            }
            else if (error.Field is null)
            {
                General = General is null ? error.Message : $"{General} {error.Message}";
            }
        }
    }
}
