using Atlas.Application.Abstractions;
using Atlas.Domain.Content;

namespace Atlas.Application.Content;

/// <param name="Kind">Only this kind, or all kinds when null.</param>
/// <param name="PublishedOnly">True for the public site; false for the dashboard.</param>
public sealed record GetContentBlocksQuery(BlockKind? Kind, bool PublishedOnly);

public sealed class GetContentBlocksHandler(IContentBlockRepository repository)
    : IQueryHandler<GetContentBlocksQuery, IReadOnlyList<ContentBlockDto>>
{
    public async Task<IReadOnlyList<ContentBlockDto>> HandleAsync(GetContentBlocksQuery query, CancellationToken cancellationToken = default)
    {
        var blocks = await repository.ListAsync(query.Kind, query.PublishedOnly, cancellationToken);
        return blocks
            .OrderBy(b => b.Kind)
            .ThenBy(b => b.DisplayOrder)
            .ThenBy(b => b.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(b => b.ToDto())
            .ToList();
    }
}
