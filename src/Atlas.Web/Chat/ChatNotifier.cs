using Atlas.Application.Abstractions;

namespace Atlas.Web.Chat;

/// <summary>
/// In-process broadcast of chat changes to every connected circuit (dashboards and visitor widgets).
/// Works for a single server instance; scale-out would need a backplane such as Azure SignalR or Redis.
/// </summary>
public sealed class ChatNotifier : IChatNotifier
{
    public event Action<Guid>? Changed;

    public void ConversationChanged(Guid conversationId) => Changed?.Invoke(conversationId);
}
