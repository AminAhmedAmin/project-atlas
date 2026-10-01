using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;

namespace Atlas.Domain.Common;

/// <summary>A syntactically valid e-mail address.</summary>
public sealed record EmailAddress
{
    public const int MaxLength = 254;

    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    public static EmailAddress Create(string? value) =>
        TryCreate(value, out var email)
            ? email
            : throw new DomainException($"'{value}' is not a valid e-mail address.");

    public static bool TryCreate(string? value, [NotNullWhen(true)] out EmailAddress? email)
    {
        email = null;
        var candidate = value?.Trim();
        if (string.IsNullOrEmpty(candidate) || candidate.Length > MaxLength)
        {
            return false;
        }

        // Reject display-name forms such as "Jane <jane@example.com>"; only a bare address is valid.
        if (!MailAddress.TryCreate(candidate, out var parsed)
            || !string.Equals(parsed.Address, candidate, StringComparison.Ordinal)
            || !parsed.Host.Contains('.', StringComparison.Ordinal))
        {
            return false;
        }

        email = new EmailAddress(candidate);
        return true;
    }

    public override string ToString() => Value;
}
