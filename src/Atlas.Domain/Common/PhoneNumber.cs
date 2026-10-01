using System.Diagnostics.CodeAnalysis;

namespace Atlas.Domain.Common;

/// <summary>
/// Helpers for phone numbers typed by people (e.g. "+966 55 123 4567" or "0551234567").
/// Numbers are stored as typed (trimmed), after checking they contain 7–15 digits.
/// </summary>
public static class PhoneNumber
{
    public const int MaxLength = 30;

    public static bool IsValid([NotNullWhen(true)] string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > MaxLength)
        {
            return false;
        }

        var trimmed = value.Trim();
        if (trimmed.Any(c => !(char.IsAsciiDigit(c) || c is '+' or ' ' or '-' or '(' or ')')))
        {
            return false;
        }

        var digits = trimmed.Count(char.IsAsciiDigit);
        return digits is >= 7 and <= 15 && trimmed.LastIndexOf('+') <= 0;
    }

    /// <summary>
    /// Digits for a wa.me link in international format. A Saudi local mobile number
    /// ("05xxxxxxxx") is converted to "9665xxxxxxxx".
    /// </summary>
    public static string ToInternationalDigits(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var digits = new string(value.Where(char.IsAsciiDigit).ToArray());
        if (digits.StartsWith("00", StringComparison.Ordinal))
        {
            digits = digits[2..];
        }
        else if (digits.Length == 10 && digits.StartsWith("05", StringComparison.Ordinal))
        {
            digits = "966" + digits[1..];
        }

        return digits;
    }
}
