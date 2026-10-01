using Atlas.Application.Abstractions;
using Atlas.Application.Common;

namespace Atlas.Application.Contact;

public sealed record SetContactMessageReadCommand(Guid Id, bool IsRead);

public sealed class SetContactMessageReadHandler(
    IContactMessageRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<SetContactMessageReadCommand>
{
    public async Task<Result> HandleAsync(SetContactMessageReadCommand command, CancellationToken cancellationToken = default)
    {
        var message = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (message is null)
        {
            return Result.Failure(Error.NotFound("Message"));
        }

        if (command.IsRead)
        {
            message.MarkAsRead(timeProvider.UtcNow());
        }
        else
        {
            message.MarkAsUnread();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
