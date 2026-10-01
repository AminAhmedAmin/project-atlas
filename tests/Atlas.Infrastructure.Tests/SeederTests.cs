using Atlas.Application.Content;
using Atlas.Application.Security;
using Atlas.Domain.Common;
using Atlas.Domain.Content;
using Atlas.Infrastructure.Identity;
using Atlas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Infrastructure.Tests;

public sealed class SeederTests
{
    private static readonly Dictionary<string, string?> AdminSettings = new()
    {
        ["Seed:AdminEmail"] = "admin@example.com",
        ["Seed:AdminPassword"] = "Test-only-password-123",
        ["Branding:CompanyName"] = "Configured Co",
    };

    [Fact]
    public async Task Seeding_creates_admin_settings_and_content_and_is_idempotent()
    {
        await using var host = await SqliteTestHost.CreateAsync(AdminSettings);
        var ct = TestContext.Current.CancellationToken;

        await host.Services.SeedDatabaseAsync(ct);
        await host.Services.SeedDatabaseAsync(ct);

        await using var scope = host.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await users.FindByEmailAsync("admin@example.com");
        Assert.NotNull(admin);
        Assert.True(await users.IsInRoleAsync(admin, AppRoles.Admin));

        var db = scope.ServiceProvider.GetRequiredService<AtlasDbContext>();
        Assert.Equal("Configured Co", (await db.SiteSettings.SingleAsync(ct)).CompanyName);
        foreach (var language in Enum.GetValues<SiteLanguage>())
        {
            Assert.Equal(Enum.GetValues<PageKey>().Length, await db.PageContents.CountAsync(p => p.Language == language, ct));
            Assert.Equal(DefaultContent.ServicesFor(language).Count, await db.Services.CountAsync(s => s.Language == language, ct));
            Assert.Equal(DefaultContent.BlocksFor(language).Count, await db.ContentBlocks.CountAsync(b => b.Language == language, ct));
        }
        Assert.Equal(1, await db.Users.CountAsync(ct));
    }

    [Fact]
    public async Task Seeding_without_admin_configuration_still_seeds_content()
    {
        await using var host = await SqliteTestHost.CreateAsync();
        var ct = TestContext.Current.CancellationToken;

        await host.Services.SeedDatabaseAsync(ct);

        await using var scope = host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AtlasDbContext>();
        Assert.Equal(0, await db.Users.CountAsync(ct));
        Assert.True(await db.Roles.AnyAsync(r => r.Name == AppRoles.Admin, ct));
        Assert.True(await db.SiteSettings.AnyAsync(ct));
    }

    [Fact]
    public async Task Deleted_starter_services_are_not_recreated()
    {
        await using var host = await SqliteTestHost.CreateAsync();
        var ct = TestContext.Current.CancellationToken;
        await host.Services.SeedDatabaseAsync(ct);

        await using (var scope = host.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AtlasDbContext>();
            db.Services.RemoveRange(db.Services);
            await db.SaveChangesAsync(ct);
        }

        await host.Services.SeedDatabaseAsync(ct);

        await using (var scope = host.Services.CreateAsyncScope())
        {
            Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<AtlasDbContext>().Services.CountAsync(ct));
        }
    }
}
