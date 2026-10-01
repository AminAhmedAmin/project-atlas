using System.Security.Cryptography;
using System.Text;
using Atlas.Domain.Common;

namespace Atlas.Domain.Chat;

public enum ChatSender
{
    Visitor = 1,
    Agent = 2,
}

/// <summary>A live chat between a website visitor and the team.</summary>
public sealed class ChatConversation : Entity
{
    public const int VisitorNameMaxLength = 100;
    public const int VisitorContactMaxLength = 150;
    public const int AccessTokenLength = 48;

    /// <summary>Upper bound on messages per conversation, to limit abuse.</summary>
    public const int MaxMessages = 300;

    private readonly List<ChatMessage> _messages = [];

    private ChatConversation()
    {
        VisitorName = string.Empty;
        AccessToken = string.Empty;
    }

    public string VisitorName { get; private set; }

    /// <summary>Optional e-mail or phone number so the team can follow up.</summary>
    public string? VisitorContact { get; private set; }

    public SiteLanguage Language { get; private set; }

    /// <summary>Secret the visitor's browser presents to read and continue the conversation.</summary>
    public string AccessToken { get; private set; }

    public DateTime StartedAtUtc { get; private set; }

    public DateTime LastMessageAtUtc { get; private set; }

    public bool IsClosed { get; private set; }

    /// <summary>Visitor messages the team has not seen yet.</summary>
    public int UnreadByAgent { get; private set; }

    public IReadOnlyList<ChatMessage> Messages => _messages;

    public static ChatConversation Start(
        string visitorName,
        string? visitorContact,
        SiteLanguage language,
        string firstMessage,
        string accessToken,
        DateTime nowUtc)
    {
        if (string.IsNullOrEmpty(accessToken) || accessToken.Length != AccessTokenLength)
        {
            throw new DomainException("Invalid chat access token.");
        }

        var conversation = new ChatConversation
        {
            VisitorName = Guard.Required(visitorName, "Name", VisitorNameMaxLength),
            VisitorContact = Guard.Optional(visitorContact, "Contact", VisitorContactMaxLength),
            Language = Languages.Ensure(language),
            AccessToken = accessToken,
            StartedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc)),
        };
        conversation.AddVisitorMessage(firstMessage, nowUtc);
        return conversation;
    }

    /// <summary>Compares the token in constant time.</summary>
    public bool HasAccessToken(string? token) =>
        token is not null
        && CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(token), Encoding.ASCII.GetBytes(AccessToken));

    /// <summary>Adds a visitor message. Writing to a closed conversation reopens it.</summary>
    public ChatMessage AddVisitorMessage(string text, DateTime nowUtc)
    {
        var message = Add(ChatSender.Visitor, null, text, nowUtc);
        IsClosed = false;
        UnreadByAgent++;
        return message;
    }

    public ChatMessage AddAgentReply(string agentName, string text, DateTime nowUtc)
    {
        if (IsClosed)
        {
            throw new DomainException("This conversation is closed.");
        }

        var message = Add(ChatSender.Agent, Guard.Required(agentName, "Agent", VisitorNameMaxLength), text, nowUtc);
        UnreadByAgent = 0;
        return message;
    }

    public void MarkReadByAgent() => UnreadByAgent = 0;

    public void Close()
    {
        IsClosed = true;
        UnreadByAgent = 0;
    }

    private ChatMessage Add(ChatSender sender, string? agentName, string text, DateTime nowUtc)
    {
        if (_messages.Count >= MaxMessages)
        {
            throw new DomainException("This conversation has reached its message limit. Please use the contact form.");
        }

        var message = ChatMessage.Create(Id, sender, agentName, text, nowUtc);
        _messages.Add(message);
        LastMessageAtUtc = message.SentAtUtc;
        return message;
    }
}

public sealed class ChatMessage : Entity
{
    public const int TextMaxLength = 1_000;

    private ChatMessage()
    {
        Text = string.Empty;
    }

    public Guid ConversationId { get; private set; }

    public ChatSender Sender { get; private set; }

    /// <summary>Who replied (team member's user name); never shown to visitors.</summary>
    public string? AgentName { get; private set; }

    public string Text { get; private set; }

    public DateTime SentAtUtc { get; private set; }

    internal static ChatMessage Create(Guid conversationId, ChatSender sender, string? agentName, string text, DateTime nowUtc) => new()
    {
        ConversationId = conversationId,
        Sender = sender,
        AgentName = agentName,
        Text = Guard.Required(text, "Message", TextMaxLength),
        SentAtUtc = Guard.Utc(nowUtc, nameof(nowUtc)),
    };
}
