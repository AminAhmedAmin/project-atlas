using Atlas.Application.Abstractions;
using Atlas.Application.Settings;
using Atlas.Application.Users;
using Atlas.Infrastructure.Email;
using Atlas.Infrastructure.Files;
using Atlas.Infrastructure.Identity;
using Atlas.Infrastructure.Persistence;
using Atlas.Infrastructure.Persistence.Repositories;
using Atlas.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Atlas.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "DefaultConnection";

    /// <summary>Tag for health checks that verify external dependencies (used by /health).</summary>
    public const string ReadinessTag = "ready";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.AddSingleton(configuration.GetSection(BrandingDefaults.SectionName).Get<BrandingDefaults>() ?? new BrandingDefaults());

        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured. Set ConnectionStrings__{ConnectionStringName}.");

        services.AddDbContext<AtlasDbContext>(options =>
        {
            switch (databaseOptions.Provider)
            {
                case DatabaseProvider.Sqlite:
                    options.UseSqlite(connectionString);
                    break;
                default:
                    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
                    break;
            }
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AtlasDbContext>());
        services.AddScoped<ISiteSettingsRepository, SiteSettingsRepository>();
        services.AddScoped<IPageContentRepository, PageContentRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IContactMessageRepository, ContactMessageRepository>();
        services.AddScoped<IContentBlockRepository, ContentBlockRepository>();
        services.AddScoped<ICaseStudyRepository, CaseStudyRepository>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 12;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AtlasDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();
        services.AddScoped<DatabaseSeeder>();

        services.AddHealthChecks()
            .AddDbContextCheck<AtlasDbContext>("database", tags: [ReadinessTag]);

        return services;
    }

    /// <summary>Runs <see cref="DatabaseSeeder"/> in its own scope.</summary>
    public static async Task SeedDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync(cancellationToken);
    }

    /// <summary>Full path of the uploads folder, for serving files from the web host.</summary>
    public static string GetUploadsRootPath(this IConfiguration configuration, IHostEnvironment environment)
    {
        var options = configuration.GetSection(FileStorageOptions.SectionName).Get<FileStorageOptions>() ?? new FileStorageOptions();
        return LocalFileStorage.ResolveRoot(options, environment);
    }
}
