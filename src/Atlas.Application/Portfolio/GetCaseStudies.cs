using Atlas.Application.Abstractions;
using Atlas.Domain.Common;

namespace Atlas.Application.Portfolio;

/// <param name="Language">Only this language, or all languages when null (dashboard).</param>
public sealed record GetCaseStudiesQuery(SiteLanguage? Language, bool PublishedOnly, bool FeaturedOnly = false);

public sealed class GetCaseStudiesHandler(ICaseStudyRepository repository)
    : IQueryHandler<GetCaseStudiesQuery, IReadOnlyList<CaseStudyDto>>
{
    public async Task<IReadOnlyList<CaseStudyDto>> HandleAsync(GetCaseStudiesQuery query, CancellationToken cancellationToken = default)
    {
        var studies = await repository.ListAsync(query.Language, query.PublishedOnly, query.FeaturedOnly, cancellationToken);
        return studies
            .OrderBy(s => s.DisplayOrder)
            .ThenByDescending(s => s.UpdatedAtUtc)
            .Select(s => s.ToDto())
            .ToList();
    }
}

/// <summary>A published case study by its URL name, or null.</summary>
public sealed record GetCaseStudyQuery(SiteLanguage Language, string Slug);

public sealed class GetCaseStudyHandler(ICaseStudyRepository repository) : IQueryHandler<GetCaseStudyQuery, CaseStudyDto?>
{
    public async Task<CaseStudyDto?> HandleAsync(GetCaseStudyQuery query, CancellationToken cancellationToken = default)
    {
        if (!Domain.Portfolio.CaseStudy.IsValidSlug(query.Slug?.ToLowerInvariant()))
        {
            return null;
        }

        var study = await repository.GetBySlugAsync(query.Language, query.Slug!.ToLowerInvariant(), cancellationToken);
        return study is { IsPublished: true } ? study.ToDto() : null;
    }
}
