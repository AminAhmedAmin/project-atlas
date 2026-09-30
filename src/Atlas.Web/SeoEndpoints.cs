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
            var builder = new StringBuilder();
            using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { Indent = true }))
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

            return Results.Text(builder.ToString(), "application/xml", Encoding.UTF8);
        }).ExcludeFromDescription();

        return endpoints;
    }
}
