using Atlas.Domain.Chat;
using Atlas.Domain.Common;

namespace Atlas.Application.Chat;

public sealed record ChatMessageDto(Guid Id, ChatSender Sender, string? AgentName, string Text, DateTime SentAtUtc);

/// <summary>A conversation as seen by the team in the dashboard.</summary>
public sealed record ChatConversationDto(
    Guid Id,
    string VisitorName,
    string? VisitorContact,
    SiteLanguage Language,
    DateTime StartedAtUtc,
    DateTime LastMessageAtUtc,
    bool IsClosed,
    int UnreadByAgent,
    IReadOnlyList<ChatMessageDto> Messages)
{
    public string LastMessagePreview => Messages.Count == 0 ? string.Empty : Messages[^1].Text;
}

/// <summary>A conversation as seen by the visitor: no internal details such as agent names.</summary>
public sealed record VisitorChatDto(Guid Id, bool IsClosed, IReadOnlyList<VisitorChatMessageDto> Messages);

public sealed record VisitorChatMessageDto(Guid Id, bool FromVisitor, string Text, DateTime SentAtUtc);

public sealed record StartChatResult(Guid ConversationId, string AccessToken);

internal static class ChatMapping
{
    public static ChatConversationDto ToDto(this ChatConversation conversation) => new(
        conversation.Id,
        conversation.VisitorName,
        conversation.VisitorContact,
        conversation.Language,
        conversation.StartedAtUtc,
        conversation.LastMessageAtUtc,
        conversation.IsClosed,
        conversation.UnreadByAgent,
        conversation.Messages
            .OrderBy(m => m.SentAtUtc)
            .Select(m => new ChatMessageDto(m.Id, m.Sender, m.AgentName, m.Text, m.SentAtUtc))
            .ToList());

    public static VisitorChatDto ToVisitorDto(this ChatConversation conversation) => new(
        conversation.Id,
        conversation.IsClosed,
        conversation.Messages
            .OrderBy(m => m.SentAtUtc)
            .Select(m => new VisitorChatMessageDto(m.Id, m.Sender == ChatSender.Visitor, m.Text, m.SentAtUtc))
            .ToList());
}
