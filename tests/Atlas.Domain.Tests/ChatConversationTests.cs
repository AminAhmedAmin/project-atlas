using Atlas.Domain.Chat;
using Atlas.Domain.Common;

namespace Atlas.Domain.Tests;

public sealed class ChatConversationTests
{
    private static readonly string Token = new('a', ChatConversation.AccessTokenLength);

    private static ChatConversation Start() =>
        ChatConversation.Start(" Sara ", "sara@example.com", SiteLanguage.Arabic, "Hello", Token, TestTime.Now);

    [Fact]
    public void Start_adds_the_first_visitor_message()
    {
        var chat = Start();

        Assert.Equal("Sara", chat.VisitorName);
        var message = Assert.Single(chat.Messages);
        Assert.Equal(ChatSender.Visitor, message.Sender);
        Assert.Equal(chat.Id, message.ConversationId);
        Assert.Equal(1, chat.UnreadByAgent);
        Assert.Equal(TestTime.Now, chat.LastMessageAtUtc);
    }

    [Fact]
    public void Token_is_checked()
    {
        var chat = Start();

        Assert.True(chat.HasAccessToken(Token));
        Assert.False(chat.HasAccessToken(new string('b', ChatConversation.AccessTokenLength)));
        Assert.False(chat.HasAccessToken("short"));
        Assert.False(chat.HasAccessToken(null));
        Assert.Throws<DomainException>(() => ChatConversation.Start("A", null, SiteLanguage.English, "Hi", "short", TestTime.Now));
    }

    [Fact]
    public void Agent_reply_clears_unread_and_visitor_message_reopens()
    {
        var chat = Start();
        chat.AddVisitorMessage("Are you there?", TestTime.Now.AddMinutes(1));
        Assert.Equal(2, chat.UnreadByAgent);

        chat.AddAgentReply("admin@example.com", "Yes, how can we help?", TestTime.Now.AddMinutes(2));
        Assert.Equal(0, chat.UnreadByAgent);

        chat.Close();
        Assert.Throws<DomainException>(() => chat.AddAgentReply("admin@example.com", "Hi", TestTime.Now));

        chat.AddVisitorMessage("One more question", TestTime.Now.AddMinutes(3));
        Assert.False(chat.IsClosed);
        Assert.Equal(4, chat.Messages.Count);
    }

    [Fact]
    public void Messages_are_validated_and_limited()
    {
        var chat = Start();

        Assert.Throws<DomainException>(() => chat.AddVisitorMessage(" ", TestTime.Now));
        Assert.Throws<DomainException>(() => chat.AddVisitorMessage(new string('x', ChatMessage.TextMaxLength + 1), TestTime.Now));

        for (var i = chat.Messages.Count; i < ChatConversation.MaxMessages; i++)
        {
            chat.AddVisitorMessage("x", TestTime.Now);
        }

        Assert.Throws<DomainException>(() => chat.AddVisitorMessage("too many", TestTime.Now));
    }
}
