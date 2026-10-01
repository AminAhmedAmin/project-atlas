using Atlas.Domain.Common;

namespace Atlas.Domain.Content;

/// <summary>A service or product offered by the company.</summary>
public sealed class Service : Entity
{
    public const int TitleMaxLength = 100;
    public const int SummaryMaxLength = 300;
    public const int DescriptionMaxLength = 4_000;
    public const int IconMaxLength = 50;

    private Service()
    {
        Title = string.Empty;
        Summary = string.Empty;
    }

    public string Title { get; private set; }

    public string Summary { get; private set; }

    public string? Description { get; private set; }

    /// <summary>Icon key understood by the UI (e.g. "code", "cloud").</summary>
    public string? Icon { get; private set; }

    public int DisplayOrder { get; private set; }

    public SiteLanguage Language { get; private set; } = SiteLanguage.English;

    public bool IsPublished { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static Service Create(
        string title,
        string summary,
        string? description,
        string? icon,
        int displayOrder,
        bool isPublished,
        DateTime nowUtc,
        SiteLanguage language = SiteLanguage.English)
    {
        var service = new Service { CreatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc)), Language = Languages.Ensure(language) };
        service.Update(title, summary, description, icon, displayOrder, isPublished, nowUtc);
        return service;
    }

    public void Update(
        string title,
        string summary,
        string? description,
        string? icon,
        int displayOrder,
        bool isPublished,
        DateTime nowUtc)
    {
        if (displayOrder < 0)
        {
            throw new DomainException("Display order cannot be negative.");
        }

        Title = Guard.Required(title, "Title", TitleMaxLength);
        Summary = Guard.Required(summary, "Summary", SummaryMaxLength);
        Description = Guard.Optional(description, "Description", DescriptionMaxLength);
        Icon = Guard.Optional(icon, "Icon", IconMaxLength);
        DisplayOrder = displayOrder;
        IsPublished = isPublished;
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }
}
