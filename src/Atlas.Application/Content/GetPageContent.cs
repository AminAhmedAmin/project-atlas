using Atlas.Application.Abstractions;
using Atlas.Domain.Common;
using Atlas.Domain.Content;

namespace Atlas.Application.Content;

public sealed record GetPageContentQuery(PageKey Key, SiteLanguage Language = SiteLanguage.English);

public sealed class GetPageContentHandler(IPageContentRepository repository)
    : IQueryHandler<GetPageContentQuery, PageContentDto>
{
    public async Task<PageContentDto> HandleAsync(GetPageContentQuery query, CancellationToken cancellationToken = default)
    {
        var page = await repository.GetAsync(query.Key, query.Language, cancellationToken);
        return page?.ToDto() ?? DefaultContent.For(query.Key, query.Language).ToDto(query.Key, query.Language);
    }
}
