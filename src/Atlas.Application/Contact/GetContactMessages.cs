using Atlas.Application.Abstractions;
using Atlas.Application.Common;

namespace Atlas.Application.Contact;

public sealed record GetContactMessagesQuery(string? Search = null, bool UnreadOnly = false, int Page = 1, int PageSize = 20);

public sealed class GetContactMessagesHandler(IContactMessageRepository repository)
    : IQueryHandler<GetContactMessagesQuery, PagedResult<ContactMessageDto>>
{
    public const int MaxPageSize = 100;

    public async Task<PagedResult<ContactMessageDto>> HandleAsync(GetContactMessagesQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        var result = await repository.SearchAsync(new ContactMessageSearch(search, query.UnreadOnly, page, pageSize), cancellationToken);
        return result.Map(m => m.ToDto());
    }
}
