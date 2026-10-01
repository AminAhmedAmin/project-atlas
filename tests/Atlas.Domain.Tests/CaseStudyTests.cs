using Atlas.Domain.Common;
using Atlas.Domain.Portfolio;

namespace Atlas.Domain.Tests;

public sealed class CaseStudyTests
{
    [Fact]
    public void Create_normalizes_slug_tags_and_highlights()
    {
        var study = CaseStudy.Create(
            SiteLanguage.English,
            new CaseStudyFields("Clinic-App", "Clinic app", "Booking app", Tags: "Web, iOS ,, web، Android", Highlights: "40% more bookings\n\n4.8 rating "),
            TestTime.Now);

        Assert.Equal("clinic-app", study.Slug);
        Assert.Equal("Web, iOS, Android", study.Tags);
        Assert.Equal(["Web", "iOS", "Android"], study.TagList);
        Assert.Equal(["40% more bookings", "4.8 rating"], study.HighlightList);
    }

    [Theory]
    [InlineData("has space")]
    [InlineData("-leading")]
    [InlineData("double--dash")]
    [InlineData("تطبيق")]
    public void Invalid_slugs_are_rejected(string slug)
    {
        Assert.Throws<DomainException>(() =>
            CaseStudy.Create(SiteLanguage.English, new CaseStudyFields(slug, "T", "S"), TestTime.Now));
    }

    [Theory]
    [InlineData("Clinic Booking App!", "clinic-booking-app")]
    [InlineData("  E-commerce   for Riyadh Foods ", "e-commerce-for-riyadh-foods")]
    [InlineData("تطبيق حجز", null)]
    public void Slug_suggestions(string title, string? expected)
    {
        Assert.Equal(expected, CaseStudy.SuggestSlug(title));
    }

    [Fact]
    public void Cover_image_must_be_a_safe_link()
    {
        Assert.Throws<DomainException>(() =>
            CaseStudy.Create(SiteLanguage.English, new CaseStudyFields("a", "T", "S", CoverImageUrl: "javascript:x"), TestTime.Now));
    }
}
