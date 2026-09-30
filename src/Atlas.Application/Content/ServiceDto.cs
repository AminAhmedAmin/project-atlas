using Atlas.Domain.Content;

namespace Atlas.Application.Content;

public sealed record ServiceDto(
    Guid Id,
    string Title,
    string Summary,
    string? Description,
    string? Icon,
    int DisplayOrder,
    bool IsPublished,
    DateTime UpdatedAtUtc);

internal static class ServiceMapping
{
    public static ServiceDto ToDto(this Service service) => new(
        service.Id,
        service.Title,
        service.Summary,
        service.Description,
        service.Icon,
        service.DisplayOrder,
        service.IsPublished,
        service.UpdatedAtUtc);
}
