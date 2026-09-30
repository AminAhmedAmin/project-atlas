using System.Text;
using System.Xml;

namespace Atlas.Web;

internal static class SeoEndpoints
{
    private static readonly string[] PublicPaths = ["/", "/services", "/about", "/contact"];

    public static IEndpointRouteBuilder MapSeoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/sitemap.xml", (HttpRequest request) =>
        {
            var baseUrl = $"{request.Scheme}://{request.Host}";
            using var stream = new MemoryStream();
            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) }))
            {
                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
                foreach (var path in PublicPaths)
                {
                    writer.WriteStartElement("url");
                    writer.WriteElementString("loc", baseUrl + path);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            return Results.Bytes(stream.ToArray(), "application/xml; charset=utf-8");
        }).ExcludeFromDescription();

        return endpoints;
    }
}
