using Atlas.Application.Abstractions;
using Atlas.Application.Common;

namespace Atlas.Application.Content;

/// <summary>Stores an image for a content block (e.g. a client logo) and returns its URL.</summary>
public sealed record UploadContentImageCommand(Stream Content, string FileName, string ContentType, long Length);

public sealed class UploadContentImageValidator : IValidator<UploadContentImageCommand>
{
    public IReadOnlyList<Error> Validate(UploadContentImageCommand instance) => new ValidationErrors()
        .ValidateUpload(instance.Length, instance.ContentType, nameof(instance.Content))
        .Errors;
}

public sealed class UploadContentImageHandler(IFileStorage fileStorage, IValidator<UploadContentImageCommand> validator)
    : ICommandHandler<UploadContentImageCommand, string>
{
    private const string Folder = "content";

    public async Task<Result<string>> HandleAsync(UploadContentImageCommand command, CancellationToken cancellationToken = default)
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
        var url = await fileStorage.SaveAsync(Folder, $"img-{Guid.NewGuid():N}{extension}", buffer, cancellationToken);
        return Result.Success(url);
    }
}
