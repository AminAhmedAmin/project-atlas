namespace Atlas.Infrastructure.Persistence;

public enum DatabaseProvider
{
    SqlServer,

    /// <summary>File-based database for quick local demos. Uses EnsureCreated instead of migrations.</summary>
    Sqlite,
}

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;

    /// <summary>Apply pending migrations at startup. Enable in Development only.</summary>
    public bool ApplyMigrationsOnStartup { get; set; }
}
