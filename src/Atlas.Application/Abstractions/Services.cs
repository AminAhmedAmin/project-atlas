namespace Atlas.Application.Abstractions;

/// <summary>Stores publicly served files (e.g. the logo) and returns their site-relative URL.</summary>
public interface IFileStorage
{
    Task<string> SaveAsync(string folder, string fileName, Stream content, CancellationToken cancellationToken = default);

    Task DeleteAsync(string url, CancellationToken cancellationToken = default);
}

/// <summary>An alert for the team, e.g. "New contact message", with labelled details.</summary>
/// <param name="Title">Short headline.</param>
/// <param name="Fields">Labelled details such as name and phone; empty values are skipped.</param>
/// <param name="Body">Longer free text, e.g. the visitor's message.</param>
/// <param name="DashboardPath">Site-relative dashboard link, e.g. "/admin/chat".</param>
public sealed record TeamAlert(
    string Title,
    IReadOnlyList<KeyValuePair<string, string?>> Fields,
    string? Body = null,
    string? DashboardPath = null);

/// <summary>Sends alerts to the team (currently through Telegram). Never throws: failures are logged.</summary>
public interface ITeamNotifier
{
    Task NotifyAsync(TeamAlert alert, CancellationToken cancellationToken = default);
}

/// <summary>A Telegram chat (person, group or channel) that has talked to the bot.</summary>
public sealed record TelegramChat(long Id, string Title);

/// <summary>Raised when an alert could not be delivered; the message is safe to show to admins.</summary>
public sealed class AlertDeliveryException : Exception
{
    public AlertDeliveryException(string message)
        : base(message)
    {
    }

    public AlertDeliveryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public AlertDeliveryException()
    {
    }
}

/// <summary>Access to the Telegram Bot API.</summary>
public interface ITelegramGateway
{
    /// <summary>True when a bot token is configured.</summary>
    bool IsConfigured { get; }

    /// <summary>Chats that recently sent a message to the bot (used to connect a chat from the dashboard).</summary>
    /// <exception cref="AlertDeliveryException">The bot is not configured or Telegram rejected the request.</exception>
    Task<IReadOnlyList<TelegramChat>> GetRecentChatsAsync(CancellationToken cancellationToken = default);

    /// <exception cref="AlertDeliveryException">The bot is not configured or Telegram rejected the message.</exception>
    Task SendAlertAsync(long chatId, TeamAlert alert, CancellationToken cancellationToken = default);
}

/// <summary>Tells connected dashboards and visitor chat windows that a conversation changed.</summary>
public interface IChatNotifier
{
    void ConversationChanged(Guid conversationId);
}
