using Atlas.Domain.Common;

namespace Atlas.Domain.Contact;

/// <summary>A message submitted through the public contact form.</summary>
public sealed class ContactMessage : Entity
{
    public const int NameMaxLength = 100;
    public const int SubjectMaxLength = 150;
    public const int MessageMaxLength = 4_000;
    public const int ServiceMaxLength = 100;
    public const int BudgetMaxLength = 50;

    private ContactMessage()
    {
        Name = string.Empty;
        Email = EmailAddress.Create("unknown@example.com");
        Message = string.Empty;
    }

    public string Name { get; private set; }

    public EmailAddress Email { get; private set; }

    public string? Subject { get; private set; }

    public string Message { get; private set; }

    /// <summary>Optional phone number, as typed by the visitor.</summary>
    public string? Phone { get; private set; }

    /// <summary>The service the visitor is interested in, if they picked one.</summary>
    public string? Service { get; private set; }

    /// <summary>Budget range key chosen by the visitor (e.g. "50k-150k").</summary>
    public string? Budget { get; private set; }

    /// <summary>Language of the page the message was sent from, so replies can match it.</summary>
    public SiteLanguage Language { get; private set; } = SiteLanguage.English;

    public DateTime ReceivedAtUtc { get; private set; }

    public bool IsRead { get; private set; }

    public DateTime? ReadAtUtc { get; private set; }

    public static ContactMessage Create(
        string name,
        EmailAddress email,
        string? subject,
        string message,
        DateTime receivedAtUtc,
        ContactDetails? details = null)
    {
        ArgumentNullException.ThrowIfNull(email);
        details ??= new ContactDetails();

        var phone = Guard.Optional(details.Phone, "Phone", PhoneNumber.MaxLength);
        if (phone is not null && !PhoneNumber.IsValid(phone))
        {
            throw new DomainException("Phone number is not valid.");
        }

        return new ContactMessage
        {
            Phone = phone,
            Service = Guard.Optional(details.Service, "Service", ServiceMaxLength),
            Budget = Guard.Optional(details.Budget, "Budget", BudgetMaxLength),
            Language = Languages.Ensure(details.Language),
            Name = Guard.Required(name, "Name", NameMaxLength),
            Email = email,
            Subject = Guard.Optional(subject, "Subject", SubjectMaxLength),
            Message = Guard.Required(message, "Message", MessageMaxLength),
            ReceivedAtUtc = Guard.Utc(receivedAtUtc, nameof(receivedAtUtc)),
        };
    }

    /// <summary>Marks the message as read. Calling it again keeps the original read time.</summary>
    public void MarkAsRead(DateTime nowUtc)
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAtUtc = null;
    }
}

/// <summary>Optional extra details sent with a contact message.</summary>
public sealed record ContactDetails(
    string? Phone = null,
    string? Service = null,
    string? Budget = null,
    SiteLanguage Language = SiteLanguage.English);
