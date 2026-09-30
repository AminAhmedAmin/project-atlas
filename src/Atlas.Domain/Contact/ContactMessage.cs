using Atlas.Domain.Common;

namespace Atlas.Domain.Contact;

/// <summary>A message submitted through the public contact form.</summary>
public sealed class ContactMessage : Entity
{
    public const int NameMaxLength = 100;
    public const int SubjectMaxLength = 150;
    public const int MessageMaxLength = 4_000;

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

    public DateTime ReceivedAtUtc { get; private set; }

    public bool IsRead { get; private set; }

    public DateTime? ReadAtUtc { get; private set; }

    public static ContactMessage Create(
        string name,
        EmailAddress email,
        string? subject,
        string message,
        DateTime receivedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(email);

        return new ContactMessage
        {
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
