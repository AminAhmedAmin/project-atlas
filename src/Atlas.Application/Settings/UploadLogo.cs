using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Settings;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Settings;

/// <param name="Content">The uploaded file. Read at most <see cref="UploadLogoValidator.MaxBytes"/> bytes.</param>
public sealed record UploadLogoCommand(Stream Content, string FileName, string ContentType, long Length);

public sealed class UploadLogoValidator : IValidator<UploadLogoCommand>
{
    public const long MaxBytes = 2 * 1024 * 1024;

    /// <summary>Allowed image types. SVG is excluded because it can carry script.</summary>
    public static readonly IReadOnlyDictionary<string, string> AllowedTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/png"] = ".png",
        ["image/jpeg"] = ".jpg",
        ["image/webp"] = ".webp",
    };

    public IReadOnlyList<Error> Validate(UploadLogoCommand instance) => new ValidationErrors()
        .Must(instance.Length > 0, nameof(instance.Content), "The file is empty.")
        .Must(instance.Length <= MaxBytes, nameof(instance.Content), "The logo must be 2 MB or smaller.")
        .Must(AllowedTypes.ContainsKey(instance.ContentType), nameof(instance.ContentType), "The logo must be a PNG, JPEG or WebP image.")
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

        using var buffer = new MemoryStream();
        if (!await TryCopyLimitedAsync(command.Content, buffer, UploadLogoValidator.MaxBytes, cancellationToken))
        {
            return Result.Failure<string>(Error.Validation(nameof(command.Content), "The logo must be 2 MB or smaller."));
        }

        if (!ImageSignature.Matches(buffer.GetBuffer().AsSpan(0, (int)buffer.Length), command.ContentType))
        {
            return Result.Failure<string>(Error.Validation(nameof(command.Content), "The file content does not match its image type."));
        }

        buffer.Position = 0;
        var extension = UploadLogoValidator.AllowedTypes[command.ContentType];
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

    private static async Task<bool> TryCopyLimitedAsync(Stream source, Stream destination, long maxBytes, CancellationToken cancellationToken)
    {
        var chunk = new byte[81920];
        long total = 0;
        int read;
        while ((read = await source.ReadAsync(chunk, cancellationToken)) > 0)
        {
            total += read;
            if (total > maxBytes)
            {
                return false;
            }

            await destination.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }

        return true;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Logo updated to {LogoUrl}")]
    private static partial void LogLogoUpdated(ILogger logger, string logoUrl);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not delete previous logo {LogoUrl}")]
    private static partial void LogDeleteFailed(ILogger logger, string logoUrl, Exception exception);
}

internal static class ImageSignature
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];

    public static bool Matches(ReadOnlySpan<byte> data, string contentType) => contentType.ToUpperInvariant() switch
    {
        "IMAGE/PNG" => data.StartsWith(Png),
        "IMAGE/JPEG" => data.StartsWith(Jpeg),
        "IMAGE/WEBP" => data.Length >= 12 && data[..4].SequenceEqual("RIFF"u8) && data[8..12].SequenceEqual("WEBP"u8),
        _ => false,
    };
}
