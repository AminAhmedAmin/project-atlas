namespace Atlas.Application.Settings;

/// <summary>
/// Fallback branding used until settings are saved in the database.
/// Bound from the "Branding" configuration section.
/// </summary>
public sealed class BrandingDefaults
{
    public const string SectionName = "Branding";

    public string CompanyName { get; set; } = "Atlas";

    public string? Tagline { get; set; }

    public string PrimaryColor { get; set; } = "#1e63e9";

    public string? ContactEmail { get; set; }

    public string? LogoUrl { get; set; }
}
