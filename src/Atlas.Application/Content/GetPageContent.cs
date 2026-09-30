using Atlas.Application.Abstractions;
using Atlas.Domain.Content;

namespace Atlas.Application.Content;

public sealed record GetPageContentQuery(PageKey Key);

public sealed class GetPageContentHandler(IPageContentRepository repository)
    : IQueryHandler<GetPageContentQuery, PageContentDto>
{
    public async Task<PageContentDto> HandleAsync(GetPageContentQuery query, CancellationToken cancellationToken = default)
    {
        var page = await repository.GetAsync(query.Key, cancellationToken);
        return page?.ToDto() ?? DefaultContent.For(query.Key).ToDto(query.Key);
    }
}
