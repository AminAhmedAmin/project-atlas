using Atlas.Domain.Common;

namespace Atlas.Domain.Content;

/// <summary>
/// A small, ordered piece of home page content (a stat, client logo, process step,
/// testimonial or FAQ). The meaning of each field depends on <see cref="Kind"/>.
/// </summary>
public sealed class ContentBlock : Entity
{
    public const int TitleMaxLength = 150;
    public const int SubtitleMaxLength = 150;
    public const int TextMaxLength = 2_000;
    public const int ImageUrlMaxLength = 500;

    private ContentBlock()
    {
        Title = string.Empty;
    }

    public BlockKind Kind { get; private set; }

    public SiteLanguage Language { get; private set; } = SiteLanguage.English;

    public string Title { get; private set; }

    public string? Subtitle { get; private set; }

    public string? Text { get; private set; }

    public string? ImageUrl { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>Whether <see cref="Text"/> is required for the given kind.</summary>
    public static bool RequiresText(BlockKind kind) => kind is not BlockKind.ClientLogo;

    public static ContentBlock Create(BlockKind kind, ContentBlockFields fields, DateTime nowUtc, SiteLanguage language = SiteLanguage.English)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new DomainException($"Unknown block kind '{kind}'.");
        }

        var block = new ContentBlock { Kind = kind, Language = Languages.Ensure(language) };
        block.Update(fields, nowUtc);
        return block;
    }

    public void Update(ContentBlockFields fields, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(fields);

        if (fields.DisplayOrder < 0)
        {
            throw new DomainException("Display order cannot be negative.");
        }

        Title = Guard.Required(fields.Title, "Title", TitleMaxLength);
        Subtitle = Guard.Optional(fields.Subtitle, "Subtitle", SubtitleMaxLength);
        Text = RequiresText(Kind)
            ? Guard.Required(fields.Text, "Text", TextMaxLength)
            : Guard.Optional(fields.Text, "Text", TextMaxLength);
        ImageUrl = Guard.OptionalLink(fields.ImageUrl, "Image", ImageUrlMaxLength);
        DisplayOrder = fields.DisplayOrder;
        IsPublished = fields.IsPublished;
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }
}

/// <summary>The editable parts of a <see cref="ContentBlock"/>.</summary>
public sealed record ContentBlockFields(
    string Title,
    string? Subtitle = null,
    string? Text = null,
    string? ImageUrl = null,
    int DisplayOrder = 0,
    bool IsPublished = true);
