namespace Atlas.Domain.Common;

internal static class Guard
{
    public static string Required(string? value, string name, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw new DomainException($"{name} is required.");
        }

        return MaxLength(trimmed, name, maxLength);
    }

    public static string? Optional(string? value, string name, int maxLength)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : MaxLength(trimmed, name, maxLength);
    }

    public static DateTime Utc(DateTime value, string name)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new DomainException($"{name} must be a UTC timestamp.");
        }

        return value;
    }

    /// <summary>Accepts site-relative paths ("/contact") and absolute http(s) URLs.</summary>
    public static string? OptionalLink(string? value, string name, int maxLength)
    {
        var link = Optional(value, name, maxLength);
        if (link is null)
        {
            return null;
        }

        var isRelative = link.StartsWith('/') && !link.StartsWith("//", StringComparison.Ordinal);
        var isAbsoluteHttp = Uri.TryCreate(link, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        if (!isRelative && !isAbsoluteHttp)
        {
            throw new DomainException($"{name} must be a site path starting with '/' or an http(s) URL.");
        }

        return link;
    }

    private static string MaxLength(string value, string name, int maxLength)
    {
        if (value.Length > maxLength)
        {
            throw new DomainException($"{name} must be at most {maxLength} characters.");
        }

        return value;
    }
}
