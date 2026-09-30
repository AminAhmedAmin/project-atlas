using Atlas.Domain.Contact;

namespace Atlas.Application.Contact;

public sealed record ContactMessageDto(
    Guid Id,
    string Name,
    string Email,
    string? Subject,
    string Message,
    DateTime ReceivedAtUtc,
    bool IsRead,
    DateTime? ReadAtUtc);

internal static class ContactMessageMapping
{
    public static ContactMessageDto ToDto(this ContactMessage message) => new(
        message.Id,
        message.Name,
        message.Email.Value,
        message.Subject,
        message.Message,
        message.ReceivedAtUtc,
        message.IsRead,
        message.ReadAtUtc);
}
