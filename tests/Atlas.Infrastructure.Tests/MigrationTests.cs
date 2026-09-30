using Atlas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Tests;

public sealed class MigrationTests
{
    [Fact]
    public void Model_has_no_changes_missing_from_migrations()
    {
        // No connection is opened; the SQL Server provider is only needed to build the model.
        var options = new DbContextOptionsBuilder<AtlasDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options;
        using var db = new AtlasDbContext(options);

        Assert.False(
            db.Database.HasPendingModelChanges(),
            "The EF model changed without a migration. Run: dotnet ef migrations add <Name> --project src/Atlas.Infrastructure --startup-project src/Atlas.Web");
    }
}
