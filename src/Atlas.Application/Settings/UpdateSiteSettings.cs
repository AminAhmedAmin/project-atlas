using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Common;
using Atlas.Domain.Settings;

namespace Atlas.Application.Settings;

public sealed record UpdateSiteSettingsCommand(
    string CompanyName,
    string? Tagline,
    string PrimaryColor,
    string? ContactEmail,
    string? ArabicCompanyName = null,
    string? ArabicTagline = null,
    string? WhatsAppNumber = null);

public sealed class UpdateSiteSettingsValidator : IValidator<UpdateSiteSettingsCommand>
{
    public IReadOnlyList<Error> Validate(UpdateSiteSettingsCommand instance) => new ValidationErrors()
        .Required(instance.CompanyName, nameof(instance.CompanyName), "Company name", SiteSettings.CompanyNameMaxLength)
        .Optional(instance.Tagline, nameof(instance.Tagline), "Tagline", SiteSettings.TaglineMaxLength)
        .Optional(instance.ArabicCompanyName, nameof(instance.ArabicCompanyName), "Arabic company name", SiteSettings.CompanyNameMaxLength)
        .Optional(instance.ArabicTagline, nameof(instance.ArabicTagline), "Arabic tagline", SiteSettings.TaglineMaxLength)
        .Must(HexColor.TryCreate(instance.PrimaryColor, out _), nameof(instance.PrimaryColor), "Primary color must be in #RRGGBB format.")
        .Email(instance.ContactEmail, nameof(instance.ContactEmail), "Contact e-mail", required: false)
        .Must(string.IsNullOrWhiteSpace(instance.WhatsAppNumber) || PhoneNumber.IsValid(instance.WhatsAppNumber), nameof(instance.WhatsAppNumber), "WhatsApp number is not valid. Use the international format, e.g. +966 55 123 4567.")
        .Errors;
}

public sealed class UpdateSiteSettingsHandler(
    ISiteSettingsRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<UpdateSiteSettingsCommand> validator,
    TimeProvider timeProvider) : ICommandHandler<UpdateSiteSettingsCommand, SiteSettingsDto>
{
    public async Task<Result<SiteSettingsDto>> HandleAsync(UpdateSiteSettingsCommand command, CancellationToken cancellationToken = default)
    {
        var errors = validator.Validate(command);
        if (errors.Count > 0)
        {
            return Result.Failure<SiteSettingsDto>(errors);
        }

        var color = HexColor.Create(command.PrimaryColor);
        var email = string.IsNullOrWhiteSpace(command.ContactEmail) ? null : EmailAddress.Create(command.ContactEmail);
        var now = timeProvider.UtcNow();

        var settings = await repository.GetAsync(cancellationToken);
        if (settings is null)
        {
            settings = SiteSettings.Create(command.CompanyName, command.Tagline, color, email, now);
            repository.Add(settings);
        }
        else
        {
            settings.Update(command.CompanyName, command.Tagline, color, email, now);
        }

        settings.UpdateArabic(command.ArabicCompanyName, command.ArabicTagline, now);
        settings.SetWhatsApp(command.WhatsAppNumber, now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(settings.ToDto());
    }
}
