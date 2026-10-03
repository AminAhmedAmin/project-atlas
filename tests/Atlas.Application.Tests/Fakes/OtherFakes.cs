using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Application.Users;

namespace Atlas.Application.Tests.Fakes;

internal sealed class RecordingTeamNotifier : ITeamNotifier
{
    public List<TeamAlert> Alerts { get; } = [];

    public Task NotifyAsync(TeamAlert alert, CancellationToken cancellationToken = default)
    {
        Alerts.Add(alert);
        return Task.CompletedTask;
    }
}

internal sealed class FakeTelegramGateway : ITelegramGateway
{
    public bool IsConfigured { get; set; } = true;

    /// <summary>When set, every call throws this delivery error.</summary>
    public string? FailWith { get; set; }

    public List<TelegramChat> Chats { get; } = [];

    public List<(long ChatId, TeamAlert Alert)> Sent { get; } = [];

    public Task<IReadOnlyList<TelegramChat>> GetRecentChatsAsync(CancellationToken cancellationToken = default) =>
        FailWith is null ? Task.FromResult<IReadOnlyList<TelegramChat>>(Chats) : throw new AlertDeliveryException(FailWith);

    public Task SendAlertAsync(long chatId, TeamAlert alert, CancellationToken cancellationToken = default)
    {
        if (FailWith is not null)
        {
            throw new AlertDeliveryException(FailWith);
        }

        Sent.Add((chatId, alert));
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryFileStorage : IFileStorage
{
    public Dictionary<string, byte[]> Files { get; } = [];

    public async Task<string> SaveAsync(string folder, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var url = $"/uploads/{folder}/{fileName}";
        Files[url] = buffer.ToArray();
        return url;
    }

    public Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        Files.Remove(url);
        return Task.CompletedTask;
    }
}

internal sealed class FakeUserAdminService : IUserAdminService
{
    public List<UserDto> Users { get; } = [];

    public Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<UserDto>>(Users.ToList());

    public Task<UserDto?> FindByIdAsync(string userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Id == userId));

    public Task<int> CountAdminsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.Count(u => u.IsAdmin));

    public Task<Result<string>> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString();
        Users.Add(new UserDto(id, email, false, true, false));
        return Task.FromResult(Result.Success(id));
    }

    public Task<Result> SetAdminAsync(string userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var index = Users.FindIndex(u => u.Id == userId);
        Users[index] = Users[index] with { IsAdmin = isAdmin };
        return Task.FromResult(Result.Success());
    }
}
