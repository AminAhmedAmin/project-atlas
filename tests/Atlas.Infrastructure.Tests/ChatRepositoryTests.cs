using Atlas.Application.Abstractions;
using Atlas.Application.Chat;
using Atlas.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Infrastructure.Tests;

public sealed class ChatRepositoryTests
{
    [Fact]
    public async Task Conversation_and_messages_round_trip_through_the_database()
    {
        await using var host = await SqliteTestHost.CreateAsync();
        var ct = TestContext.Current.CancellationToken;

        StartChatResult started;
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var start = scope.ServiceProvider.GetRequiredService<ICommandHandler<StartChatCommand, StartChatResult>>();
            started = (await start.HandleAsync(new StartChatCommand("Sara", null, SiteLanguage.English, "Hello"), ct)).Value;
        }

        // New messages added to an existing, tracked conversation must be inserted (not updated).
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var send = scope.ServiceProvider.GetRequiredService<ICommandHandler<SendVisitorChatMessageCommand>>();
            Assert.True((await send.HandleAsync(new SendVisitorChatMessageCommand(started.ConversationId, started.AccessToken, "Second"), ct)).IsSuccess);
        }

        await using (var scope = host.Services.CreateAsyncScope())
        {
            var reply = scope.ServiceProvider.GetRequiredService<ICommandHandler<ReplyToChatCommand>>();
            Assert.True((await reply.HandleAsync(new ReplyToChatCommand(started.ConversationId, "admin@example.com", "Hi Sara"), ct)).IsSuccess);
        }

        await using (var scope = host.Services.CreateAsyncScope())
        {
            var list = await scope.ServiceProvider.GetRequiredService<IQueryHandler<GetChatConversationsQuery, IReadOnlyList<ChatConversationDto>>>()
                .HandleAsync(new GetChatConversationsQuery(IncludeClosed: false), ct);
            var conversation = Assert.Single(list);
            Assert.Equal(["Hello", "Second", "Hi Sara"], conversation.Messages.Select(m => m.Text));
            Assert.Equal(0, conversation.UnreadByAgent);

            var update = scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdateChatCommand>>();
            Assert.True((await update.HandleAsync(new UpdateChatCommand(started.ConversationId, ChatAction.Delete), ct)).IsSuccess);
        }

        await using (var scope = host.Services.CreateAsyncScope())
        {
            var list = await scope.ServiceProvider.GetRequiredService<IQueryHandler<GetChatConversationsQuery, IReadOnlyList<ChatConversationDto>>>()
                .HandleAsync(new GetChatConversationsQuery(IncludeClosed: true), ct);
            Assert.Empty(list);
        }
    }
}
