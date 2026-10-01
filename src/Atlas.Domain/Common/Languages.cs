namespace Atlas.Domain.Common;

internal static class Languages
{
    public static SiteLanguage Ensure(SiteLanguage language) =>
        Enum.IsDefined(language) ? language : throw new DomainException($"Unknown language '{language}'.");
}
