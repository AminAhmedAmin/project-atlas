using Atlas.Application.Settings;
using Atlas.Domain.Common;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Atlas.Web.Localization;

/// <summary>Base class for public-site components: language, translations and localized links.</summary>
public abstract class PublicComponentBase : ComponentBase
{
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    /// <summary>The language of the current page, taken from its URL.</summary>
    protected SiteLanguage Lang => SiteLanguages.FromUri(Navigation.Uri);

    protected bool IsArabic => Lang == SiteLanguage.Arabic;

    /// <summary>Translate fixed interface text.</summary>
    protected string L(string english) => UiText.Get(Lang, english);

    /// <summary>Link to a site page in the current language, e.g. Href("/contact").</summary>
    protected string Href(string neutralPath) => SiteLanguages.Localize(neutralPath, Lang);

    /// <summary>An arrow that points "forward" in the reading direction.</summary>
    protected string ForwardIcon => IsArabic ? Icons.Material.Filled.ArrowBack : Icons.Material.Filled.ArrowForward;

    protected static string CompanyNameOf(SiteSettingsDto? settings, SiteLanguage language) =>
        settings?.CompanyNameFor(language) ?? string.Empty;
}
