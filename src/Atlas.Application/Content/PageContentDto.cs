using Atlas.Domain.Common;
using Atlas.Domain.Content;

namespace Atlas.Application.Content;

public sealed record PageContentDto(
    PageKey Key,
    SiteLanguage Language,
    string Title,
    string? Subtitle,
    string? Body,
    string? CallToActionText,
    string? CallToActionUrl,
    string? MetaDescription,
    DateTime? UpdatedAtUtc);

internal static class PageContentMapping
{
    public static PageContentDto ToDto(this PageContent page) => new(
        page.Key,
        page.Language,
        page.Title,
        page.Subtitle,
        page.Body,
        page.CallToActionText,
        page.CallToActionUrl,
        page.MetaDescription,
        page.UpdatedAtUtc);

    public static PageContentDto ToDto(this PageText text, PageKey key, SiteLanguage language) => new(
        key,
        language,
        text.Title,
        text.Subtitle,
        text.Body,
        text.CallToActionText,
        text.CallToActionUrl,
        text.MetaDescription,
        null);
}
