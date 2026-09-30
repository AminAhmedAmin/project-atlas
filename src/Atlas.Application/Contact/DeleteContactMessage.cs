using Atlas.Application.Abstractions;
using Atlas.Application.Common;

namespace Atlas.Application.Contact;

public sealed record DeleteContactMessageCommand(Guid Id);

public sealed class DeleteContactMessageHandler(IContactMessageRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteContactMessageCommand>
{
    public async Task<Result> HandleAsync(DeleteContactMessageCommand command, CancellationToken cancellationToken = default)
    {
        var message = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (message is null)
        {
            return Result.Failure(Error.NotFound("Message"));
        }

        repository.Remove(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
