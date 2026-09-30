using Atlas.Application.Common;

namespace Atlas.Application.Users;

public sealed record UserDto(string Id, string Email, bool IsAdmin, bool EmailConfirmed, bool IsLockedOut);

/// <summary>User and role management, implemented over ASP.NET Core Identity.</summary>
public interface IUserAdminService
{
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<UserDto?> FindByIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<int> CountAdminsAsync(CancellationToken cancellationToken = default);

    Task<Result<string>> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<Result> SetAdminAsync(string userId, bool isAdmin, CancellationToken cancellationToken = default);
}
