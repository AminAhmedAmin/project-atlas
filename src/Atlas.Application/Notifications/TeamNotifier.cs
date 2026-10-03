using Atlas.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Notifications;

/// <summary>Delivers team alerts to the Telegram chat connected in the dashboard, if any.</summary>
public sealed partial class TeamNotifier(
    ISiteSettingsRepository settingsRepository,
    ITelegramGateway telegram,
    ILogger<TeamNotifier> logger) : ITeamNotifier
{
    public async Task NotifyAsync(TeamAlert alert, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alert);

        try
        {
            var chatId = (await settingsRepository.GetAsync(cancellationToken))?.TelegramChatId;
            if (chatId is null || !telegram.IsConfigured)
            {
                LogNotConfigured(logger, alert.Title);
                return;
            }

            await telegram.SendAlertAsync(chatId.Value, alert, cancellationToken);
        }
#pragma warning disable CA1031 // Alerts are best effort: the visitor's message is already saved.
        catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
        {
            LogFailed(logger, alert.Title, ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Team alert '{Title}' not sent: no Telegram chat connected")]
    private static partial void LogNotConfigured(ILogger logger, string title);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Team alert '{Title}' could not be delivered")]
    private static partial void LogFailed(ILogger logger, string title, Exception exception);
}
