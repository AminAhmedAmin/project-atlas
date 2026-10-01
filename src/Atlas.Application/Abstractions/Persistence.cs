using Atlas.Application.Common;
using Atlas.Domain.Contact;
using Atlas.Domain.Content;
using Atlas.Domain.Settings;

namespace Atlas.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ISiteSettingsRepository
{
    Task<SiteSettings?> GetAsync(CancellationToken cancellationToken = default);

    void Add(SiteSettings settings);
}

public interface IPageContentRepository
{
    Task<PageContent?> GetAsync(PageKey key, CancellationToken cancellationToken = default);

    void Add(PageContent page);
}

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Service>> ListAsync(bool publishedOnly, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    void Add(Service service);

    void Remove(Service service);
}

public interface IContentBlockRepository
{
    Task<ContentBlock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Blocks ordered by kind and display order, optionally filtered.</summary>
    Task<IReadOnlyList<ContentBlock>> ListAsync(BlockKind? kind, bool publishedOnly, CancellationToken cancellationToken = default);

    void Add(ContentBlock block);

    void Remove(ContentBlock block);
}

public sealed record ContactMessageSearch(string? Text, bool UnreadOnly, int Page, int PageSize);

public interface IContactMessageRepository
{
    Task<ContactMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Newest first, filtered by free text over name, e-mail, subject and message.</summary>
    Task<PagedResult<ContactMessage>> SearchAsync(ContactMessageSearch search, CancellationToken cancellationToken = default);

    Task<int> CountAsync(bool unreadOnly, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DateTime>> GetReceivedTimesSinceAsync(DateTime fromUtc, CancellationToken cancellationToken = default);

    void Add(ContactMessage message);

    void Remove(ContactMessage message);
}
