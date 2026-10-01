using Atlas.Application.Abstractions;
using Atlas.Domain.Common;

namespace Atlas.Application.Content;

/// <param name="PublishedOnly">True for the public site; false for the dashboard.</param>
/// <param name="Language">Only services in this language, or all languages when null.</param>
public sealed record GetServicesQuery(bool PublishedOnly, SiteLanguage? Language = null);

public sealed class GetServicesHandler(IServiceRepository repository)
    : IQueryHandler<GetServicesQuery, IReadOnlyList<ServiceDto>>
{
    public async Task<IReadOnlyList<ServiceDto>> HandleAsync(GetServicesQuery query, CancellationToken cancellationToken = default)
    {
        var services = await repository.ListAsync(query.Language, query.PublishedOnly, cancellationToken);
        return services
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(s => s.ToDto())
            .ToList();
    }
}
