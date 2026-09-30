using Atlas.Application.Abstractions;

namespace Atlas.Application.Content;

/// <param name="PublishedOnly">True for the public site; false for the dashboard.</param>
public sealed record GetServicesQuery(bool PublishedOnly);

public sealed class GetServicesHandler(IServiceRepository repository)
    : IQueryHandler<GetServicesQuery, IReadOnlyList<ServiceDto>>
{
    public async Task<IReadOnlyList<ServiceDto>> HandleAsync(GetServicesQuery query, CancellationToken cancellationToken = default)
    {
        var services = await repository.ListAsync(query.PublishedOnly, cancellationToken);
        return services
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(s => s.ToDto())
            .ToList();
    }
}
