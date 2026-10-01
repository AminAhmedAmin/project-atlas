using Atlas.Application.Abstractions;
using Atlas.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Infrastructure.Tests;

public sealed class UserAdminServiceTests
{
    [Fact]
    public async Task Create_list_and_toggle_admin()
    {
        await using var host = await SqliteTestHost.CreateAsync();
        var ct = TestContext.Current.CancellationToken;
        await host.Services.SeedDatabaseAsync(ct);

        await using var scope = host.Services.CreateAsyncScope();
        var create = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateUserCommand, string>>();
        var service = scope.ServiceProvider.GetRequiredService<IUserAdminService>();

        var created = await create.HandleAsync(new CreateUserCommand("editor@example.com", "Long-enough-pass-1", IsAdmin: true), ct);
        Assert.True(created.IsSuccess, created.ErrorMessage);

        var weak = await create.HandleAsync(new CreateUserCommand("weak@example.com", "short", IsAdmin: false), ct);
        Assert.True(weak.IsFailure);

        var duplicate = await create.HandleAsync(new CreateUserCommand("editor@example.com", "Long-enough-pass-1", IsAdmin: false), ct);
        Assert.True(duplicate.IsFailure);

        Assert.True((await service.GetUsersAsync(ct)).Single().IsAdmin);
        Assert.True((await service.SetAdminAsync(created.Value, false, ct)).IsSuccess);
        Assert.Equal(0, await service.CountAdminsAsync(ct));
    }
}
