using Atlas.Application.Abstractions;
using Atlas.Domain.Common;
using Atlas.Domain.Content;

namespace Atlas.Application.Content;

/// <param name="Kind">Only this kind, or all kinds when null.</param>
/// <param name="PublishedOnly">True for the public site; false for the dashboard.</param>
/// <param name="Language">Only blocks in this language, or all languages when null.</param>
public sealed record GetContentBlocksQuery(BlockKind? Kind, bool PublishedOnly, SiteLanguage? Language = null);

public sealed class GetContentBlocksHandler(IContentBlockRepository repository)
    : IQueryHandler<GetContentBlocksQuery, IReadOnlyList<ContentBlockDto>>
{
    public async Task<IReadOnlyList<ContentBlockDto>> HandleAsync(GetContentBlocksQuery query, CancellationToken cancellationToken = default)
    {
        var blocks = await repository.ListAsync(query.Language, query.Kind, query.PublishedOnly, cancellationToken);
        return blocks
            .OrderBy(b => b.Kind)
            .ThenBy(b => b.DisplayOrder)
            .ThenBy(b => b.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(b => b.ToDto())
            .ToList();
    }
}
