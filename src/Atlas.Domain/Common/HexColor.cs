using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Atlas.Domain.Common;

/// <summary>A CSS color in #RRGGBB form (stored lower-case).</summary>
public sealed record HexColor
{
    private HexColor(string value) => Value = value;

    public string Value { get; }

    public static HexColor Create(string? value) =>
        TryCreate(value, out var color)
            ? color
            : throw new DomainException($"'{value}' is not a valid hex color. Use the #RRGGBB format.");

    public static bool TryCreate(string? value, [NotNullWhen(true)] out HexColor? color)
    {
        color = null;
        var candidate = value?.Trim();
        if (string.IsNullOrEmpty(candidate) || candidate[0] != '#')
        {
            return false;
        }

        var digits = candidate[1..];
        if (digits.Length == 3)
        {
            digits = string.Concat(digits.Select(c => new string(c, 2)));
        }

        if (digits.Length != 6 || !int.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
        {
            return false;
        }

        color = new HexColor("#" + digits.ToLowerInvariant());
        return true;
    }

    public override string ToString() => Value;
}
