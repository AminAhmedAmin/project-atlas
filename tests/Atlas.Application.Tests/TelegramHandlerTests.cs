using Atlas.Application.Abstractions;
using Atlas.Application.Notifications;
using Atlas.Application.Settings;
using Atlas.Application.Tests.Fakes;
using Atlas.Domain.Common;
using Atlas.Domain.Settings;
using Microsoft.Extensions.Logging.Abstractions;

namespace Atlas.Application.Tests;

public sealed class TelegramHandlerTests
{
    private readonly InMemorySiteSettingsRepository _settings = new();
    private readonly FakeTelegramGateway _telegram = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ConnectTelegramHandler Connect() =>
        new(_settings, _unitOfWork, _telegram, new BrandingDefaults(), TestData.Clock());

    private TeamNotifier Notifier() => new(_settings, _telegram, NullLogger<TeamNotifier>.Instance);

    private static TeamAlert Alert => new("New message", [new("Name", "Sara")], "Hello");

    [Fact]
    public async Task Connect_sends_a_confirmation_and_saves_the_chat()
    {
        var result = await Connect().HandleAsync(new ConnectTelegramCommand(42, "Sales team"), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, Assert.Single(_telegram.Sent).ChatId);
        Assert.Equal(42, _settings.Settings?.TelegramChatId);
        Assert.Equal("Sales team", _settings.Settings?.TelegramChatTitle);
    }

    [Fact]
    public async Task Connect_does_not_save_when_telegram_rejects_the_chat()
    {
        _telegram.FailWith = "Telegram could not find this chat.";

        var result = await Connect().HandleAsync(new ConnectTelegramCommand(42, null), TestContext.Current.CancellationToken);

        Assert.Equal("Telegram could not find this chat.", result.ErrorMessage);
        Assert.Null(_settings.Settings?.TelegramChatId);
    }

    [Fact]
    public async Task Notifier_sends_to_the_connected_chat()
    {
        await Connect().HandleAsync(new ConnectTelegramCommand(42, null), TestContext.Current.CancellationToken);
        _telegram.Sent.Clear();

        await Notifier().NotifyAsync(Alert, TestContext.Current.CancellationToken);

        Assert.Equal("New message", Assert.Single(_telegram.Sent).Alert.Title);
    }

    [Fact]
    public async Task Notifier_does_nothing_without_a_chat_or_token_and_never_throws()
    {
        await Notifier().NotifyAsync(Alert, TestContext.Current.CancellationToken);
        Assert.Empty(_telegram.Sent);

        _settings.Settings = SiteSettings.Create("Co", null, HexColor.Create("#000000"), null, TestData.Now.UtcDateTime);
        _settings.Settings.ConnectTelegram(42, null, TestData.Now.UtcDateTime);
        _telegram.IsConfigured = false;
        await Notifier().NotifyAsync(Alert, TestContext.Current.CancellationToken);
        Assert.Empty(_telegram.Sent);

        _telegram.IsConfigured = true;
        _telegram.FailWith = "Telegram is down";
        await Notifier().NotifyAsync(Alert, TestContext.Current.CancellationToken); // must not throw
    }

    [Fact]
    public async Task Find_chats_reports_errors_as_results()
    {
        _telegram.Chats.Add(new TelegramChat(7, "Amin"));
        var ok = await new FindTelegramChatsHandler(_telegram).HandleAsync(new FindTelegramChatsQuery(), TestContext.Current.CancellationToken);
        Assert.Equal("Amin", Assert.Single(ok.Value).Title);

        _telegram.FailWith = "Telegram rejected the bot token.";
        var failed = await new FindTelegramChatsHandler(_telegram).HandleAsync(new FindTelegramChatsQuery(), TestContext.Current.CancellationToken);
        Assert.True(failed.IsFailure);
    }

    [Fact]
    public async Task Test_alert_needs_a_connected_chat_and_disconnect_clears_it()
    {
        var ct = TestContext.Current.CancellationToken;
        var test = new SendTestAlertHandler(_settings, _telegram);

        Assert.True((await test.HandleAsync(new SendTestAlertCommand(), ct)).IsFailure);

        await Connect().HandleAsync(new ConnectTelegramCommand(42, null), ct);
        Assert.True((await test.HandleAsync(new SendTestAlertCommand(), ct)).IsSuccess);

        await new DisconnectTelegramHandler(_settings, _unitOfWork, TestData.Clock()).HandleAsync(new DisconnectTelegramCommand(), ct);
        var status = await new GetTelegramStatusHandler(_settings, _telegram).HandleAsync(new GetTelegramStatusQuery(), ct);
        Assert.False(status.IsConnected);
        Assert.True(status.IsBotConfigured);
    }
}
