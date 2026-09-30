using Atlas.Application.Abstractions;

namespace Atlas.Application.Users;

public sealed record GetUsersQuery;

public sealed class GetUsersHandler(IUserAdminService users) : IQueryHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    public async Task<IReadOnlyList<UserDto>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        var list = await users.GetUsersAsync(cancellationToken);
        return list.OrderBy(u => u.Email, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
