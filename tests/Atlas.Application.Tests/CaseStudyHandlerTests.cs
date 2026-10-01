using Atlas.Application.Common;
using Atlas.Application.Portfolio;
using Atlas.Application.Tests.Fakes;
using Atlas.Domain.Common;

namespace Atlas.Application.Tests;

public sealed class CaseStudyHandlerTests
{
    private readonly InMemoryCaseStudyRepository _studies = new();
    private readonly InMemoryFileStorage _files = new();

    private SaveCaseStudyHandler Save() =>
        new(_studies, new FakeUnitOfWork(), _files, new SaveCaseStudyValidator(), TestData.Clock());

    private static SaveCaseStudyCommand Command(string slug, SiteLanguage language = SiteLanguage.English, Guid? id = null, bool published = true, string? cover = null) =>
        new(id, language, slug, "Title", "Summary", null, null, null, null, cover, 0, published, false);

    [Fact]
    public async Task Slug_must_be_unique_per_language()
    {
        var ct = TestContext.Current.CancellationToken;
        Assert.True((await Save().HandleAsync(Command("clinic-app"), ct)).IsSuccess);

        var duplicate = await Save().HandleAsync(Command("Clinic-App"), ct);
        var arabic = await Save().HandleAsync(Command("clinic-app", SiteLanguage.Arabic), ct);

        Assert.Equal(ErrorType.Conflict, Assert.Single(duplicate.Errors).Type);
        Assert.True(arabic.IsSuccess);
    }

    [Fact]
    public async Task Updating_keeps_its_own_slug()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await Save().HandleAsync(Command("clinic-app"), ct);

        var updated = await Save().HandleAsync(Command("clinic-app", id: created.Value.Id) with { Title = "New title" }, ct);

        Assert.True(updated.IsSuccess);
        Assert.Equal("New title", updated.Value.Title);
    }

    [Fact]
    public async Task Invalid_slug_is_a_field_error()
    {
        var result = await Save().HandleAsync(Command("not valid!"), TestContext.Current.CancellationToken);

        Assert.Equal("Slug", Assert.Single(result.Errors).Field);
    }

    [Fact]
    public async Task Public_lookup_only_returns_published_case_studies()
    {
        var ct = TestContext.Current.CancellationToken;
        await Save().HandleAsync(Command("visible"), ct);
        await Save().HandleAsync(Command("hidden", published: false), ct);
        var handler = new GetCaseStudyHandler(_studies);

        Assert.NotNull(await handler.HandleAsync(new GetCaseStudyQuery(SiteLanguage.English, "VISIBLE"), ct));
        Assert.Null(await handler.HandleAsync(new GetCaseStudyQuery(SiteLanguage.English, "hidden"), ct));
        Assert.Null(await handler.HandleAsync(new GetCaseStudyQuery(SiteLanguage.Arabic, "visible"), ct));
        Assert.Null(await handler.HandleAsync(new GetCaseStudyQuery(SiteLanguage.English, "../etc"), ct));
    }

    [Fact]
    public async Task Featured_filter_and_cover_cleanup()
    {
        var ct = TestContext.Current.CancellationToken;
        _files.Files["/uploads/content/a.png"] = [1];
        _files.Files["/uploads/content/b.png"] = [2];

        var created = await Save().HandleAsync(Command("one", cover: "/uploads/content/a.png") with { IsFeatured = true }, ct);
        await Save().HandleAsync(Command("two"), ct);
        await Save().HandleAsync(Command("one", id: created.Value.Id, cover: "/uploads/content/b.png") with { IsFeatured = true }, ct);

        var featured = await new GetCaseStudiesHandler(_studies).HandleAsync(new GetCaseStudiesQuery(SiteLanguage.English, true, FeaturedOnly: true), ct);
        Assert.Equal(["one"], featured.Select(s => s.Slug));
        Assert.Equal(["/uploads/content/b.png"], _files.Files.Keys);

        await new DeleteCaseStudyHandler(_studies, new FakeUnitOfWork(), _files).HandleAsync(new DeleteCaseStudyCommand(created.Value.Id), ct);
        Assert.Empty(_files.Files);
    }
}
