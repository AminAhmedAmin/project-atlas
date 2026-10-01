using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Common;
using Atlas.Domain.Content;

namespace Atlas.Application.Content;

/// <summary>Creates a block when <see cref="Id"/> is null, otherwise updates it.</summary>
public sealed record SaveContentBlockCommand(
    Guid? Id,
    BlockKind Kind,
    string Title,
    string? Subtitle,
    string? Text,
    string? ImageUrl,
    int DisplayOrder,
    bool IsPublished)
{
    public ContentBlockFields ToFields() => new(Title, Subtitle, Text, ImageUrl, DisplayOrder, IsPublished);
}

public sealed class SaveContentBlockValidator : IValidator<SaveContentBlockCommand>
{
    public IReadOnlyList<Error> Validate(SaveContentBlockCommand instance)
    {
        var errors = new ValidationErrors()
            .Must(Enum.IsDefined(instance.Kind), nameof(instance.Kind), "Unknown block type.")
            .Required(instance.Title, nameof(instance.Title), TitleLabel(instance.Kind), ContentBlock.TitleMaxLength)
            .Optional(instance.Subtitle, nameof(instance.Subtitle), "Subtitle", ContentBlock.SubtitleMaxLength)
            .Optional(instance.ImageUrl, nameof(instance.ImageUrl), "Image", ContentBlock.ImageUrlMaxLength)
            .Must(instance.DisplayOrder >= 0, nameof(instance.DisplayOrder), "Display order cannot be negative.");

        return (ContentBlock.RequiresText(instance.Kind)
                ? errors.Required(instance.Text, nameof(instance.Text), TextLabel(instance.Kind), ContentBlock.TextMaxLength)
                : errors.Optional(instance.Text, nameof(instance.Text), TextLabel(instance.Kind), ContentBlock.TextMaxLength))
            .Errors;
    }

    private static string TitleLabel(BlockKind kind) => kind switch
    {
        BlockKind.Stat => "Number",
        BlockKind.ClientLogo => "Client name",
        BlockKind.Testimonial => "Person's name",
        BlockKind.Faq => "Question",
        _ => "Title",
    };

    private static string TextLabel(BlockKind kind) => kind switch
    {
        BlockKind.Stat => "Label",
        BlockKind.Testimonial => "Quote",
        BlockKind.Faq => "Answer",
        _ => "Description",
    };
}

public sealed class SaveContentBlockHandler(
    IContentBlockRepository repository,
    IUnitOfWork unitOfWork,
    IFileStorage fileStorage,
    IValidator<SaveContentBlockCommand> validator,
    TimeProvider timeProvider) : ICommandHandler<SaveContentBlockCommand, ContentBlockDto>
{
    public async Task<Result<ContentBlockDto>> HandleAsync(SaveContentBlockCommand command, CancellationToken cancellationToken = default)
    {
        var errors = validator.Validate(command);
        if (errors.Count > 0)
        {
            return Result.Failure<ContentBlockDto>(errors);
        }

        try
        {
            var now = timeProvider.UtcNow();
            ContentBlock block;
            string? replacedImage = null;

            if (command.Id is { } id)
            {
                var existing = await repository.GetByIdAsync(id, cancellationToken);
                if (existing is null || existing.Kind != command.Kind)
                {
                    return Result.Failure<ContentBlockDto>(Error.NotFound("Block"));
                }

                block = existing;
                if (block.ImageUrl is { } oldImage && !string.Equals(oldImage, command.ImageUrl, StringComparison.Ordinal))
                {
                    replacedImage = oldImage;
                }

                block.Update(command.ToFields(), now);
            }
            else
            {
                block = ContentBlock.Create(command.Kind, command.ToFields(), now);
                repository.Add(block);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (replacedImage is not null)
            {
                await fileStorage.DeleteAsync(replacedImage, cancellationToken);
            }

            return Result.Success(block.ToDto());
        }
        catch (DomainException ex)
        {
            return Result.Failure<ContentBlockDto>(DomainErrors.From(ex));
        }
    }
}
