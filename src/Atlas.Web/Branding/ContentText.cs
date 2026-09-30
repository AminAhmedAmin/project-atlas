using Atlas.Application.Content;

namespace Atlas.Web.Branding;

public static class ContentText
{
    /// <summary>Substitutes the {company} token so editable text never hard-codes the brand.</summary>
    public static string Render(string? text, string companyName) =>
        string.IsNullOrEmpty(text) ? string.Empty : text.Replace(DefaultContent.CompanyToken, companyName, StringComparison.OrdinalIgnoreCase);

    /// <summary>Splits body text into paragraphs on blank lines.</summary>
    public static IReadOnlyList<string> Paragraphs(string? text, string companyName) =>
        Render(text, companyName)
            .Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
