using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Application.Settings;
using Atlas.Domain.Common;
using Atlas.Domain.Settings;

namespace Atlas.Application.Notifications;

public sealed record TelegramStatusDto(bool IsBotConfigured, long? ChatId, string? ChatTitle)
{
    public bool IsConnected => IsBotConfigured && ChatId is not null;
}

public sealed record GetTelegramStatusQuery;

public sealed class GetTelegramStatusHandler(ISiteSettingsRepository repository, ITelegramGateway telegram)
    : IQueryHandler<GetTelegramStatusQuery, TelegramStatusDto>
{
    public async Task<TelegramStatusDto> HandleAsync(GetTelegramStatusQuery query, CancellationToken cancellationToken = default)
    {
        var settings = await repository.GetAsync(cancellationToken);
        return new TelegramStatusDto(telegram.IsConfigured, settings?.TelegramChatId, settings?.TelegramChatTitle);
    }
}

/// <summary>Chats that recently messaged the bot, for the "connect" step in the dashboard.</summary>
public sealed record FindTelegramChatsQuery;

public sealed class FindTelegramChatsHandler(ITelegramGateway telegram)
    : IQueryHandler<FindTelegramChatsQuery, Result<IReadOnlyList<TelegramChat>>>
{
    public async Task<Result<IReadOnlyList<TelegramChat>>> HandleAsync(FindTelegramChatsQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            return Result.Success(await telegram.GetRecentChatsAsync(cancellationToken));
        }
        catch (AlertDeliveryException ex)
        {
            return Result.Failure<IReadOnlyList<TelegramChat>>(new Error("Telegram", ex.Message));
        }
    }
}

/// <summary>Connects a chat after proving delivery with a confirmation message.</summary>
public sealed record ConnectTelegramCommand(long ChatId, string? ChatTitle);

public sealed class ConnectTelegramHandler(
    ISiteSettingsRepository repository,
    IUnitOfWork unitOfWork,
    ITelegramGateway telegram,
    BrandingDefaults defaults,
    TimeProvider timeProvider) : ICommandHandler<ConnectTelegramCommand>
{
    public async Task<Result> HandleAsync(ConnectTelegramCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ChatId == 0)
        {
            return Result.Failure(Error.Validation(nameof(command.ChatId), "Choose a Telegram chat."));
        }

        try
        {
            await telegram.SendAlertAsync(
                command.ChatId,
                new TeamAlert("✅ Website alerts connected", [], "New contact messages and live chats will be posted here."),
                cancellationToken);
        }
        catch (AlertDeliveryException ex)
        {
            return Result.Failure(new Error("Telegram", ex.Message));
        }

        var now = timeProvider.UtcNow();
        var settings = await repository.GetAsync(cancellationToken);
        if (settings is null)
        {
            settings = SiteSettings.Create(defaults.CompanyName, defaults.Tagline, HexColor.Create(defaults.PrimaryColor), null, now);
            repository.Add(settings);
        }

        settings.ConnectTelegram(command.ChatId, command.ChatTitle, now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed record DisconnectTelegramCommand;

public sealed class DisconnectTelegramHandler(ISiteSettingsRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : ICommandHandler<DisconnectTelegramCommand>
{
    public async Task<Result> HandleAsync(DisconnectTelegramCommand command, CancellationToken cancellationToken = default)
    {
        var settings = await repository.GetAsync(cancellationToken);
        if (settings?.TelegramChatId is not null)
        {
            settings.DisconnectTelegram(timeProvider.UtcNow());
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}

public sealed record SendTestAlertCommand;

public sealed class SendTestAlertHandler(ISiteSettingsRepository repository, ITelegramGateway telegram)
    : ICommandHandler<SendTestAlertCommand>
{
    public async Task<Result> HandleAsync(SendTestAlertCommand command, CancellationToken cancellationToken = default)
    {
        var chatId = (await repository.GetAsync(cancellationToken))?.TelegramChatId;
        if (chatId is null)
        {
            return Result.Failure(new Error("Telegram", "No Telegram chat is connected yet."));
        }

        try
        {
            await telegram.SendAlertAsync(chatId.Value, new TeamAlert("🔔 Test alert", [], "Telegram alerts from your website are working."), cancellationToken);
            return Result.Success();
        }
        catch (AlertDeliveryException ex)
        {
            return Result.Failure(new Error("Telegram", ex.Message));
        }
    }
}
