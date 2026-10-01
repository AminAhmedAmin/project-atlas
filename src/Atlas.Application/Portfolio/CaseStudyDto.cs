using Atlas.Domain.Common;
using Atlas.Domain.Portfolio;

namespace Atlas.Application.Portfolio;

public sealed record CaseStudyDto(
    Guid Id,
    SiteLanguage Language,
    string Slug,
    string Title,
    string? Client,
    string Summary,
    string? Highlights,
    string? Body,
    string? Tags,
    string? CoverImageUrl,
    int DisplayOrder,
    bool IsPublished,
    bool IsFeatured,
    DateTime UpdatedAtUtc)
{
    public IReadOnlyList<string> TagList =>
        (Tags ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public IReadOnlyList<string> HighlightList =>
        (Highlights ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

internal static class CaseStudyMapping
{
    public static CaseStudyDto ToDto(this CaseStudy study) => new(
        study.Id,
        study.Language,
        study.Slug,
        study.Title,
        study.Client,
        study.Summary,
        study.Highlights,
        study.Body,
        study.Tags,
        study.CoverImageUrl,
        study.DisplayOrder,
        study.IsPublished,
        study.IsFeatured,
        study.UpdatedAtUtc);
}
