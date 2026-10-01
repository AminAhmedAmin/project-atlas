using Atlas.Application.Common;
using Atlas.Application.Tests.Fakes;
using Atlas.Application.Users;
using Microsoft.Extensions.Logging.Abstractions;

namespace Atlas.Application.Tests;

public sealed class UserHandlerTests
{
    private readonly FakeUserAdminService _users = new();

    private SetAdminRoleHandler Handler() => new(_users, NullLogger<SetAdminRoleHandler>.Instance);

    [Fact]
    public async Task Admin_can_promote_and_demote_others()
    {
        _users.Users.Add(new UserDto("me", "me@example.com", true, true, false));
        _users.Users.Add(new UserDto("other", "other@example.com", false, true, false));

        var promote = await Handler().HandleAsync(new SetAdminRoleCommand("other", true, "me"), TestContext.Current.CancellationToken);
        var demote = await Handler().HandleAsync(new SetAdminRoleCommand("other", false, "me"), TestContext.Current.CancellationToken);

        Assert.True(promote.IsSuccess);
        Assert.True(demote.IsSuccess);
        Assert.False(_users.Users[1].IsAdmin);
    }

    [Fact]
    public async Task Admin_cannot_demote_self()
    {
        _users.Users.Add(new UserDto("me", "me@example.com", true, true, false));
        _users.Users.Add(new UserDto("other", "other@example.com", true, true, false));

        var result = await Handler().HandleAsync(new SetAdminRoleCommand("me", false, "me"), TestContext.Current.CancellationToken);

        Assert.Equal(ErrorType.Forbidden, Assert.Single(result.Errors).Type);
        Assert.True(_users.Users[0].IsAdmin);
    }

    [Fact]
    public async Task Last_admin_cannot_be_demoted()
    {
        _users.Users.Add(new UserDto("admin", "admin@example.com", true, true, false));
        _users.Users.Add(new UserDto("me", "me@example.com", false, true, false));

        var result = await Handler().HandleAsync(new SetAdminRoleCommand("admin", false, "me"), TestContext.Current.CancellationToken);

        Assert.Equal(ErrorType.Conflict, Assert.Single(result.Errors).Type);
    }

    [Fact]
    public async Task Unknown_user_returns_not_found()
    {
        var result = await Handler().HandleAsync(new SetAdminRoleCommand("ghost", true, "me"), TestContext.Current.CancellationToken);

        Assert.Equal(ErrorType.NotFound, Assert.Single(result.Errors).Type);
    }

    [Fact]
    public async Task Create_user_optionally_grants_admin()
    {
        var handler = new CreateUserHandler(_users, new CreateUserValidator());

        var result = await handler.HandleAsync(new CreateUserCommand(" new@example.com ", "a-long-password", IsAdmin: true), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var user = Assert.Single(_users.Users);
        Assert.Equal("new@example.com", user.Email);
        Assert.True(user.IsAdmin);
    }
}
