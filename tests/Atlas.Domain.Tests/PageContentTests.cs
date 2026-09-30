using Atlas.Domain.Common;
using Atlas.Domain.Content;

namespace Atlas.Domain.Tests;

public sealed class PageContentTests
{
    [Fact]
    public void Create_sets_text()
    {
        var page = PageContent.Create(
            PageKey.Home,
            new PageText("Welcome", "Sub", "Body", "Contact us", "/contact", "Meta"),
            TestTime.Now);

        Assert.Equal(PageKey.Home, page.Key);
        Assert.Equal("Welcome", page.Title);
        Assert.Equal("/contact", page.CallToActionUrl);
    }

    [Theory]
    [InlineData("/contact")]
    [InlineData("https://example.com/book")]
    public void Call_to_action_accepts_site_paths_and_http_links(string url)
    {
        var page = PageContent.Create(PageKey.Home, new PageText("T", CallToActionText: "Go", CallToActionUrl: url), TestTime.Now);

        Assert.Equal(url, page.CallToActionUrl);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("//evil.example.com")]
    [InlineData("contact")]
    public void Call_to_action_rejects_unsafe_links(string url)
    {
        Assert.Throws<DomainException>(() =>
            PageContent.Create(PageKey.Home, new PageText("T", CallToActionText: "Go", CallToActionUrl: url), TestTime.Now));
    }

    [Fact]
    public void Call_to_action_text_and_link_must_come_together()
    {
        Assert.Throws<DomainException>(() =>
            PageContent.Create(PageKey.Home, new PageText("T", CallToActionText: "Go"), TestTime.Now));
    }

    [Fact]
    public void Unknown_page_key_is_rejected()
    {
        Assert.Throws<DomainException>(() => PageContent.Create((PageKey)99, new PageText("T"), TestTime.Now));
    }

    [Fact]
    public void Meta_description_is_limited_for_seo()
    {
        var meta = new string('m', PageContent.MetaDescriptionMaxLength + 1);

        Assert.Throws<DomainException>(() =>
            PageContent.Create(PageKey.About, new PageText("T", MetaDescription: meta), TestTime.Now));
    }
}
