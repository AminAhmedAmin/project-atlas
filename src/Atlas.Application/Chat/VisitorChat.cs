using System.Security.Cryptography;
using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Chat;
using Atlas.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Chat;

public sealed record StartChatCommand(string Name, string? Contact, SiteLanguage Language, string Message);

public sealed class StartChatValidator : IValidator<StartChatCommand>
{
    public IReadOnlyList<Error> Validate(StartChatCommand instance) => new ValidationErrors()
        .Required(instance.Name, nameof(instance.Name), "Name", ChatConversation.VisitorNameMaxLength)
        .Optional(instance.Contact, nameof(instance.Contact), "Contact", ChatConversation.VisitorContactMaxLength)
        .Must(Enum.IsDefined(instance.Language), nameof(instance.Language), "Unknown language.")
        .Required(instance.Message, nameof(instance.Message), "Message", ChatMessage.TextMaxLength)
        .Errors;
}

public sealed partial class StartChatHandler(
    IChatRepository repository,
    IUnitOfWork unitOfWork,
    IChatNotifier notifier,
    ITeamNotifier teamNotifier,
    IValidator<StartChatCommand> validator,
    TimeProvider timeProvider,
    ILogger<StartChatHandler> logger) : ICommandHandler<StartChatCommand, StartChatResult>
{
    public async Task<Result<StartChatResult>> HandleAsync(StartChatCommand command, CancellationToken cancellationToken = default)
    {
        var errors = validator.Validate(command);
        if (errors.Count > 0)
        {
            return Result.Failure<StartChatResult>(errors);
        }

        // 24 random bytes as 48 hex characters: unguessable, and safe to keep in browser storage.
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(ChatConversation.AccessTokenLength / 2));

        ChatConversation conversation;
        try
        {
            conversation = ChatConversation.Start(command.Name, command.Contact, command.Language, command.Message, token, timeProvider.UtcNow());
        }
        catch (DomainException ex)
        {
            return Result.Failure<StartChatResult>(DomainErrors.From(ex));
        }

        repository.Add(conversation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        LogStarted(logger, conversation.Id);
        notifier.ConversationChanged(conversation.Id);

        await teamNotifier.NotifyAsync(
            new TeamAlert(
                "💬 New live chat",
                [
                    new("Name", conversation.VisitorName),
                    new("Contact", conversation.VisitorContact),
                    new("Language", conversation.Language.ToString()),
                ],
                command.Message,
                "/admin/chat"),
            cancellationToken);

        return Result.Success(new StartChatResult(conversation.Id, token));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Chat {ConversationId} started")]
    private static partial void LogStarted(ILogger logger, Guid conversationId);

}

public sealed record SendVisitorChatMessageCommand(Guid ConversationId, string AccessToken, string Text);

public sealed class SendVisitorChatMessageHandler(
    IChatRepository repository,
    IUnitOfWork unitOfWork,
    IChatNotifier notifier,
    ITeamNotifier teamNotifier,
    TimeProvider timeProvider) : ICommandHandler<SendVisitorChatMessageCommand>
{
    public async Task<Result> HandleAsync(SendVisitorChatMessageCommand command, CancellationToken cancellationToken = default)
    {
        var conversation = await repository.GetAsync(command.ConversationId, cancellationToken);
        if (conversation is null || !conversation.HasAccessToken(command.AccessToken))
        {
            return Result.Failure(Error.NotFound("Conversation"));
        }

        // Alert the team when a visitor writes again after the team has caught up,
        // not for every message in a burst.
        var teamWasUpToDate = conversation.UnreadByAgent == 0;
        try
        {
            conversation.AddVisitorMessage(command.Text, timeProvider.UtcNow());
        }
        catch (DomainException ex)
        {
            return Result.Failure(DomainErrors.From(ex));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        notifier.ConversationChanged(conversation.Id);

        if (teamWasUpToDate)
        {
            await teamNotifier.NotifyAsync(
                new TeamAlert("💬 New chat message", [new("From", conversation.VisitorName)], command.Text, "/admin/chat"),
                cancellationToken);
        }

        return Result.Success();
    }
}

/// <summary>The visitor's own conversation, or null when the id/token pair does not match.</summary>
public sealed record GetVisitorChatQuery(Guid ConversationId, string AccessToken);

public sealed class GetVisitorChatHandler(IChatRepository repository) : IQueryHandler<GetVisitorChatQuery, VisitorChatDto?>
{
    public async Task<VisitorChatDto?> HandleAsync(GetVisitorChatQuery query, CancellationToken cancellationToken = default)
    {
        var conversation = await repository.GetAsync(query.ConversationId, cancellationToken);
        return conversation is not null && conversation.HasAccessToken(query.AccessToken) ? conversation.ToVisitorDto() : null;
    }
}
