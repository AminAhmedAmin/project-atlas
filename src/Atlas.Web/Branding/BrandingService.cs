using Atlas.Application.Abstractions;
using Atlas.Application.Settings;

namespace Atlas.Web.Branding;

/// <summary>
/// Caches the site branding for all circuits and notifies them when it changes,
/// so a save in the dashboard is reflected on every open page immediately.
/// </summary>
public sealed class BrandingService(IServiceScopeFactory scopeFactory) : IDisposable
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private SiteSettingsDto? _current;

    public event Action<SiteSettingsDto>? Changed;

    public async Task<SiteSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        if (_current is { } cached)
        {
            return cached;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_current is null)
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetSiteSettingsQuery, SiteSettingsDto>>();
                _current = await handler.HandleAsync(new GetSiteSettingsQuery(), cancellationToken);
            }

            return _current;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>Call after saving settings; reloads the cache and notifies subscribers.</summary>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _current = null;
        var settings = await GetAsync(cancellationToken);
        Changed?.Invoke(settings);
    }

    public void Dispose() => _lock.Dispose();
}
