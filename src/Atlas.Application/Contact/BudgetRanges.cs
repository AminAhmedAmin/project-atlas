namespace Atlas.Application.Contact;

/// <summary>Budget ranges offered on the contact form (stored as keys, shown with localized labels).</summary>
public static class BudgetRanges
{
    public const string Under50K = "under-50k";
    public const string From50KTo150K = "50k-150k";
    public const string From150KTo500K = "150k-500k";
    public const string Over500K = "over-500k";
    public const string NotSure = "not-sure";

    public static IReadOnlyList<string> All { get; } = [Under50K, From50KTo150K, From150KTo500K, Over500K, NotSure];

    /// <summary>English label for the dashboard and e-mails.</summary>
    public static string Label(string? key) => key switch
    {
        Under50K => "Under 50,000 SAR",
        From50KTo150K => "50,000 – 150,000 SAR",
        From150KTo500K => "150,000 – 500,000 SAR",
        Over500K => "Over 500,000 SAR",
        NotSure => "Not sure yet",
        null or "" => "—",
        _ => key,
    };
}
