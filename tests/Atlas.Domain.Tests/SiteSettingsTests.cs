using Atlas.Domain.Common;
using Atlas.Domain.Settings;

namespace Atlas.Domain.Tests;

public sealed class SiteSettingsTests
{
    private static SiteSettings NewSettings() =>
        SiteSettings.Create("Atlas", "Tagline", HexColor.Create("#123456"), null, TestTime.Now);

    [Fact]
    public void Create_sets_branding()
    {
        var settings = NewSettings();

        Assert.Equal("Atlas", settings.CompanyName);
        Assert.Equal("#123456", settings.PrimaryColor.Value);
        Assert.Null(settings.LogoUrl);
    }

    [Fact]
    public void Company_name_is_required()
    {
        Assert.Throws<DomainException>(() =>
            SiteSettings.Create("  ", null, HexColor.Create("#123456"), null, TestTime.Now));
    }

    [Fact]
    public void Logo_can_be_set_and_removed()
    {
        var settings = NewSettings();

        settings.SetLogo("/uploads/logo.png", TestTime.Now.AddMinutes(1));
        Assert.Equal("/uploads/logo.png", settings.LogoUrl);

        settings.RemoveLogo(TestTime.Now.AddMinutes(2));
        Assert.Null(settings.LogoUrl);
        Assert.Equal(TestTime.Now.AddMinutes(2), settings.UpdatedAtUtc);
    }

    [Fact]
    public void Update_changes_contact_email()
    {
        var settings = NewSettings();

        settings.Update("New Co", null, HexColor.Create("#000000"), EmailAddress.Create("hi@example.com"), TestTime.Now);

        Assert.Equal("New Co", settings.CompanyName);
        Assert.Equal("hi@example.com", settings.ContactEmail?.Value);
    }
}

public sealed class ArabicSettingsTests
{
    [Fact]
    public void Arabic_name_and_tagline_are_optional()
    {
        var settings = SiteSettings.Create("Atlas", null, HexColor.Create("#123456"), null, TestTime.Now);

        settings.UpdateArabic("  أطلس ", "  ", TestTime.Now);

        Assert.Equal("أطلس", settings.ArabicCompanyName);
        Assert.Null(settings.ArabicTagline);
    }

    [Fact]
    public void Unknown_language_is_rejected()
    {
        Assert.Throws<DomainException>(() =>
            Atlas.Domain.Content.PageContent.Create(Atlas.Domain.Content.PageKey.Home, new Atlas.Domain.Content.PageText("T"), TestTime.Now, (SiteLanguage)9));
    }
}
