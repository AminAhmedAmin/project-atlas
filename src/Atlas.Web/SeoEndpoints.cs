using System.Text;
using System.Xml;
using Atlas.Application.Portfolio;
using Atlas.Domain.Common;
using Atlas.Web.Localization;

namespace Atlas.Web;

internal static class SeoEndpoints
{
    private static readonly string[] PublicPaths = ["/", "/services", "/work", "/about", "/contact"];

    public static IEndpointRouteBuilder MapSeoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/sitemap.xml", async (HttpRequest request, UseCases useCases, CancellationToken cancellationToken) =>
        {
            var studies = await useCases.QueryAsync<GetCaseStudiesQuery, IReadOnlyList<CaseStudyDto>>(
                new(null, PublishedOnly: true), cancellationToken);
            var baseUrl = $"{request.Scheme}://{request.Host}";
            using var stream = new MemoryStream();
            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) }))
            {
                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
                foreach (var study in studies)
                {
                    writer.WriteStartElement("url");
                    writer.WriteElementString("loc", baseUrl + SiteLanguages.Localize($"/work/{study.Slug}", study.Language));
                    writer.WriteEndElement();
                }

                foreach (var path in PublicPaths)
                {
                    foreach (var language in Enum.GetValues<SiteLanguage>())
                    {
                        writer.WriteStartElement("url");
                        writer.WriteElementString("loc", baseUrl + SiteLanguages.Localize(path, language));
                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();
            }

            return Results.Bytes(stream.ToArray(), "application/xml; charset=utf-8");
        }).ExcludeFromDescription();

        return endpoints;
    }
}
