using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Common;
using Atlas.Domain.Contact;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Contact;

public sealed record SubmitContactMessageCommand(
    string Name,
    string Email,
    string? Subject,
    string Message,
    string? Phone = null,
    string? Service = null,
    string? Budget = null,
    SiteLanguage Language = SiteLanguage.English);

public sealed class SubmitContactMessageValidator : IValidator<SubmitContactMessageCommand>
{
    public IReadOnlyList<Error> Validate(SubmitContactMessageCommand instance) => new ValidationErrors()
        .Required(instance.Name, nameof(instance.Name), "Name", ContactMessage.NameMaxLength)
        .Email(instance.Email, nameof(instance.Email), "E-mail", required: true)
        .Optional(instance.Subject, nameof(instance.Subject), "Subject", ContactMessage.SubjectMaxLength)
        .Required(instance.Message, nameof(instance.Message), "Message", ContactMessage.MessageMaxLength)
        .Must(string.IsNullOrWhiteSpace(instance.Phone) || PhoneNumber.IsValid(instance.Phone), nameof(instance.Phone), "Phone number is not valid.")
        .Optional(instance.Service, nameof(instance.Service), "Service", ContactMessage.ServiceMaxLength)
        .Must(string.IsNullOrWhiteSpace(instance.Budget) || BudgetRanges.All.Contains(instance.Budget), nameof(instance.Budget), "Unknown budget range.")
        .Must(Enum.IsDefined(instance.Language), nameof(instance.Language), "Unknown language.")
        .Errors;
}

public sealed partial class SubmitContactMessageHandler(
    IContactMessageRepository repository,
    IUnitOfWork unitOfWork,
    ITeamNotifier teamNotifier,
    IValidator<SubmitContactMessageCommand> validator,
    TimeProvider timeProvider,
    ILogger<SubmitContactMessageHandler> logger) : ICommandHandler<SubmitContactMessageCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(SubmitContactMessageCommand command, CancellationToken cancellationToken = default)
    {
        var errors = validator.Validate(command);
        if (errors.Count > 0)
        {
            return Result.Failure<Guid>(errors);
        }

        ContactMessage message;
        try
        {
            message = ContactMessage.Create(
                command.Name,
                EmailAddress.Create(command.Email),
                command.Subject,
                command.Message,
                timeProvider.UtcNow(),
                new ContactDetails(command.Phone, command.Service, command.Budget, command.Language));
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(DomainErrors.From(ex));
        }

        repository.Add(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        LogReceived(logger, message.Id);

        await NotifyAsync(message, cancellationToken);
        return Result.Success(message.Id);
    }

    private Task NotifyAsync(ContactMessage message, CancellationToken cancellationToken) =>
        teamNotifier.NotifyAsync(
            new TeamAlert(
                "📩 New contact message",
                [
                    new("Name", message.Name),
                    new("E-mail", message.Email.Value),
                    new("Phone", message.Phone),
                    new("Service", message.Service),
                    new("Budget", message.Budget is null ? null : BudgetRanges.Label(message.Budget)),
                    new("Language", message.Language.ToString()),
                    new("Subject", message.Subject),
                ],
                message.Message,
                "/admin/messages"),
            cancellationToken);

    [LoggerMessage(Level = LogLevel.Information, Message = "Contact message {MessageId} received")]
    private static partial void LogReceived(ILogger logger, Guid messageId);

}
