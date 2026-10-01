using Atlas.Domain.Common;
using Atlas.Domain.Settings;

namespace Atlas.Application.Settings;

public sealed record SiteSettingsDto(
    string CompanyName,
    string? Tagline,
    string? LogoUrl,
    string PrimaryColor,
    string? ContactEmail,
    DateTime? UpdatedAtUtc,
    string? ArabicCompanyName = null,
    string? ArabicTagline = null,
    string? WhatsAppNumber = null)
{
    /// <summary>Company name for the given language, falling back to the English name.</summary>
    public string CompanyNameFor(SiteLanguage language) =>
        language == SiteLanguage.Arabic && !string.IsNullOrWhiteSpace(ArabicCompanyName) ? ArabicCompanyName : CompanyName;

    /// <summary>Tagline for the given language, falling back to the English tagline.</summary>
    public string? TaglineFor(SiteLanguage language) =>
        language == SiteLanguage.Arabic && !string.IsNullOrWhiteSpace(ArabicTagline) ? ArabicTagline : Tagline;
}

internal static class SiteSettingsMapping
{
    public static SiteSettingsDto ToDto(this SiteSettings settings) => new(
        settings.CompanyName,
        settings.Tagline,
        settings.LogoUrl,
        settings.PrimaryColor.Value,
        settings.ContactEmail?.Value,
        settings.UpdatedAtUtc,
        settings.ArabicCompanyName,
        settings.ArabicTagline,
        settings.WhatsAppNumber);

    public static SiteSettingsDto ToDto(this BrandingDefaults defaults) => new(
        defaults.CompanyName,
        defaults.Tagline,
        defaults.LogoUrl,
        defaults.PrimaryColor,
        defaults.ContactEmail,
        null,
        defaults.ArabicCompanyName,
        defaults.ArabicTagline);
}
