using Atlas.Application;
using Atlas.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Infrastructure.Tests;

/// <summary>Wires the real Infrastructure services against a private in-memory SQLite database.</summary>
internal sealed class SqliteTestHost : IAsyncDisposable
{
    private readonly SqliteConnection _keepAlive;

    private SqliteTestHost(ServiceProvider services, SqliteConnection keepAlive)
    {
        Services = services;
        _keepAlive = keepAlive;
    }

    public ServiceProvider Services { get; }

    public static async Task<SqliteTestHost> CreateAsync(IDictionary<string, string?>? settings = null)
    {
        var connectionString = $"Data Source=file:atlas-{Guid.NewGuid():N}?mode=memory&cache=shared";
        var keepAlive = new SqliteConnection(connectionString);
        await keepAlive.OpenAsync();

        var values = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = connectionString,
            ["Database:Provider"] = "Sqlite",
        };
        foreach (var (key, value) in settings ?? new Dictionary<string, string?>())
        {
            values[key] = value;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection()
            .AddLogging()
            .AddDataProtection().Services
            .AddApplication()
            .AddInfrastructure(configuration)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        await using (var scope = services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<AtlasDbContext>().Database.EnsureCreatedAsync();
        }

        return new SqliteTestHost(services, keepAlive);
    }

    public async ValueTask DisposeAsync()
    {
        await Services.DisposeAsync();
        await _keepAlive.DisposeAsync();
    }
}
