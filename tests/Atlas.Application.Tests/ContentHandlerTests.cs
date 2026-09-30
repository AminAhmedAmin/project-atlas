using Atlas.Application.Content;
using Atlas.Application.Tests.Fakes;
using Atlas.Domain.Content;

namespace Atlas.Application.Tests;

public sealed class ContentHandlerTests
{
    private readonly InMemoryPageContentRepository _pages = new();
    private readonly InMemoryServiceRepository _services = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task Get_page_falls_back_to_default_content()
    {
        var page = await new GetPageContentHandler(_pages).HandleAsync(new GetPageContentQuery(PageKey.About), TestContext.Current.CancellationToken);

        Assert.Equal(DefaultContent.For(PageKey.About).Title, page.Title);
        Assert.Null(page.UpdatedAtUtc);
    }

    [Fact]
    public async Task Update_page_creates_then_updates()
    {
        var handler = new UpdatePageContentHandler(_pages, _unitOfWork, new UpdatePageContentValidator(), TestData.Clock());
        var command = new UpdatePageContentCommand(PageKey.Home, "Hello", "Sub", null, "Go", "/contact", "Meta");

        var created = await handler.HandleAsync(command, TestContext.Current.CancellationToken);
        var updated = await handler.HandleAsync(command with { Title = "Hello again" }, TestContext.Current.CancellationToken);

        Assert.True(created.IsSuccess);
        Assert.True(updated.IsSuccess);
        Assert.Equal("Hello again", Assert.Single(_pages.Pages).Title);
    }

    [Fact]
    public async Task Update_page_rejects_unsafe_link_without_throwing()
    {
        var handler = new UpdatePageContentHandler(_pages, _unitOfWork, new UpdatePageContentValidator(), TestData.Clock());

        var result = await handler.HandleAsync(
            new UpdatePageContentCommand(PageKey.Home, "Hello", null, null, "Go", "javascript:alert(1)", null),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Empty(_pages.Pages);
    }

    [Fact]
    public async Task Update_page_requires_button_text_and_link_together()
    {
        var handler = new UpdatePageContentHandler(_pages, _unitOfWork, new UpdatePageContentValidator(), TestData.Clock());

        var result = await handler.HandleAsync(
            new UpdatePageContentCommand(PageKey.Home, "Hello", null, null, "Go", null, null),
            TestContext.Current.CancellationToken);

        Assert.Equal("CallToActionUrl", Assert.Single(result.Errors).Field);
    }

    [Fact]
    public async Task Service_crud()
    {
        var save = new SaveServiceHandler(_services, _unitOfWork, new SaveServiceValidator(), TestData.Clock());

        var created = await save.HandleAsync(new SaveServiceCommand(null, "Web", "Web apps", null, "web", 2, true), TestContext.Current.CancellationToken);
        Assert.True(created.IsSuccess);

        var updated = await save.HandleAsync(new SaveServiceCommand(created.Value.Id, "Web+", "Web apps", null, "web", 1, false), TestContext.Current.CancellationToken);
        Assert.Equal("Web+", updated.Value.Title);
        Assert.False(updated.Value.IsPublished);

        var deleted = await new DeleteServiceHandler(_services, _unitOfWork).HandleAsync(new DeleteServiceCommand(created.Value.Id), TestContext.Current.CancellationToken);
        Assert.True(deleted.IsSuccess);
        Assert.Empty(_services.Services);
    }

    [Fact]
    public async Task Update_missing_service_returns_not_found()
    {
        var save = new SaveServiceHandler(_services, _unitOfWork, new SaveServiceValidator(), TestData.Clock());

        var result = await save.HandleAsync(new SaveServiceCommand(Guid.NewGuid(), "Web", "Web apps", null, null, 0, true), TestContext.Current.CancellationToken);

        Assert.Equal(Common.ErrorType.NotFound, Assert.Single(result.Errors).Type);
    }

    [Fact]
    public async Task Public_services_are_published_only_and_ordered()
    {
        var now = TestData.Now.UtcDateTime;
        _services.Add(Service.Create("B", "s", null, null, 2, true, now));
        _services.Add(Service.Create("A", "s", null, null, 1, true, now));
        _services.Add(Service.Create("Hidden", "s", null, null, 0, false, now));

        var result = await new GetServicesHandler(_services).HandleAsync(new GetServicesQuery(PublishedOnly: true), TestContext.Current.CancellationToken);

        Assert.Equal(["A", "B"], result.Select(s => s.Title));
    }
}
