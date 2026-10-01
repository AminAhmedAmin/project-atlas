using Atlas.Domain.Content;

namespace Atlas.Application.Content;

public sealed record ContentBlockDto(
    Guid Id,
    BlockKind Kind,
    string Title,
    string? Subtitle,
    string? Text,
    string? ImageUrl,
    int DisplayOrder,
    bool IsPublished);

internal static class ContentBlockMapping
{
    public static ContentBlockDto ToDto(this ContentBlock block) => new(
        block.Id,
        block.Kind,
        block.Title,
        block.Subtitle,
        block.Text,
        block.ImageUrl,
        block.DisplayOrder,
        block.IsPublished);
}
