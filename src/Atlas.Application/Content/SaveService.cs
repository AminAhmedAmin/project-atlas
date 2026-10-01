using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Common;
using Atlas.Domain.Content;

namespace Atlas.Application.Content;

/// <summary>Creates a service when <see cref="Id"/> is null, otherwise updates it.</summary>
public sealed record SaveServiceCommand(
    Guid? Id,
    string Title,
    string Summary,
    string? Description,
    string? Icon,
    int DisplayOrder,
    bool IsPublished,
    SiteLanguage Language = SiteLanguage.English);

public sealed class SaveServiceValidator : IValidator<SaveServiceCommand>
{
    public IReadOnlyList<Error> Validate(SaveServiceCommand instance) => new ValidationErrors()
        .Required(instance.Title, nameof(instance.Title), "Title", Service.TitleMaxLength)
        .Required(instance.Summary, nameof(instance.Summary), "Summary", Service.SummaryMaxLength)
        .Optional(instance.Description, nameof(instance.Description), "Description", Service.DescriptionMaxLength)
        .Optional(instance.Icon, nameof(instance.Icon), "Icon", Service.IconMaxLength)
        .Must(instance.DisplayOrder >= 0, nameof(instance.DisplayOrder), "Display order cannot be negative.")
        .Must(Enum.IsDefined(instance.Language), nameof(instance.Language), "Unknown language.")
        .Errors;
}

public sealed class SaveServiceHandler(
    IServiceRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<SaveServiceCommand> validator,
    TimeProvider timeProvider) : ICommandHandler<SaveServiceCommand, ServiceDto>
{
    public async Task<Result<ServiceDto>> HandleAsync(SaveServiceCommand command, CancellationToken cancellationToken = default)
    {
        var errors = validator.Validate(command);
        if (errors.Count > 0)
        {
            return Result.Failure<ServiceDto>(errors);
        }

        try
        {
            var now = timeProvider.UtcNow();
            Service service;
            if (command.Id is { } id)
            {
                var existing = await repository.GetByIdAsync(id, cancellationToken);
                if (existing is null || existing.Language != command.Language)
                {
                    return Result.Failure<ServiceDto>(Error.NotFound("Service"));
                }

                service = existing;
                service.Update(command.Title, command.Summary, command.Description, command.Icon, command.DisplayOrder, command.IsPublished, now);
            }
            else
            {
                service = Service.Create(command.Title, command.Summary, command.Description, command.Icon, command.DisplayOrder, command.IsPublished, now, command.Language);
                repository.Add(service);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(service.ToDto());
        }
        catch (DomainException ex)
        {
            return Result.Failure<ServiceDto>(DomainErrors.From(ex));
        }
    }
}
