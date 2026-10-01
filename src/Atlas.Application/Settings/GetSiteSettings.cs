using Atlas.Application.Abstractions;

namespace Atlas.Application.Settings;

public sealed record GetSiteSettingsQuery;

public sealed class GetSiteSettingsHandler(ISiteSettingsRepository repository, BrandingDefaults defaults)
    : IQueryHandler<GetSiteSettingsQuery, SiteSettingsDto>
{
    public async Task<SiteSettingsDto> HandleAsync(GetSiteSettingsQuery query, CancellationToken cancellationToken = default)
    {
        var settings = await repository.GetAsync(cancellationToken);
        return settings?.ToDto() ?? defaults.ToDto();
    }
}
