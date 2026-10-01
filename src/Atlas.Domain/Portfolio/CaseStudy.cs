using System.Text.RegularExpressions;
using Atlas.Domain.Common;

namespace Atlas.Domain.Portfolio;

/// <summary>A project shown in the portfolio ("Our work"), in one language.</summary>
public sealed partial class CaseStudy : Entity
{
    public const int SlugMaxLength = 80;
    public const int TitleMaxLength = 150;
    public const int ClientMaxLength = 100;
    public const int SummaryMaxLength = 300;
    public const int HighlightsMaxLength = 1_000;
    public const int BodyMaxLength = 8_000;
    public const int TagsMaxLength = 200;
    public const int ImageUrlMaxLength = 500;

    private CaseStudy()
    {
        Slug = string.Empty;
        Title = string.Empty;
        Summary = string.Empty;
    }

    public SiteLanguage Language { get; private set; }

    /// <summary>URL segment, e.g. "clinic-booking-app" for /work/clinic-booking-app.</summary>
    public string Slug { get; private set; }

    public string Title { get; private set; }

    public string? Client { get; private set; }

    public string Summary { get; private set; }

    /// <summary>Key results, one per line (e.g. "40% more online bookings").</summary>
    public string? Highlights { get; private set; }

    public string? Body { get; private set; }

    /// <summary>Comma-separated tags such as "Web, iOS, Android".</summary>
    public string? Tags { get; private set; }

    public string? CoverImageUrl { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsPublished { get; private set; }

    /// <summary>Featured case studies are shown on the home page.</summary>
    public bool IsFeatured { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyList<string> TagList => SplitTags(Tags);

    public IReadOnlyList<string> HighlightList =>
        (Highlights ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static CaseStudy Create(SiteLanguage language, CaseStudyFields fields, DateTime nowUtc)
    {
        var study = new CaseStudy
        {
            Language = Languages.Ensure(language),
            CreatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc)),
        };
        study.Update(fields, nowUtc);
        return study;
    }

    public void Update(CaseStudyFields fields, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(fields);

        if (fields.DisplayOrder < 0)
        {
            throw new DomainException("Display order cannot be negative.");
        }

        var slug = Guard.Required(fields.Slug, "URL name", SlugMaxLength).ToLowerInvariant();
        if (!IsValidSlug(slug))
        {
            throw new DomainException("URL name may only contain lowercase letters, digits and dashes (e.g. clinic-booking-app).");
        }

        Slug = slug;
        Title = Guard.Required(fields.Title, "Title", TitleMaxLength);
        Client = Guard.Optional(fields.Client, "Client", ClientMaxLength);
        Summary = Guard.Required(fields.Summary, "Summary", SummaryMaxLength);
        Highlights = Guard.Optional(fields.Highlights, "Highlights", HighlightsMaxLength);
        Body = Guard.Optional(fields.Body, "Body", BodyMaxLength);
        Tags = NormalizeTags(Guard.Optional(fields.Tags, "Tags", TagsMaxLength));
        CoverImageUrl = Guard.OptionalLink(fields.CoverImageUrl, "Cover image", ImageUrlMaxLength);
        DisplayOrder = fields.DisplayOrder;
        IsPublished = fields.IsPublished;
        IsFeatured = fields.IsFeatured;
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    public static bool IsValidSlug(string? slug) =>
        !string.IsNullOrEmpty(slug) && slug.Length <= SlugMaxLength && SlugPattern().IsMatch(slug);

    /// <summary>Suggests a URL name from a (Latin) title, e.g. "Clinic Booking App!" becomes "clinic-booking-app".</summary>
    public static string? SuggestSlug(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var slug = NonSlugCharacters().Replace(title.Trim().ToLowerInvariant(), "-").Trim('-');
        slug = RepeatedDashes().Replace(slug, "-");
        if (slug.Length > SlugMaxLength)
        {
            slug = slug[..SlugMaxLength].TrimEnd('-');
        }

        return IsValidSlug(slug) ? slug : null;
    }

    private static string? NormalizeTags(string? tags)
    {
        var list = SplitTags(tags);
        return list.Count == 0 ? null : string.Join(", ", list);
    }

    private static List<string> SplitTags(string? tags) =>
        (tags ?? string.Empty)
            .Split([',', '،'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();

    [GeneratedRegex("-{2,}")]
    private static partial Regex RepeatedDashes();
}

/// <summary>The editable parts of a <see cref="CaseStudy"/>.</summary>
public sealed record CaseStudyFields(
    string Slug,
    string Title,
    string Summary,
    string? Client = null,
    string? Highlights = null,
    string? Body = null,
    string? Tags = null,
    string? CoverImageUrl = null,
    int DisplayOrder = 0,
    bool IsPublished = true,
    bool IsFeatured = false);
