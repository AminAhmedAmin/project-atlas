using Atlas.Domain.Common;

namespace Atlas.Domain.Content;

/// <summary>Editable text and SEO metadata for one public page.</summary>
public sealed class PageContent : Entity
{
    public const int TitleMaxLength = 150;
    public const int SubtitleMaxLength = 400;
    public const int BodyMaxLength = 10_000;
    public const int CallToActionTextMaxLength = 50;
    public const int CallToActionUrlMaxLength = 300;
    public const int MetaDescriptionMaxLength = 160;

    private PageContent()
    {
        Title = string.Empty;
    }

    public PageKey Key { get; private set; }

    public string Title { get; private set; }

    public string? Subtitle { get; private set; }

    public string? Body { get; private set; }

    public string? CallToActionText { get; private set; }

    public string? CallToActionUrl { get; private set; }

    public string? MetaDescription { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static PageContent Create(PageKey key, PageText text, DateTime nowUtc)
    {
        if (!Enum.IsDefined(key))
        {
            throw new DomainException($"Unknown page '{key}'.");
        }

        var page = new PageContent { Key = key };
        page.Update(text, nowUtc);
        return page;
    }

    public void Update(PageText text, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(text);

        var ctaText = Guard.Optional(text.CallToActionText, "Call-to-action text", CallToActionTextMaxLength);
        var ctaUrl = Guard.OptionalLink(text.CallToActionUrl, "Call-to-action link", CallToActionUrlMaxLength);
        if ((ctaText is null) != (ctaUrl is null))
        {
            throw new DomainException("Call-to-action text and link must be provided together.");
        }

        Title = Guard.Required(text.Title, "Title", TitleMaxLength);
        Subtitle = Guard.Optional(text.Subtitle, "Subtitle", SubtitleMaxLength);
        Body = Guard.Optional(text.Body, "Body", BodyMaxLength);
        CallToActionText = ctaText;
        CallToActionUrl = ctaUrl;
        MetaDescription = Guard.Optional(text.MetaDescription, "Meta description", MetaDescriptionMaxLength);
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }
}
