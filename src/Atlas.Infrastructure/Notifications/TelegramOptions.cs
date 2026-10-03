namespace Atlas.Infrastructure.Notifications;

/// <summary>
/// Telegram bot settings. The bot token is a secret: set it with user-secrets or the
/// Telegram__BotToken environment variable, never in a committed file.
/// </summary>
public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    /// <summary>Token from @BotFather, e.g. "123456789:AA...". Alerts are off while empty.</summary>
    public string? BotToken { get; set; }

    /// <summary>Public address of the website (e.g. https://example.com), used for "Open dashboard" links.</summary>
    public string? SiteUrl { get; set; }

    /// <summary>Bot API base address. Only changed for testing.</summary>
    public string ApiBaseUrl { get; set; } = "https://api.telegram.org";
}
