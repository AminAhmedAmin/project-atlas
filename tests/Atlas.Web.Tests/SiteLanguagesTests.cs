using Atlas.Domain.Common;
using Atlas.Web.Localization;

namespace Atlas.Web.Tests;

public sealed class SiteLanguagesTests
{
    [Theory]
    [InlineData("/", SiteLanguage.English)]
    [InlineData("/services", SiteLanguage.English)]
    [InlineData("/arabic-food", SiteLanguage.English)]
    [InlineData("/ar", SiteLanguage.Arabic)]
    [InlineData("/ar/", SiteLanguage.Arabic)]
    [InlineData("/AR/contact", SiteLanguage.Arabic)]
    public void Language_comes_from_the_path(string path, SiteLanguage expected)
    {
        Assert.Equal(expected, SiteLanguages.FromPath(path));
    }

    [Theory]
    [InlineData("/", "/ar")]
    [InlineData("/services", "/ar/services")]
    [InlineData("/work/my-app", "/ar/work/my-app")]
    public void Localize_and_neutralize_round_trip(string neutral, string arabic)
    {
        Assert.Equal(arabic, SiteLanguages.Localize(neutral, SiteLanguage.Arabic));
        Assert.Equal(neutral, SiteLanguages.Localize(neutral, SiteLanguage.English));
        Assert.Equal(neutral, SiteLanguages.NeutralPath(arabic));
        Assert.Equal(neutral, SiteLanguages.NeutralPath(neutral));
    }

    [Fact]
    public void Arabic_is_right_to_left()
    {
        Assert.Equal("rtl", SiteLanguages.Direction(SiteLanguage.Arabic));
        Assert.Equal("ltr", SiteLanguages.Direction(SiteLanguage.English));
        Assert.Equal("ar", SiteLanguages.Code(SiteLanguage.Arabic));
    }
}
