using Atlas.Domain.Settings;

namespace Atlas.Application.Settings;

public sealed record SiteSettingsDto(
    string CompanyName,
    string? Tagline,
    string? LogoUrl,
    string PrimaryColor,
    string? ContactEmail,
    DateTime? UpdatedAtUtc);

internal static class SiteSettingsMapping
{
    public static SiteSettingsDto ToDto(this SiteSettings settings) => new(
        settings.CompanyName,
        settings.Tagline,
        settings.LogoUrl,
        settings.PrimaryColor.Value,
        settings.ContactEmail?.Value,
        settings.UpdatedAtUtc);

    public static SiteSettingsDto ToDto(this BrandingDefaults defaults) => new(
        defaults.CompanyName,
        defaults.Tagline,
        defaults.LogoUrl,
        defaults.PrimaryColor,
        defaults.ContactEmail,
        null);
}
