using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Common;
using Atlas.Domain.Portfolio;

namespace Atlas.Application.Portfolio;

/// <summary>Creates a case study when <see cref="Id"/> is null, otherwise updates it.</summary>
public sealed record SaveCaseStudyCommand(
    Guid? Id,
    SiteLanguage Language,
    string Slug,
    string Title,
    string Summary,
    string? Client,
    string? Highlights,
    string? Body,
    string? Tags,
    string? CoverImageUrl,
    int DisplayOrder,
    bool IsPublished,
    bool IsFeatured)
{
    public CaseStudyFields ToFields() =>
        new(Slug, Title, Summary, Client, Highlights, Body, Tags, CoverImageUrl, DisplayOrder, IsPublished, IsFeatured);
}

public sealed class SaveCaseStudyValidator : IValidator<SaveCaseStudyCommand>
{
    public IReadOnlyList<Error> Validate(SaveCaseStudyCommand instance) => new ValidationErrors()
        .Must(Enum.IsDefined(instance.Language), nameof(instance.Language), "Unknown language.")
        .Must(CaseStudy.IsValidSlug(instance.Slug?.Trim().ToLowerInvariant()), nameof(instance.Slug),
            "URL name may only contain lowercase letters, digits and dashes (e.g. clinic-booking-app).")
        .Required(instance.Title, nameof(instance.Title), "Title", CaseStudy.TitleMaxLength)
        .Required(instance.Summary, nameof(instance.Summary), "Summary", CaseStudy.SummaryMaxLength)
        .Optional(instance.Client, nameof(instance.Client), "Client", CaseStudy.ClientMaxLength)
        .Optional(instance.Highlights, nameof(instance.Highlights), "Results", CaseStudy.HighlightsMaxLength)
        .Optional(instance.Body, nameof(instance.Body), "Story", CaseStudy.BodyMaxLength)
        .Optional(instance.Tags, nameof(instance.Tags), "Tags", CaseStudy.TagsMaxLength)
        .Must(instance.DisplayOrder >= 0, nameof(instance.DisplayOrder), "Display order cannot be negative.")
        .Errors;
}

public sealed class SaveCaseStudyHandler(
    ICaseStudyRepository repository,
    IUnitOfWork unitOfWork,
    IFileStorage fileStorage,
    IValidator<SaveCaseStudyCommand> validator,
    TimeProvider timeProvider) : ICommandHandler<SaveCaseStudyCommand, CaseStudyDto>
{
    public async Task<Result<CaseStudyDto>> HandleAsync(SaveCaseStudyCommand command, CancellationToken cancellationToken = default)
    {
        var errors = validator.Validate(command);
        if (errors.Count > 0)
        {
            return Result.Failure<CaseStudyDto>(errors);
        }

        var slug = command.Slug.Trim().ToLowerInvariant();
        if (await repository.SlugExistsAsync(command.Language, slug, command.Id, cancellationToken))
        {
            return Result.Failure<CaseStudyDto>(new Error(
                "CaseStudy.SlugTaken", "Another case study already uses this URL name.", ErrorType.Conflict, nameof(command.Slug)));
        }

        try
        {
            var now = timeProvider.UtcNow();
            CaseStudy study;
            string? replacedImage = null;

            if (command.Id is { } id)
            {
                var existing = await repository.GetByIdAsync(id, cancellationToken);
                if (existing is null || existing.Language != command.Language)
                {
                    return Result.Failure<CaseStudyDto>(Error.NotFound("Case study"));
                }

                study = existing;
                if (study.CoverImageUrl is { } old && !string.Equals(old, command.CoverImageUrl, StringComparison.Ordinal))
                {
                    replacedImage = old;
                }

                study.Update(command.ToFields() with { Slug = slug }, now);
            }
            else
            {
                study = CaseStudy.Create(command.Language, command.ToFields() with { Slug = slug }, now);
                repository.Add(study);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (replacedImage is not null)
            {
                await fileStorage.DeleteAsync(replacedImage, cancellationToken);
            }

            return Result.Success(study.ToDto());
        }
        catch (DomainException ex)
        {
            return Result.Failure<CaseStudyDto>(DomainErrors.From(ex));
        }
    }
}

public sealed record DeleteCaseStudyCommand(Guid Id);

public sealed class DeleteCaseStudyHandler(ICaseStudyRepository repository, IUnitOfWork unitOfWork, IFileStorage fileStorage)
    : ICommandHandler<DeleteCaseStudyCommand>
{
    public async Task<Result> HandleAsync(DeleteCaseStudyCommand command, CancellationToken cancellationToken = default)
    {
        var study = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (study is null)
        {
            return Result.Failure(Error.NotFound("Case study"));
        }

        var image = study.CoverImageUrl;
        repository.Remove(study);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (image is not null)
        {
            await fileStorage.DeleteAsync(image, cancellationToken);
        }

        return Result.Success();
    }
}
