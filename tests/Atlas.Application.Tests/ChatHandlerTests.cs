using Atlas.Application.Chat;
using Atlas.Application.Common;
using Atlas.Application.Tests.Fakes;
using Atlas.Domain.Common;
using Microsoft.Extensions.Logging.Abstractions;

namespace Atlas.Application.Tests;

public sealed class ChatHandlerTests
{
    private readonly InMemoryChatRepository _chats = new();
    private readonly RecordingChatNotifier _notifier = new();
    private readonly RecordingTeamNotifier _alerts = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private async Task<StartChatResult> StartAsync(string message = "Hello")
    {
        var handler = new StartChatHandler(_chats, _unitOfWork, _notifier, _alerts, new StartChatValidator(), TestData.Clock(), NullLogger<StartChatHandler>.Instance);
        var result = await handler.HandleAsync(new StartChatCommand("Sara", "0551234567", SiteLanguage.Arabic, message), TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value;
    }

    [Fact]
    public async Task Start_creates_conversation_with_secret_token_and_notifies()
    {
        var started = await StartAsync();

        Assert.Equal(48, started.AccessToken.Length);
        Assert.Equal([started.ConversationId], _notifier.Notifications);
        var alert = Assert.Single(_alerts.Alerts);
        Assert.Equal("Hello", alert.Body);
        Assert.Equal("/admin/chat", alert.DashboardPath);
        Assert.Equal(1, await new GetUnreadChatCountHandler(_chats).HandleAsync(new(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Tokens_are_unique_per_conversation()
    {
        var first = await StartAsync();
        var second = await StartAsync();

        Assert.NotEqual(first.AccessToken, second.AccessToken);
    }

    [Fact]
    public async Task Visitor_needs_the_right_token()
    {
        var ct = TestContext.Current.CancellationToken;
        var started = await StartAsync();
        var send = new SendVisitorChatMessageHandler(_chats, _unitOfWork, _notifier, _alerts, TestData.Clock());
        var get = new GetVisitorChatHandler(_chats);

        var wrongToken = new string('0', 48);
        Assert.True((await send.HandleAsync(new SendVisitorChatMessageCommand(started.ConversationId, wrongToken, "hi"), ct)).IsFailure);
        Assert.Null(await get.HandleAsync(new GetVisitorChatQuery(started.ConversationId, wrongToken), ct));

        Assert.True((await send.HandleAsync(new SendVisitorChatMessageCommand(started.ConversationId, started.AccessToken, "Still there?"), ct)).IsSuccess);
        var chat = await get.HandleAsync(new GetVisitorChatQuery(started.ConversationId, started.AccessToken), ct);
        Assert.Equal(["Hello", "Still there?"], chat!.Messages.Select(m => m.Text));
    }

    [Fact]
    public async Task Agent_reply_is_visible_to_visitor_without_agent_name()
    {
        var ct = TestContext.Current.CancellationToken;
        var started = await StartAsync();

        var reply = await new ReplyToChatHandler(_chats, _unitOfWork, _notifier, TestData.Clock())
            .HandleAsync(new ReplyToChatCommand(started.ConversationId, "admin@example.com", "Welcome!"), ct);
        Assert.True(reply.IsSuccess);

        var visitorView = await new GetVisitorChatHandler(_chats).HandleAsync(new GetVisitorChatQuery(started.ConversationId, started.AccessToken), ct);
        var last = visitorView!.Messages[^1];
        Assert.False(last.FromVisitor);
        Assert.Equal("Welcome!", last.Text);

        var adminView = await new GetChatConversationHandler(_chats).HandleAsync(new GetChatConversationQuery(started.ConversationId), ct);
        Assert.Equal("admin@example.com", adminView!.Messages[^1].AgentName);
        Assert.Equal(0, adminView.UnreadByAgent);
    }

    [Fact]
    public async Task Close_hides_from_open_list_and_delete_removes()
    {
        var ct = TestContext.Current.CancellationToken;
        var started = await StartAsync();
        var update = new UpdateChatHandler(_chats, _unitOfWork, _notifier);

        await update.HandleAsync(new UpdateChatCommand(started.ConversationId, ChatAction.Close), ct);
        var open = await new GetChatConversationsHandler(_chats).HandleAsync(new GetChatConversationsQuery(IncludeClosed: false), ct);
        Assert.Empty(open);

        var reply = await new ReplyToChatHandler(_chats, _unitOfWork, _notifier, TestData.Clock())
            .HandleAsync(new ReplyToChatCommand(started.ConversationId, "admin", "Hi"), ct);
        Assert.True(reply.IsFailure);

        await update.HandleAsync(new UpdateChatCommand(started.ConversationId, ChatAction.Delete), ct);
        Assert.Empty(_chats.Conversations);
    }

    [Fact]
    public async Task Start_requires_name_and_message()
    {
        var handler = new StartChatHandler(_chats, _unitOfWork, _notifier, _alerts, new StartChatValidator(), TestData.Clock(), NullLogger<StartChatHandler>.Instance);

        var result = await handler.HandleAsync(new StartChatCommand(" ", null, SiteLanguage.English, ""), TestContext.Current.CancellationToken);

        Assert.Equal(["Name", "Message"], result.Errors.Select(e => e.Field));
        Assert.Empty(_chats.Conversations);
    }

    [Fact]
    public async Task Follow_up_messages_alert_only_after_the_team_caught_up()
    {
        var ct = TestContext.Current.CancellationToken;
        var started = await StartAsync();
        var send = new SendVisitorChatMessageHandler(_chats, _unitOfWork, _notifier, _alerts, TestData.Clock());
        var reply = new ReplyToChatHandler(_chats, _unitOfWork, _notifier, TestData.Clock());

        await send.HandleAsync(new SendVisitorChatMessageCommand(started.ConversationId, started.AccessToken, "Anyone there?"), ct);
        Assert.Single(_alerts.Alerts); // still unread: no extra alert for a burst of messages

        await reply.HandleAsync(new ReplyToChatCommand(started.ConversationId, "admin", "Yes!"), ct);
        await send.HandleAsync(new SendVisitorChatMessageCommand(started.ConversationId, started.AccessToken, "Great, thanks"), ct);

        Assert.Equal(2, _alerts.Alerts.Count);
        Assert.Equal("Great, thanks", _alerts.Alerts[^1].Body);
    }
}
