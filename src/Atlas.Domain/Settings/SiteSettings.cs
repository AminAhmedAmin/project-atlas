using Atlas.Domain.Common;

namespace Atlas.Domain.Settings;

/// <summary>Branding and contact settings for the whole site (a single row).</summary>
public sealed class SiteSettings : Entity
{
    public const int CompanyNameMaxLength = 100;
    public const int TaglineMaxLength = 200;
    public const int LogoUrlMaxLength = 500;

    private SiteSettings()
    {
        CompanyName = string.Empty;
        PrimaryColor = HexColor.Create("#000000");
    }

    public string CompanyName { get; private set; }

    public string? Tagline { get; private set; }

    public string? LogoUrl { get; private set; }

    /// <summary>WhatsApp number in international digits (e.g. 9665XXXXXXXX), used for the WhatsApp button.</summary>
    public string? WhatsAppNumber { get; private set; }

    /// <summary>Telegram chat (person or group) that receives alerts about new messages and chats.</summary>
    public long? TelegramChatId { get; private set; }

    /// <summary>Display name of <see cref="TelegramChatId"/>, so the dashboard can show who receives alerts.</summary>
    public string? TelegramChatTitle { get; private set; }

    /// <summary>Company name shown on the Arabic site. Falls back to <see cref="CompanyName"/>.</summary>
    public string? ArabicCompanyName { get; private set; }

    /// <summary>Tagline shown on the Arabic site. Falls back to <see cref="Tagline"/>.</summary>
    public string? ArabicTagline { get; private set; }

    public HexColor PrimaryColor { get; private set; }

    public EmailAddress? ContactEmail { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static SiteSettings Create(
        string companyName,
        string? tagline,
        HexColor primaryColor,
        EmailAddress? contactEmail,
        DateTime nowUtc)
    {
        var settings = new SiteSettings();
        settings.Update(companyName, tagline, primaryColor, contactEmail, nowUtc);
        return settings;
    }

    public void Update(
        string companyName,
        string? tagline,
        HexColor primaryColor,
        EmailAddress? contactEmail,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(primaryColor);

        CompanyName = Guard.Required(companyName, "Company name", CompanyNameMaxLength);
        Tagline = Guard.Optional(tagline, "Tagline", TaglineMaxLength);
        PrimaryColor = primaryColor;
        ContactEmail = contactEmail;
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    public void SetLogo(string logoUrl, DateTime nowUtc)
    {
        LogoUrl = Guard.Required(logoUrl, "Logo URL", LogoUrlMaxLength);
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    public void UpdateArabic(string? arabicCompanyName, string? arabicTagline, DateTime nowUtc)
    {
        ArabicCompanyName = Guard.Optional(arabicCompanyName, "Arabic company name", CompanyNameMaxLength);
        ArabicTagline = Guard.Optional(arabicTagline, "Arabic tagline", TaglineMaxLength);
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    public void SetWhatsApp(string? number, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            WhatsAppNumber = null;
        }
        else if (PhoneNumber.IsValid(number))
        {
            WhatsAppNumber = PhoneNumber.ToInternationalDigits(number);
        }
        else
        {
            throw new DomainException("WhatsApp number is not valid.");
        }

        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    public void ConnectTelegram(long chatId, string? chatTitle, DateTime nowUtc)
    {
        if (chatId == 0)
        {
            throw new DomainException("Telegram chat id is not valid.");
        }

        TelegramChatId = chatId;
        TelegramChatTitle = Guard.Optional(chatTitle, "Telegram chat name", CompanyNameMaxLength);
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    public void DisconnectTelegram(DateTime nowUtc)
    {
        TelegramChatId = null;
        TelegramChatTitle = null;
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    public void RemoveLogo(DateTime nowUtc)
    {
        LogoUrl = null;
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }
}
