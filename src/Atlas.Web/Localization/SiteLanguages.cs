using Atlas.Domain.Common;

namespace Atlas.Web.Localization;

/// <summary>
/// The public site is published in English at "/..." and in Arabic at "/ar/...".
/// The language is derived from the URL, so every page has a stable, indexable address per language.
/// </summary>
public static class SiteLanguages
{
    public const string ArabicPrefix = "/ar";

    public static SiteLanguage FromPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return SiteLanguage.English;
        }

        return path.Equals(ArabicPrefix, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(ArabicPrefix + "/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(ArabicPrefix + "?", StringComparison.OrdinalIgnoreCase)
            ? SiteLanguage.Arabic
            : SiteLanguage.English;
    }

    public static SiteLanguage FromUri(string uri) => FromPath(new Uri(uri).AbsolutePath);

    public static string Code(SiteLanguage language) => language == SiteLanguage.Arabic ? "ar" : "en";

    public static bool IsRightToLeft(SiteLanguage language) => language == SiteLanguage.Arabic;

    public static string Direction(SiteLanguage language) => IsRightToLeft(language) ? "rtl" : "ltr";

    /// <summary>The language-neutral path, e.g. "/ar/services" becomes "/services".</summary>
    public static string NeutralPath(string path)
    {
        if (FromPath(path) != SiteLanguage.Arabic)
        {
            return string.IsNullOrEmpty(path) ? "/" : path;
        }

        var rest = path[ArabicPrefix.Length..];
        return rest.Length == 0 || rest[0] != '/' ? "/" + rest : rest;
    }

    /// <summary>The path of a site page in the given language, e.g. ("/services", Arabic) becomes "/ar/services".</summary>
    public static string Localize(string neutralPath, SiteLanguage language)
    {
        var path = string.IsNullOrEmpty(neutralPath) ? "/" : neutralPath;
        if (language != SiteLanguage.Arabic)
        {
            return path;
        }

        return path == "/" ? ArabicPrefix : ArabicPrefix + path;
    }

    public static SiteLanguage Other(SiteLanguage language) =>
        language == SiteLanguage.Arabic ? SiteLanguage.English : SiteLanguage.Arabic;
}
