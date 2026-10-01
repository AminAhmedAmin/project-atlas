using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Settings;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Settings;

/// <param name="Content">The uploaded file. Read at most <see cref="UploadLogoValidator.MaxBytes"/> bytes.</param>
public sealed record UploadLogoCommand(Stream Content, string FileName, string ContentType, long Length);

public sealed class UploadLogoValidator : IValidator<UploadLogoCommand>
{
    public const long MaxBytes = ImageUpload.MaxBytes;

    public IReadOnlyList<Error> Validate(UploadLogoCommand instance) => new ValidationErrors()
        .ValidateUpload(instance.Length, instance.ContentType, nameof(instance.Content))
        .Errors;
}

public sealed partial class UploadLogoHandler(
    ISiteSettingsRepository repository,
    IUnitOfWork unitOfWork,
    IFileStorage fileStorage,
    IValidator<UploadLogoCommand> validator,
    BrandingDefaults defaults,
    TimeProvider timeProvider,
    ILogger<UploadLogoHandler> logger) : ICommandHandler<UploadLogoCommand, string>
{
    private const string Folder = "branding";

    public async Task<Result<string>> HandleAsync(UploadLogoCommand command, CancellationToken cancellationToken = default)
    {
        var errors = validator.Validate(command);
        if (errors.Count > 0)
        {
            return Result.Failure<string>(errors);
        }

        var image = await ImageUpload.ReadVerifiedAsync(command.Content, command.ContentType, nameof(command.Content), cancellationToken);
        if (image.IsFailure)
        {
            return Result.Failure<string>(image.Errors);
        }

        await using var buffer = image.Value;
        var extension = ImageUpload.AllowedTypes[command.ContentType];
        var url = await fileStorage.SaveAsync(Folder, $"logo-{Guid.NewGuid():N}{extension}", buffer, cancellationToken);

        var now = timeProvider.UtcNow();
        var settings = await repository.GetAsync(cancellationToken);
        var previousUrl = settings?.LogoUrl;
        if (settings is null)
        {
            settings = SiteSettings.Create(
                defaults.CompanyName,
                defaults.Tagline,
                Domain.Common.HexColor.Create(defaults.PrimaryColor),
                null,
                now);
            repository.Add(settings);
        }

        settings.SetLogo(url, now);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (previousUrl is not null)
        {
            try
            {
                await fileStorage.DeleteAsync(previousUrl, cancellationToken);
            }
            catch (IOException ex)
            {
                LogDeleteFailed(logger, previousUrl, ex);
            }
        }

        LogLogoUpdated(logger, url);
        return Result.Success(url);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Logo updated to {LogoUrl}")]
    private static partial void LogLogoUpdated(ILogger logger, string logoUrl);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not delete previous logo {LogoUrl}")]
    private static partial void LogDeleteFailed(ILogger logger, string logoUrl, Exception exception);
}
