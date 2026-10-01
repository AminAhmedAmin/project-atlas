using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Contact;
using Atlas.Domain.Content;
using Atlas.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class SiteSettingsRepository(AtlasDbContext db) : ISiteSettingsRepository
{
    public Task<SiteSettings?> GetAsync(CancellationToken cancellationToken = default) =>
        db.SiteSettings.OrderBy(s => s.Id).FirstOrDefaultAsync(cancellationToken);

    public void Add(SiteSettings settings) => db.SiteSettings.Add(settings);
}

internal sealed class PageContentRepository(AtlasDbContext db) : IPageContentRepository
{
    public Task<PageContent?> GetAsync(PageKey key, CancellationToken cancellationToken = default) =>
        db.PageContents.FirstOrDefaultAsync(p => p.Key == key, cancellationToken);

    public void Add(PageContent page) => db.PageContents.Add(page);
}

internal sealed class ServiceRepository(AtlasDbContext db) : IServiceRepository
{
    public Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Services.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Service>> ListAsync(bool publishedOnly, CancellationToken cancellationToken = default)
    {
        var query = db.Services.AsNoTracking();
        if (publishedOnly)
        {
            query = query.Where(s => s.IsPublished);
        }

        return await query.OrderBy(s => s.DisplayOrder).ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        db.Services.CountAsync(cancellationToken);

    public void Add(Service service) => db.Services.Add(service);

    public void Remove(Service service) => db.Services.Remove(service);
}

internal sealed class ContentBlockRepository(AtlasDbContext db) : IContentBlockRepository
{
    public Task<ContentBlock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.ContentBlocks.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ContentBlock>> ListAsync(BlockKind? kind, bool publishedOnly, CancellationToken cancellationToken = default)
    {
        var query = db.ContentBlocks.AsNoTracking();
        if (kind is { } k)
        {
            query = query.Where(b => b.Kind == k);
        }

        if (publishedOnly)
        {
            query = query.Where(b => b.IsPublished);
        }

        return await query.OrderBy(b => b.Kind).ThenBy(b => b.DisplayOrder).ToListAsync(cancellationToken);
    }

    public void Add(ContentBlock block) => db.ContentBlocks.Add(block);

    public void Remove(ContentBlock block) => db.ContentBlocks.Remove(block);
}

internal sealed class ContactMessageRepository(AtlasDbContext db) : IContactMessageRepository
{
    public Task<ContactMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.ContactMessages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<PagedResult<ContactMessage>> SearchAsync(ContactMessageSearch search, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(search);

        var query = db.ContactMessages.AsNoTracking();
        if (search.UnreadOnly)
        {
            query = query.Where(m => !m.IsRead);
        }

        if (!string.IsNullOrWhiteSpace(search.Text))
        {
            var text = search.Text;
            query = query.Where(m =>
                m.Name.Contains(text)
                || m.Email.Value.Contains(text)
                || (m.Subject != null && m.Subject.Contains(text))
                || m.Message.Contains(text));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(m => m.ReceivedAtUtc)
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ContactMessage>(items, total, search.Page, search.PageSize);
    }

    public Task<int> CountAsync(bool unreadOnly, CancellationToken cancellationToken = default) =>
        unreadOnly
            ? db.ContactMessages.CountAsync(m => !m.IsRead, cancellationToken)
            : db.ContactMessages.CountAsync(cancellationToken);

    public async Task<IReadOnlyList<DateTime>> GetReceivedTimesSinceAsync(DateTime fromUtc, CancellationToken cancellationToken = default) =>
        await db.ContactMessages
            .Where(m => m.ReceivedAtUtc >= fromUtc)
            .Select(m => m.ReceivedAtUtc)
            .ToListAsync(cancellationToken);

    public void Add(ContactMessage message) => db.ContactMessages.Add(message);

    public void Remove(ContactMessage message) => db.ContactMessages.Remove(message);
}
