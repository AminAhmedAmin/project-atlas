using Atlas.Domain.Common;
using Atlas.Domain.Content;

namespace Atlas.Domain.Tests;

public sealed class ContentBlockTests
{
    [Fact]
    public void Create_sets_fields()
    {
        var block = ContentBlock.Create(
            BlockKind.Testimonial,
            new ContentBlockFields(" Sara ", "CTO, Example Co", "Great team.", null, 2, true),
            TestTime.Now);

        Assert.Equal(BlockKind.Testimonial, block.Kind);
        Assert.Equal("Sara", block.Title);
        Assert.Equal("Great team.", block.Text);
        Assert.Equal(2, block.DisplayOrder);
    }

    [Theory]
    [InlineData(BlockKind.Stat)]
    [InlineData(BlockKind.ProcessStep)]
    [InlineData(BlockKind.Testimonial)]
    [InlineData(BlockKind.Faq)]
    public void Text_is_required_except_for_client_logos(BlockKind kind)
    {
        Assert.Throws<DomainException>(() => ContentBlock.Create(kind, new ContentBlockFields("Title"), TestTime.Now));
    }

    [Fact]
    public void Client_logo_needs_only_a_name()
    {
        var block = ContentBlock.Create(BlockKind.ClientLogo, new ContentBlockFields("Acme"), TestTime.Now);

        Assert.Null(block.Text);
        Assert.Null(block.ImageUrl);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("//evil.example.com/logo.png")]
    public void Image_url_must_be_safe(string url)
    {
        Assert.Throws<DomainException>(() =>
            ContentBlock.Create(BlockKind.ClientLogo, new ContentBlockFields("Acme", ImageUrl: url), TestTime.Now));
    }

    [Fact]
    public void Negative_order_and_unknown_kind_are_rejected()
    {
        Assert.Throws<DomainException>(() =>
            ContentBlock.Create(BlockKind.Faq, new ContentBlockFields("Q", Text: "A", DisplayOrder: -1), TestTime.Now));
        Assert.Throws<DomainException>(() =>
            ContentBlock.Create((BlockKind)42, new ContentBlockFields("Q", Text: "A"), TestTime.Now));
    }
}
