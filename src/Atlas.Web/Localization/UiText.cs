using System.Collections.Frozen;
using Atlas.Domain.Common;

namespace Atlas.Web.Localization;

/// <summary>
/// Translations of fixed interface text on the public site. Keys are the English text,
/// so English needs no entries and a missing translation falls back to English.
/// Editable content (page text, services, FAQ…) is managed per language in the dashboard instead.
/// </summary>
public static class UiText
{
    private static readonly FrozenDictionary<string, string> Arabic = new Dictionary<string, string>
    {
        // Navigation and layout
        ["Home"] = "الرئيسية",
        ["Services"] = "خدماتنا",
        ["Our work"] = "أعمالنا",
        ["About"] = "من نحن",
        ["Contact"] = "تواصل معنا",
        ["Get in touch"] = "تواصل معنا",
        ["Open menu"] = "فتح القائمة",
        ["Main"] = "القائمة الرئيسية",
        ["Footer"] = "تذييل الصفحة",
        ["All rights reserved."] = "جميع الحقوق محفوظة.",
        ["Admin"] = "لوحة التحكم",
        ["Language"] = "اللغة",
        ["home"] = "الصفحة الرئيسية",

        // Home sections
        ["Explore services"] = "استكشف خدماتنا",
        ["What we do"] = "ماذا نقدّم",
        ["View all services"] = "عرض جميع الخدمات",
        ["How we work"] = "طريقة عملنا",
        ["From idea to launch, step by step"] = "من الفكرة إلى الإطلاق، خطوة بخطوة",
        ["Testimonials"] = "آراء العملاء",
        ["What our clients say"] = "ماذا يقول عملاؤنا",
        ["FAQ"] = "الأسئلة الشائعة",
        ["Frequently asked questions"] = "الأسئلة الشائعة",
        ["Trusted by teams across the Kingdom"] = "موثوق من فرق عمل في جميع أنحاء المملكة",
        ["Contact us"] = "تواصل معنا",
        ["A web application and a mobile app"] = "تطبيق ويب وتطبيق جوال",

        // Services page
        ["Our services will be listed here soon."] = "ستُعرض خدماتنا هنا قريبًا.",
        ["Work with us"] = "اعمل معنا",

        // Contact form
        ["Name"] = "الاسم",
        ["E-mail"] = "البريد الإلكتروني",
        ["Subject (optional)"] = "الموضوع (اختياري)",
        ["Message"] = "الرسالة",
        ["Send message"] = "إرسال الرسالة",
        ["Sending…"] = "جارٍ الإرسال…",
        ["Thank you!"] = "شكرًا لك!",
        ["Your message has been sent. We will get back to you soon."] = "تم إرسال رسالتك، وسنتواصل معك قريبًا.",
        ["Send another message"] = "إرسال رسالة أخرى",
        ["Please enter your name."] = "يرجى إدخال اسمك.",
        ["Please enter your e-mail address."] = "يرجى إدخال بريدك الإلكتروني.",
        ["Please enter a valid e-mail address."] = "يرجى إدخال بريد إلكتروني صحيح.",
        ["Please enter a message."] = "يرجى كتابة رسالتك.",
        ["This text is too long."] = "هذا النص طويل جدًا.",
        ["Please wait a few seconds before sending another message."] = "يرجى الانتظار بضع ثوانٍ قبل إرسال رسالة أخرى.",
        ["Something went wrong. Please try again."] = "حدث خطأ ما. يرجى المحاولة مرة أخرى.",
        ["Prefer e-mail?"] = "تفضّل البريد الإلكتروني؟",
        ["Phone (optional)"] = "رقم الجوال (اختياري)",
        ["Service you're interested in"] = "الخدمة التي تهمك",
        ["Budget (optional)"] = "الميزانية التقريبية (اختياري)",
        ["Other"] = "أخرى",
        ["Please enter a valid phone number."] = "يرجى إدخال رقم جوال صحيح.",
        ["Under 50,000 SAR"] = "أقل من 50,000 ريال",
        ["50,000 – 150,000 SAR"] = "50,000 – 150,000 ريال",
        ["150,000 – 500,000 SAR"] = "150,000 – 500,000 ريال",
        ["Over 500,000 SAR"] = "أكثر من 500,000 ريال",
        ["Not sure yet"] = "لم أحدد بعد",
        ["Ask about this service"] = "استفسر عن هذه الخدمة",

        // Portfolio
        ["Projects we're proud of"] = "مشاريع نفخر بها",
        ["Real projects for real businesses, and the results they achieved."] = "مشاريع حقيقية لشركات حقيقية، والنتائج التي حققتها.",
        ["View all work"] = "عرض جميع الأعمال",
        ["View case study"] = "عرض دراسة الحالة",
        ["Case studies are coming soon."] = "ستُعرض دراسات الحالة هنا قريبًا.",
        ["Results"] = "النتائج",
        ["Client"] = "العميل",
        ["The story"] = "القصة",
        ["Start a similar project"] = "ابدأ مشروعًا مشابهًا",
        ["Back to all work"] = "العودة إلى جميع الأعمال",
        ["Have a project in mind?"] = "لديك فكرة مشروع؟",
        ["Tell us about it and get a free estimate."] = "أخبرنا عنها واحصل على تقدير مجاني.",

        // WhatsApp
        ["Chat on WhatsApp"] = "تواصل عبر واتساب",
        ["Prefer WhatsApp?"] = "تفضّل واتساب؟",
        ["Message us on WhatsApp"] = "راسلنا على واتساب",
        ["Hello! I'd like to ask about your services."] = "مرحبًا! أود الاستفسار عن خدماتكم.",
        ["We usually reply within one business day."] = "نرد عادةً خلال يوم عمل واحد.",

        // Errors
        ["Page not found"] = "الصفحة غير موجودة",
        ["Sorry, we couldn't find that page."] = "عذرًا، لم نتمكن من العثور على هذه الصفحة.",
        ["Back to home"] = "العودة إلى الرئيسية",
        ["Something went wrong"] = "حدث خطأ ما",
        ["An error occurred while processing your request. Please try again."] = "حدث خطأ أثناء معالجة طلبك. يرجى المحاولة مرة أخرى.",
    }.ToFrozenDictionary(StringComparer.Ordinal);

    public static string Get(SiteLanguage language, string english) =>
        language == SiteLanguage.Arabic && Arabic.TryGetValue(english, out var arabic) ? arabic : english;

    /// <summary>Keys with an Arabic translation (used by tests).</summary>
    public static IEnumerable<string> TranslatedKeys => Arabic.Keys;
}
