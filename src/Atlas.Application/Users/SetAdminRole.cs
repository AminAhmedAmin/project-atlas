using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Users;

/// <param name="CurrentUserId">The admin performing the change.</param>
public sealed record SetAdminRoleCommand(string UserId, bool IsAdmin, string CurrentUserId);

public sealed partial class SetAdminRoleHandler(IUserAdminService users, ILogger<SetAdminRoleHandler> logger)
    : ICommandHandler<SetAdminRoleCommand>
{
    public async Task<Result> HandleAsync(SetAdminRoleCommand command, CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("User"));
        }

        if (user.IsAdmin == command.IsAdmin)
        {
            return Result.Success();
        }

        if (!command.IsAdmin)
        {
            if (string.Equals(command.UserId, command.CurrentUserId, StringComparison.Ordinal))
            {
                return Result.Failure(new Error("Users.SelfDemotion", "You cannot remove your own Admin role.", ErrorType.Forbidden));
            }

            if (await users.CountAdminsAsync(cancellationToken) <= 1)
            {
                return Result.Failure(new Error("Users.LastAdmin", "At least one admin must remain.", ErrorType.Conflict));
            }
        }

        var result = await users.SetAdminAsync(command.UserId, command.IsAdmin, cancellationToken);
        if (result.IsSuccess)
        {
            LogRoleChanged(logger, command.UserId, command.IsAdmin, command.CurrentUserId);
        }

        return result;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Admin role for user {UserId} set to {IsAdmin} by {ChangedBy}")]
    private static partial void LogRoleChanged(ILogger logger, string userId, bool isAdmin, string changedBy);
}
