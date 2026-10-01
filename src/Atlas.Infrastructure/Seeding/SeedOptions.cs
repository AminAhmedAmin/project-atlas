namespace Atlas.Infrastructure.Seeding;

/// <summary>
/// Initial admin account. Supply via user-secrets or environment variables
/// (Seed__AdminEmail / Seed__AdminPassword); never commit real values.
/// </summary>
public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string? AdminEmail { get; set; }

    public string? AdminPassword { get; set; }
}
