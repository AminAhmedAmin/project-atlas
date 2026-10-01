using Atlas.Application.Abstractions;
using Atlas.Application.Common;

namespace Atlas.Application.Content;

public sealed record DeleteServiceCommand(Guid Id);

public sealed class DeleteServiceHandler(IServiceRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteServiceCommand>
{
    public async Task<Result> HandleAsync(DeleteServiceCommand command, CancellationToken cancellationToken = default)
    {
        var service = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (service is null)
        {
            return Result.Failure(Error.NotFound("Service"));
        }

        repository.Remove(service);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
