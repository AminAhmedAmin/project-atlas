using Atlas.Domain.Common;
using Atlas.Domain.Contact;
using Atlas.Domain.Settings;

namespace Atlas.Domain.Tests;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData("+966 55 123 4567")]
    [InlineData("0551234567")]
    [InlineData("(011) 456-7890")]
    public void Typical_numbers_are_valid(string value)
    {
        Assert.True(PhoneNumber.IsValid(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("call me maybe")]
    [InlineData("+966+55 123 4567")]
    [InlineData("1234567890123456")]
    public void Invalid_numbers_are_rejected(string value)
    {
        Assert.False(PhoneNumber.IsValid(value));
    }

    [Theory]
    [InlineData("+966 55 123 4567", "966551234567")]
    [InlineData("0551234567", "966551234567")]
    [InlineData("00966551234567", "966551234567")]
    [InlineData("+44 20 7946 0958", "442079460958")]
    public void International_digits_for_whatsapp(string value, string expected)
    {
        Assert.Equal(expected, PhoneNumber.ToInternationalDigits(value));
    }

    [Fact]
    public void Contact_message_keeps_details()
    {
        var message = ContactMessage.Create(
            "Sara", EmailAddress.Create("sara@example.com"), null, "Hi", TestTime.Now,
            new ContactDetails(" 0551234567 ", "Mobile apps", "50k-150k", SiteLanguage.Arabic));

        Assert.Equal("0551234567", message.Phone);
        Assert.Equal("Mobile apps", message.Service);
        Assert.Equal("50k-150k", message.Budget);
        Assert.Equal(SiteLanguage.Arabic, message.Language);
    }

    [Fact]
    public void Contact_message_rejects_invalid_phone()
    {
        Assert.Throws<DomainException>(() => ContactMessage.Create(
            "Sara", EmailAddress.Create("sara@example.com"), null, "Hi", TestTime.Now, new ContactDetails("abc")));
    }

    [Fact]
    public void WhatsApp_number_is_normalized_and_can_be_cleared()
    {
        var settings = SiteSettings.Create("Atlas", null, HexColor.Create("#123456"), null, TestTime.Now);

        settings.SetWhatsApp("055 123 4567", TestTime.Now);
        Assert.Equal("966551234567", settings.WhatsAppNumber);

        settings.SetWhatsApp(" ", TestTime.Now);
        Assert.Null(settings.WhatsAppNumber);

        Assert.Throws<DomainException>(() => settings.SetWhatsApp("not a number", TestTime.Now));
    }
}
