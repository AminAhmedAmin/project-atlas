using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Chat;
using Atlas.Domain.Common;

namespace Atlas.Application.Chat;

public sealed record GetChatConversationsQuery(bool IncludeClosed, int Take = 100);

public sealed class GetChatConversationsHandler(IChatRepository repository)
    : IQueryHandler<GetChatConversationsQuery, IReadOnlyList<ChatConversationDto>>
{
    public async Task<IReadOnlyList<ChatConversationDto>> HandleAsync(GetChatConversationsQuery query, CancellationToken cancellationToken = default)
    {
        var conversations = await repository.ListAsync(query.IncludeClosed, Math.Clamp(query.Take, 1, 500), cancellationToken);
        return conversations.OrderByDescending(c => c.LastMessageAtUtc).Select(c => c.ToDto()).ToList();
    }
}

public sealed record GetChatConversationQuery(Guid ConversationId);

public sealed class GetChatConversationHandler(IChatRepository repository) : IQueryHandler<GetChatConversationQuery, ChatConversationDto?>
{
    public async Task<ChatConversationDto?> HandleAsync(GetChatConversationQuery query, CancellationToken cancellationToken = default) =>
        (await repository.GetAsync(query.ConversationId, cancellationToken))?.ToDto();
}

public sealed record GetUnreadChatCountQuery;

public sealed class GetUnreadChatCountHandler(IChatRepository repository) : IQueryHandler<GetUnreadChatCountQuery, int>
{
    public Task<int> HandleAsync(GetUnreadChatCountQuery query, CancellationToken cancellationToken = default) =>
        repository.CountUnreadAsync(cancellationToken);
}

public sealed record ReplyToChatCommand(Guid ConversationId, string AgentName, string Text);

public sealed class ReplyToChatHandler(
    IChatRepository repository,
    IUnitOfWork unitOfWork,
    IChatNotifier notifier,
    TimeProvider timeProvider) : ICommandHandler<ReplyToChatCommand>
{
    public async Task<Result> HandleAsync(ReplyToChatCommand command, CancellationToken cancellationToken = default)
    {
        var conversation = await repository.GetAsync(command.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return Result.Failure(Error.NotFound("Conversation"));
        }

        try
        {
            conversation.AddAgentReply(command.AgentName, command.Text, timeProvider.UtcNow());
        }
        catch (DomainException ex)
        {
            return Result.Failure(DomainErrors.From(ex));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        notifier.ConversationChanged(conversation.Id);
        return Result.Success();
    }
}

public enum ChatAction
{
    MarkRead,
    Close,
    Delete,
}

public sealed record UpdateChatCommand(Guid ConversationId, ChatAction Action);

public sealed class UpdateChatHandler(IChatRepository repository, IUnitOfWork unitOfWork, IChatNotifier notifier)
    : ICommandHandler<UpdateChatCommand>
{
    public async Task<Result> HandleAsync(UpdateChatCommand command, CancellationToken cancellationToken = default)
    {
        var conversation = await repository.GetAsync(command.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return Result.Failure(Error.NotFound("Conversation"));
        }

        switch (command.Action)
        {
            case ChatAction.MarkRead:
                if (conversation.UnreadByAgent == 0)
                {
                    return Result.Success();
                }

                conversation.MarkReadByAgent();
                break;
            case ChatAction.Close:
                conversation.Close();
                break;
            case ChatAction.Delete:
                repository.Remove(conversation);
                break;
            default:
                return Result.Failure(Error.Validation(nameof(command.Action), "Unknown action."));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        notifier.ConversationChanged(conversation.Id);
        return Result.Success();
    }
}
