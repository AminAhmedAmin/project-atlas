using Atlas.Application.Content;
using Atlas.Application.Security;
using Atlas.Application.Settings;
using Atlas.Domain.Common;
using Atlas.Domain.Content;
using Atlas.Domain.Portfolio;
using Atlas.Domain.Settings;
using Atlas.Infrastructure.Identity;
using Atlas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Seeding;

/// <summary>Prepares the database: schema, roles, the initial admin and starter content. Safe to run on every start.</summary>
public sealed partial class DatabaseSeeder(
    AtlasDbContext db,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<DatabaseOptions> databaseOptions,
    IOptions<SeedOptions> seedOptions,
    BrandingDefaults brandingDefaults,
    TimeProvider timeProvider,
    ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await PrepareSchemaAsync(cancellationToken);
        await EnsureRolesAsync();
        await EnsureAdminAsync();
        await EnsureStarterContentAsync(cancellationToken);
    }

    private async Task PrepareSchemaAsync(CancellationToken cancellationToken)
    {
        var options = databaseOptions.Value;
        if (options.Provider == DatabaseProvider.Sqlite)
        {
            // Migrations target SQL Server; the SQLite demo database is created directly from the model.
            await db.Database.EnsureCreatedAsync(cancellationToken);
            return;
        }

        if (options.ApplyMigrationsOnStartup)
        {
            LogApplyingMigrations(logger);
            await db.Database.MigrateAsync(cancellationToken);
        }
    }

    private async Task EnsureRolesAsync()
    {
        if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
        {
            ThrowIfFailed(await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin)), "create the Admin role");
        }
    }

    private async Task EnsureAdminAsync()
    {
        var email = seedOptions.Value.AdminEmail?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            LogNoAdminConfigured(logger);
            return;
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            var password = seedOptions.Value.AdminPassword;
            if (string.IsNullOrEmpty(password))
            {
                LogAdminPasswordMissing(logger, email);
                return;
            }

            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            ThrowIfFailed(await userManager.CreateAsync(user, password), "create the initial admin user");
            LogAdminCreated(logger, email);
        }

        // Existing passwords are never reset by seeding.
        if (!await userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            ThrowIfFailed(await userManager.AddToRoleAsync(user, AppRoles.Admin), "grant the Admin role");
        }
    }

    private async Task EnsureStarterContentAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        if (!await db.SiteSettings.AnyAsync(cancellationToken))
        {
            var color = HexColor.TryCreate(brandingDefaults.PrimaryColor, out var parsed) ? parsed : HexColor.Create("#1e63e9");
            EmailAddress.TryCreate(brandingDefaults.ContactEmail, out var contactEmail);

            var settings = SiteSettings.Create(brandingDefaults.CompanyName, brandingDefaults.Tagline, color, contactEmail, now);
            settings.UpdateArabic(brandingDefaults.ArabicCompanyName, brandingDefaults.ArabicTagline, now);
            if (!string.IsNullOrWhiteSpace(brandingDefaults.LogoUrl))
            {
                settings.SetLogo(brandingDefaults.LogoUrl, now);
            }

            db.SiteSettings.Add(settings);
        }

        // Starter content is written once per language: pages cannot be deleted from the dashboard,
        // so "no pages in this language" reliably means the language has never been seeded.
        // Admins can then delete starter services or blocks freely without them coming back.
        foreach (var language in Enum.GetValues<SiteLanguage>())
        {
            if (await db.PageContents.AnyAsync(p => p.Language == language, cancellationToken))
            {
                continue;
            }

            foreach (var key in Enum.GetValues<PageKey>())
            {
                db.PageContents.Add(PageContent.Create(key, DefaultContent.For(key, language), now, language));
            }

            var order = 0;
            foreach (var (title, summary, icon) in DefaultContent.ServicesFor(language))
            {
                db.Services.Add(Service.Create(title, summary, null, icon, order++, isPublished: true, now, language));
            }

            foreach (var (kind, fields) in DefaultContent.BlocksFor(language))
            {
                db.ContentBlocks.Add(ContentBlock.Create(kind, fields, now, language));
            }

            if (!await db.CaseStudies.AnyAsync(c => c.Language == language, cancellationToken))
            {
                db.CaseStudies.Add(CaseStudy.Create(language, DefaultContent.SampleCaseStudy(language), now));
            }

            LogLanguageSeeded(logger, language);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static void ThrowIfFailed(IdentityResult result, string action)
    {
        if (!result.Succeeded)
        {
            var reasons = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Seeding failed to {action}: {reasons}");
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying database migrations")]
    private static partial void LogApplyingMigrations(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No initial admin configured (Seed:AdminEmail). Skipping admin seeding.")]
    private static partial void LogNoAdminConfigured(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Admin {Email} does not exist and Seed:AdminPassword is not set. Skipping admin creation.")]
    private static partial void LogAdminPasswordMissing(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Created initial admin user {Email}")]
    private static partial void LogAdminCreated(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded starter pages, services and home page blocks for {Language}")]
    private static partial void LogLanguageSeeded(ILogger logger, SiteLanguage language);
}
