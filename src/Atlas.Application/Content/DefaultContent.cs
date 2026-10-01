using Atlas.Domain.Common;
using Atlas.Domain.Content;
using Atlas.Domain.Portfolio;

namespace Atlas.Application.Content;

/// <summary>
/// Starter text used before an admin edits a page, and for seeding, in English and Arabic.
/// "{company}" is replaced with the configured company name when a page is rendered.
/// Written for a web and mobile app studio serving businesses in Saudi Arabia; edit it from the dashboard.
/// </summary>
public static class DefaultContent
{
    public const string CompanyToken = "{company}";

    public static PageText For(PageKey key, SiteLanguage language = SiteLanguage.English) =>
        language == SiteLanguage.Arabic ? Arabic(key) : English(key);

    public static IReadOnlyList<(string Title, string Summary, string Icon)> ServicesFor(SiteLanguage language) =>
        language == SiteLanguage.Arabic ? ArabicServices : EnglishServices;

    /// <summary>Starter home page blocks. Samples that would be false claims are seeded unpublished.</summary>
    public static IReadOnlyList<(BlockKind Kind, ContentBlockFields Fields)> BlocksFor(SiteLanguage language) =>
        language == SiteLanguage.Arabic ? ArabicBlocks : EnglishBlocks;

    /// <summary>A hidden sample case study per language, showing admins what a good one looks like.</summary>
    public static CaseStudyFields SampleCaseStudy(SiteLanguage language) => language == SiteLanguage.Arabic
        ? new CaseStudyFields(
            Slug: "sample-project",
            Title: "مثال: تطبيق حجز مواعيد لعيادة",
            Summary: "استبدل هذا المثال بأحد مشاريعك الحقيقية. يبقى مخفيًا حتى تنشره.",
            Client: "اسم العميل",
            Highlights: "زيادة الحجوزات عبر الإنترنت بنسبة 40%\nتطبيق iOS وAndroid بالعربية والإنجليزية\nإطلاق خلال 12 أسبوعًا",
            Body: "التحدي: صف المشكلة التي كان العميل يواجهها.\n\nالحل: اشرح ما بنيتموه وكيف.\n\nالنتيجة: اذكر الأرقام والنتائج التي تحققت.",
            Tags: "جوال, ويب",
            IsPublished: false)
        : new CaseStudyFields(
            Slug: "sample-project",
            Title: "Sample: clinic booking app",
            Summary: "Replace this sample with one of your real projects. It stays hidden until you publish it.",
            Client: "Client name",
            Highlights: "40% more online bookings\niOS and Android apps in Arabic and English\nLaunched in 12 weeks",
            Body: "The challenge: describe the problem the client had.\n\nThe solution: explain what you built and how.\n\nThe result: share the numbers and outcomes.",
            Tags: "Mobile, Web",
            IsPublished: false);

    private static PageText English(PageKey key) => key switch
    {
        PageKey.Home => new PageText(
            Title: "Web and mobile apps that grow your business",
            Subtitle: "We design and build fast, secure web applications and iOS & Android apps for companies across Saudi Arabia, in Arabic and English.",
            Body: "From your first idea to launch day and beyond, one team handles design, development and support, so you can focus on your customers.",
            CallToActionText: "Get a free consultation",
            CallToActionUrl: "/contact",
            MetaDescription: "Web application and mobile app development company in Saudi Arabia. Arabic and English apps, built to grow your business."),
        PageKey.Services => new PageText(
            Title: "Services",
            Subtitle: "Everything you need to launch and grow a digital product, under one roof.",
            CallToActionText: "Discuss your project",
            CallToActionUrl: "/contact",
            MetaDescription: "Web application development, iOS and Android apps, UI/UX design and cloud hosting for Saudi businesses."),
        PageKey.About => new PageText(
            Title: "About us",
            Subtitle: "A team of engineers and designers building digital products for Saudi businesses.",
            Body: "{company} helps companies across the Kingdom turn ideas into reliable web and mobile apps.\n\nWe work closely with our clients, keep communication simple, and build every product in Arabic and English from day one.\n\nOur goal is simple: software that your customers enjoy using and your team can rely on.",
            MetaDescription: "Learn about our team, our values and how we build web and mobile apps."),
        PageKey.Contact => new PageText(
            Title: "Let's talk about your project",
            Subtitle: "Tell us what you want to build. We reply within one business day with next steps and a free estimate.",
            MetaDescription: "Contact us for a free consultation on your web application or mobile app."),
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown page."),
    };

    private static PageText Arabic(PageKey key) => key switch
    {
        PageKey.Home => new PageText(
            Title: "تطبيقات ويب وجوال تنمّي أعمالك",
            Subtitle: "نصمّم ونطوّر تطبيقات ويب وتطبيقات iOS وAndroid سريعة وآمنة للشركات في جميع أنحاء المملكة العربية السعودية، باللغتين العربية والإنجليزية.",
            Body: "من الفكرة الأولى إلى يوم الإطلاق وما بعده، فريق واحد يتولى التصميم والتطوير والدعم، لتتفرغ أنت لعملائك.",
            CallToActionText: "احصل على استشارة مجانية",
            CallToActionUrl: "/ar/contact",
            MetaDescription: "شركة تطوير تطبيقات ويب وتطبيقات جوال في السعودية. تطبيقات بالعربية والإنجليزية مصممة لتنمية أعمالك."),
        PageKey.Services => new PageText(
            Title: "خدماتنا",
            Subtitle: "كل ما تحتاجه لإطلاق منتجك الرقمي وتنميته، تحت سقف واحد.",
            CallToActionText: "ناقش مشروعك معنا",
            CallToActionUrl: "/ar/contact",
            MetaDescription: "تطوير تطبيقات الويب وتطبيقات iOS وAndroid وتصميم واجهات المستخدم والاستضافة السحابية للشركات السعودية."),
        PageKey.About => new PageText(
            Title: "من نحن",
            Subtitle: "فريق من المهندسين والمصممين يبني منتجات رقمية للشركات السعودية.",
            Body: "تساعد {company} الشركات في جميع أنحاء المملكة على تحويل أفكارها إلى تطبيقات ويب وجوال موثوقة.\n\nنعمل جنبًا إلى جنب مع عملائنا، ونحرص على تواصل واضح وبسيط، ونبني كل منتج بالعربية والإنجليزية منذ اليوم الأول.\n\nهدفنا بسيط: برمجيات يستمتع عملاؤك باستخدامها ويعتمد عليها فريقك.",
            MetaDescription: "تعرّف على فريقنا وقيمنا وطريقتنا في بناء تطبيقات الويب والجوال."),
        PageKey.Contact => new PageText(
            Title: "لنتحدث عن مشروعك",
            Subtitle: "أخبرنا بما تريد بناءه، وسنرد عليك خلال يوم عمل واحد بالخطوات التالية وتقدير مجاني للتكلفة.",
            MetaDescription: "تواصل معنا للحصول على استشارة مجانية لتطبيق الويب أو تطبيق الجوال الخاص بك."),
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown page."),
    };

    private static readonly IReadOnlyList<(string Title, string Summary, string Icon)> EnglishServices =
    [
        ("Web applications", "Customer portals, booking systems, dashboards and e-commerce built to be fast, secure and easy to use.", "web"),
        ("Mobile apps", "Native-quality iOS and Android apps with Arabic and English interfaces, published to the App Store and Google Play.", "mobile"),
        ("UI/UX design", "Clear, modern designs tested with real users, so your product is easy to use from the first tap.", "design"),
        ("Cloud & support", "Reliable hosting, monitoring and ongoing maintenance to keep your app running smoothly after launch.", "cloud"),
    ];

    private static readonly IReadOnlyList<(string Title, string Summary, string Icon)> ArabicServices =
    [
        ("تطبيقات الويب", "بوابات العملاء وأنظمة الحجز ولوحات التحكم والمتاجر الإلكترونية، مبنية لتكون سريعة وآمنة وسهلة الاستخدام.", "web"),
        ("تطبيقات الجوال", "تطبيقات iOS وAndroid بجودة عالية وواجهات عربية وإنجليزية، ننشرها على App Store وGoogle Play.", "mobile"),
        ("تصميم واجهات وتجربة المستخدم", "تصاميم عصرية وواضحة نختبرها مع مستخدمين حقيقيين، ليكون منتجك سهل الاستخدام من أول لمسة.", "design"),
        ("الاستضافة والدعم", "استضافة موثوقة ومراقبة مستمرة وصيانة دورية لضمان عمل تطبيقك بسلاسة بعد الإطلاق.", "cloud"),
    ];

    private static readonly IReadOnlyList<(BlockKind Kind, ContentBlockFields Fields)> EnglishBlocks =
    [
        (BlockKind.Stat, new ContentBlockFields("Web + Mobile", Text: "One team for every platform", DisplayOrder: 0)),
        (BlockKind.Stat, new ContentBlockFields("عربي + EN", Text: "Bilingual, right-to-left ready", DisplayOrder: 1)),
        (BlockKind.Stat, new ContentBlockFields("2 weeks", Text: "Between progress demos", DisplayOrder: 2)),
        (BlockKind.Stat, new ContentBlockFields("100%", Text: "Source code ownership for you", DisplayOrder: 3)),

        (BlockKind.ProcessStep, new ContentBlockFields("Discover", Text: "A free call to understand your goals, users and budget. You get a clear scope and estimate.", DisplayOrder: 0)),
        (BlockKind.ProcessStep, new ContentBlockFields("Design", Text: "We design the screens and flows with you and refine them before any code is written.", DisplayOrder: 1)),
        (BlockKind.ProcessStep, new ContentBlockFields("Build", Text: "We develop in short cycles and show you working software every two weeks.", DisplayOrder: 2)),
        (BlockKind.ProcessStep, new ContentBlockFields("Launch & support", Text: "We publish your app, monitor it and keep improving it as your business grows.", DisplayOrder: 3)),

        (BlockKind.Testimonial, new ContentBlockFields("Client name", "Role, Company", "Replace this sample with a real quote from a happy client. It stays hidden until you publish it.", DisplayOrder: 0, IsPublished: false)),
        (BlockKind.ClientLogo, new ContentBlockFields("Client name", Text: "Sample: upload a client logo and publish it.", DisplayOrder: 0, IsPublished: false)),

        (BlockKind.Faq, new ContentBlockFields("How much does an app cost?", Text: "It depends on the features you need. After a short free call we send you a clear fixed-scope estimate, with no obligation.", DisplayOrder: 0)),
        (BlockKind.Faq, new ContentBlockFields("How long does it take?", Text: "A first version of a typical web or mobile app takes a few months. We agree on a timeline with you during the discovery phase and share progress every two weeks.", DisplayOrder: 1)),
        (BlockKind.Faq, new ContentBlockFields("Do you build apps in Arabic?", Text: "Yes. We build every product in Arabic and English, with proper right-to-left layouts.", DisplayOrder: 2)),
        (BlockKind.Faq, new ContentBlockFields("Who owns the source code?", Text: "You do. When the project is complete you receive the full source code and all accounts.", DisplayOrder: 3)),
        (BlockKind.Faq, new ContentBlockFields("Do you support the app after launch?", Text: "Yes. We offer maintenance plans that cover updates, monitoring, bug fixes and new features.", DisplayOrder: 4)),
    ];

    private static readonly IReadOnlyList<(BlockKind Kind, ContentBlockFields Fields)> ArabicBlocks =
    [
        (BlockKind.Stat, new ContentBlockFields("ويب + جوال", Text: "فريق واحد لكل المنصات", DisplayOrder: 0)),
        (BlockKind.Stat, new ContentBlockFields("عربي + EN", Text: "واجهات ثنائية اللغة تدعم الاتجاه من اليمين لليسار", DisplayOrder: 1)),
        (BlockKind.Stat, new ContentBlockFields("أسبوعان", Text: "بين كل عرض تقدّم والآخر", DisplayOrder: 2)),
        (BlockKind.Stat, new ContentBlockFields("100%", Text: "ملكية الكود المصدري لك", DisplayOrder: 3)),

        (BlockKind.ProcessStep, new ContentBlockFields("الاكتشاف", Text: "مكالمة مجانية لفهم أهدافك وجمهورك وميزانيتك، لتحصل على نطاق عمل وتقدير واضحين.", DisplayOrder: 0)),
        (BlockKind.ProcessStep, new ContentBlockFields("التصميم", Text: "نصمم الشاشات وتجربة الاستخدام معك ونحسّنها قبل كتابة أي سطر برمجي.", DisplayOrder: 1)),
        (BlockKind.ProcessStep, new ContentBlockFields("التطوير", Text: "نطوّر على مراحل قصيرة ونعرض عليك نسخة تعمل كل أسبوعين.", DisplayOrder: 2)),
        (BlockKind.ProcessStep, new ContentBlockFields("الإطلاق والدعم", Text: "ننشر تطبيقك ونراقبه ونستمر في تطويره مع نمو أعمالك.", DisplayOrder: 3)),

        (BlockKind.Testimonial, new ContentBlockFields("اسم العميل", "المنصب، الشركة", "استبدل هذا المثال بشهادة حقيقية من أحد عملائك. يبقى مخفيًا حتى تنشره.", DisplayOrder: 0, IsPublished: false)),
        (BlockKind.ClientLogo, new ContentBlockFields("اسم العميل", Text: "مثال: ارفع شعار العميل ثم انشره.", DisplayOrder: 0, IsPublished: false)),

        (BlockKind.Faq, new ContentBlockFields("كم تكلفة تطوير التطبيق؟", Text: "تعتمد التكلفة على المزايا التي تحتاجها. بعد مكالمة قصيرة مجانية نرسل لك تقديرًا واضحًا بنطاق عمل محدد، دون أي التزام.", DisplayOrder: 0)),
        (BlockKind.Faq, new ContentBlockFields("كم يستغرق تطوير التطبيق؟", Text: "تستغرق النسخة الأولى من تطبيق ويب أو جوال نموذجي بضعة أشهر. نتفق معك على جدول زمني في مرحلة الاكتشاف ونشاركك التقدم كل أسبوعين.", DisplayOrder: 1)),
        (BlockKind.Faq, new ContentBlockFields("هل تطورون تطبيقات باللغة العربية؟", Text: "نعم، نبني كل منتج بالعربية والإنجليزية مع دعم كامل للاتجاه من اليمين إلى اليسار.", DisplayOrder: 2)),
        (BlockKind.Faq, new ContentBlockFields("لمن تعود ملكية الكود المصدري؟", Text: "لك أنت. عند اكتمال المشروع تستلم الكود المصدري كاملًا وجميع الحسابات.", DisplayOrder: 3)),
        (BlockKind.Faq, new ContentBlockFields("هل تقدمون الدعم بعد الإطلاق؟", Text: "نعم، نقدم باقات صيانة تشمل التحديثات والمراقبة وإصلاح الأخطاء وإضافة مزايا جديدة.", DisplayOrder: 4)),
    ];
}
