using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Common;
using Atlas.Domain.Content;

namespace Atlas.Application.Content;

public sealed record UpdatePageContentCommand(
    PageKey Key,
    string Title,
    string? Subtitle,
    string? Body,
    string? CallToActionText,
    string? CallToActionUrl,
    string? MetaDescription,
    SiteLanguage Language = SiteLanguage.English)
{
    public PageText ToPageText() =>
        new(Title, Subtitle, Body, CallToActionText, CallToActionUrl, MetaDescription);
}

public sealed class UpdatePageContentValidator : IValidator<UpdatePageContentCommand>
{
    public IReadOnlyList<Error> Validate(UpdatePageContentCommand instance) => new ValidationErrors()
        .Must(Enum.IsDefined(instance.Key), nameof(instance.Key), "Unknown page.")
        .Must(Enum.IsDefined(instance.Language), nameof(instance.Language), "Unknown language.")
        .Required(instance.Title, nameof(instance.Title), "Title", PageContent.TitleMaxLength)
        .Optional(instance.Subtitle, nameof(instance.Subtitle), "Subtitle", PageContent.SubtitleMaxLength)
        .Optional(instance.Body, nameof(instance.Body), "Body", PageContent.BodyMaxLength)
        .Optional(instance.CallToActionText, nameof(instance.CallToActionText), "Button text", PageContent.CallToActionTextMaxLength)
        .Optional(instance.CallToActionUrl, nameof(instance.CallToActionUrl), "Button link", PageContent.CallToActionUrlMaxLength)
        .Must(
            string.IsNullOrWhiteSpace(instance.CallToActionText) == string.IsNullOrWhiteSpace(instance.CallToActionUrl),
            nameof(instance.CallToActionUrl),
            "Button text and link must be provided together.")
        .Optional(instance.MetaDescription, nameof(instance.MetaDescription), "Meta description", PageContent.MetaDescriptionMaxLength)
        .Errors;
}

public sealed class UpdatePageContentHandler(
    IPageContentRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<UpdatePageContentCommand> validator,
    TimeProvider timeProvider) : ICommandHandler<UpdatePageContentCommand, PageContentDto>
{
    public async Task<Result<PageContentDto>> HandleAsync(UpdatePageContentCommand command, CancellationToken cancellationToken = default)
    {
        var errors = validator.Validate(command);
        if (errors.Count > 0)
        {
            return Result.Failure<PageContentDto>(errors);
        }

        try
        {
            var now = timeProvider.UtcNow();
            var page = await repository.GetAsync(command.Key, command.Language, cancellationToken);
            if (page is null)
            {
                page = PageContent.Create(command.Key, command.ToPageText(), now, command.Language);
                repository.Add(page);
            }
            else
            {
                page.Update(command.ToPageText(), now);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(page.ToDto());
        }
        catch (DomainException ex)
        {
            return Result.Failure<PageContentDto>(DomainErrors.From(ex));
        }
    }
}
