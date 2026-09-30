using Atlas.Application.Abstractions;
using Atlas.Application.Common;

namespace Atlas.Application.Settings;

public sealed record RemoveLogoCommand;

public sealed class RemoveLogoHandler(
    ISiteSettingsRepository repository,
    IUnitOfWork unitOfWork,
    IFileStorage fileStorage,
    TimeProvider timeProvider) : ICommandHandler<RemoveLogoCommand>
{
    public async Task<Result> HandleAsync(RemoveLogoCommand command, CancellationToken cancellationToken = default)
    {
        var settings = await repository.GetAsync(cancellationToken);
        if (settings?.LogoUrl is not { } logoUrl)
        {
            return Result.Success();
        }

        settings.RemoveLogo(timeProvider.UtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await fileStorage.DeleteAsync(logoUrl, cancellationToken);
        return Result.Success();
    }
}
