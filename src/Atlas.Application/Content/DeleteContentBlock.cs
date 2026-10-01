using Atlas.Application.Abstractions;
using Atlas.Application.Common;

namespace Atlas.Application.Content;

public sealed record DeleteContentBlockCommand(Guid Id);

public sealed class DeleteContentBlockHandler(IContentBlockRepository repository, IUnitOfWork unitOfWork, IFileStorage fileStorage)
    : ICommandHandler<DeleteContentBlockCommand>
{
    public async Task<Result> HandleAsync(DeleteContentBlockCommand command, CancellationToken cancellationToken = default)
    {
        var block = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (block is null)
        {
            return Result.Failure(Error.NotFound("Block"));
        }

        var image = block.ImageUrl;
        repository.Remove(block);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (image is not null)
        {
            await fileStorage.DeleteAsync(image, cancellationToken);
        }

        return Result.Success();
    }
}
