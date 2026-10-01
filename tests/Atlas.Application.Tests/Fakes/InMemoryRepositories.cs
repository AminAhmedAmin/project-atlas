using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Common;
using Atlas.Domain.Contact;
using Atlas.Domain.Content;
using Atlas.Domain.Settings;

namespace Atlas.Application.Tests.Fakes;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}

internal sealed class InMemorySiteSettingsRepository : ISiteSettingsRepository
{
    public SiteSettings? Settings { get; set; }

    public Task<SiteSettings?> GetAsync(CancellationToken cancellationToken = default) => Task.FromResult(Settings);

    public void Add(SiteSettings settings) => Settings = settings;
}

internal sealed class InMemoryPageContentRepository : IPageContentRepository
{
    public List<PageContent> Pages { get; } = [];

    public Task<PageContent?> GetAsync(PageKey key, SiteLanguage language, CancellationToken cancellationToken = default) =>
        Task.FromResult(Pages.FirstOrDefault(p => p.Key == key && p.Language == language));

    public void Add(PageContent page) => Pages.Add(page);
}

internal sealed class InMemoryServiceRepository : IServiceRepository
{
    public List<Service> Services { get; } = [];

    public Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Services.FirstOrDefault(s => s.Id == id));

    public Task<IReadOnlyList<Service>> ListAsync(SiteLanguage? language, bool publishedOnly, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Service>>(Services
            .Where(s => language is null || s.Language == language)
            .Where(s => !publishedOnly || s.IsPublished)
            .ToList());

    public Task<int> CountAsync(CancellationToken cancellationToken = default) => Task.FromResult(Services.Count);

    public void Add(Service service) => Services.Add(service);

    public void Remove(Service service) => Services.Remove(service);
}

internal sealed class InMemoryContactMessageRepository : IContactMessageRepository
{
    public List<ContactMessage> Messages { get; } = [];

    public Task<ContactMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Messages.FirstOrDefault(m => m.Id == id));

    public Task<PagedResult<ContactMessage>> SearchAsync(ContactMessageSearch search, CancellationToken cancellationToken = default)
    {
        var query = Messages.AsEnumerable();
        if (search.UnreadOnly)
        {
            query = query.Where(m => !m.IsRead);
        }

        if (search.Text is { } text)
        {
            query = query.Where(m =>
                m.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
                || m.Email.Value.Contains(text, StringComparison.OrdinalIgnoreCase)
                || (m.Subject?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false)
                || m.Message.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        var all = query.OrderByDescending(m => m.ReceivedAtUtc).ToList();
        var items = all.Skip((search.Page - 1) * search.PageSize).Take(search.PageSize).ToList();
        return Task.FromResult(new PagedResult<ContactMessage>(items, all.Count, search.Page, search.PageSize));
    }

    public Task<int> CountAsync(bool unreadOnly, CancellationToken cancellationToken = default) =>
        Task.FromResult(Messages.Count(m => !unreadOnly || !m.IsRead));

    public Task<IReadOnlyList<DateTime>> GetReceivedTimesSinceAsync(DateTime fromUtc, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<DateTime>>(Messages.Where(m => m.ReceivedAtUtc >= fromUtc).Select(m => m.ReceivedAtUtc).ToList());

    public void Add(ContactMessage message) => Messages.Add(message);

    public void Remove(ContactMessage message) => Messages.Remove(message);
}

internal sealed class InMemoryContentBlockRepository : IContentBlockRepository
{
    public List<ContentBlock> Blocks { get; } = [];

    public Task<ContentBlock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Blocks.FirstOrDefault(b => b.Id == id));

    public Task<IReadOnlyList<ContentBlock>> ListAsync(SiteLanguage? language, BlockKind? kind, bool publishedOnly, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ContentBlock>>(Blocks
            .Where(b => language is null || b.Language == language)
            .Where(b => kind is null || b.Kind == kind)
            .Where(b => !publishedOnly || b.IsPublished)
            .ToList());

    public void Add(ContentBlock block) => Blocks.Add(block);

    public void Remove(ContentBlock block) => Blocks.Remove(block);
}

internal sealed class InMemoryCaseStudyRepository : ICaseStudyRepository
{
    public List<Domain.Portfolio.CaseStudy> Studies { get; } = [];

    public Task<Domain.Portfolio.CaseStudy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Studies.FirstOrDefault(s => s.Id == id));

    public Task<Domain.Portfolio.CaseStudy?> GetBySlugAsync(SiteLanguage language, string slug, CancellationToken cancellationToken = default) =>
        Task.FromResult(Studies.FirstOrDefault(s => s.Language == language && s.Slug == slug));

    public Task<IReadOnlyList<Domain.Portfolio.CaseStudy>> ListAsync(SiteLanguage? language, bool publishedOnly, bool featuredOnly, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Domain.Portfolio.CaseStudy>>(Studies
            .Where(s => language is null || s.Language == language)
            .Where(s => !publishedOnly || s.IsPublished)
            .Where(s => !featuredOnly || s.IsFeatured)
            .ToList());

    public Task<bool> SlugExistsAsync(SiteLanguage language, string slug, Guid? excludingId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Studies.Any(s => s.Language == language && s.Slug == slug && s.Id != excludingId));

    public void Add(Domain.Portfolio.CaseStudy caseStudy) => Studies.Add(caseStudy);

    public void Remove(Domain.Portfolio.CaseStudy caseStudy) => Studies.Remove(caseStudy);
}
