using Atlas.Domain.Common;
using Atlas.Web.Localization;

namespace Atlas.Web.Tests;

public sealed class UiTextTests
{
    [Fact]
    public void English_returns_the_key()
    {
        Assert.Equal("Services", UiText.Get(SiteLanguage.English, "Services"));
    }

    [Fact]
    public void Arabic_returns_the_translation_or_falls_back_to_english()
    {
        Assert.Equal("خدماتنا", UiText.Get(SiteLanguage.Arabic, "Services"));
        Assert.Equal("Untranslated text", UiText.Get(SiteLanguage.Arabic, "Untranslated text"));
    }

    [Fact]
    public void Every_translation_contains_arabic_text()
    {
        foreach (var key in UiText.TranslatedKeys)
        {
            var value = UiText.Get(SiteLanguage.Arabic, key);
            Assert.True(value.Any(c => c is >= '؀' and <= 'ۿ'), $"'{key}' has no Arabic translation.");
        }
    }
}
