using Atlas.Application.Common;
using Atlas.Application.Security;
using Atlas.Application.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Identity;

internal sealed class UserAdminService(UserManager<ApplicationUser> userManager, TimeProvider timeProvider) : IUserAdminService
{
    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users.AsNoTracking().ToListAsync(cancellationToken);
        var adminIds = (await userManager.GetUsersInRoleAsync(AppRoles.Admin)).Select(u => u.Id).ToHashSet();
        return users.Select(u => ToDto(u, adminIds.Contains(u.Id))).ToList();
    }

    public async Task<UserDto?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user is null ? null : ToDto(user, await userManager.IsInRoleAsync(user, AppRoles.Admin));
    }

    public async Task<int> CountAdminsAsync(CancellationToken cancellationToken = default) =>
        (await userManager.GetUsersInRoleAsync(AppRoles.Admin)).Count;

    public async Task<Result<string>> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return Result.Failure<string>(new Error("Users.Duplicate", "A user with this e-mail already exists.", ErrorType.Conflict, "Email"));
        }

        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, password);
        return result.Succeeded ? Result.Success(user.Id) : Result.Failure<string>(ToErrors(result));
    }

    public async Task<Result> SetAdminAsync(string userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("User"));
        }

        var inRole = await userManager.IsInRoleAsync(user, AppRoles.Admin);
        if (inRole == isAdmin)
        {
            return Result.Success();
        }

        var result = isAdmin
            ? await userManager.AddToRoleAsync(user, AppRoles.Admin)
            : await userManager.RemoveFromRoleAsync(user, AppRoles.Admin);

        if (!result.Succeeded)
        {
            return Result.Failure(ToErrors(result));
        }

        // Invalidate existing cookies so the role change takes effect on next validation.
        await userManager.UpdateSecurityStampAsync(user);
        return Result.Success();
    }

    private UserDto ToDto(ApplicationUser user, bool isAdmin) => new(
        user.Id,
        user.Email ?? user.UserName ?? user.Id,
        isAdmin,
        user.EmailConfirmed,
        user.LockoutEnd is { } end && end > timeProvider.GetUtcNow());

    private static IEnumerable<Error> ToErrors(IdentityResult result) =>
        result.Errors.Select(e => new Error($"Identity.{e.Code}", e.Description, ErrorType.Validation, "Password"));
}
