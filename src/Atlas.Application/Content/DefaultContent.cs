using Atlas.Domain.Content;

namespace Atlas.Application.Content;

/// <summary>
/// Starter text used before an admin edits a page, and for seeding.
/// "{company}" is replaced with the configured company name when a page is rendered.
/// Written for a web and mobile app studio serving businesses in Saudi Arabia; edit it from the dashboard.
/// </summary>
public static class DefaultContent
{
    public const string CompanyToken = "{company}";

    public static PageText For(PageKey key) => key switch
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

    public static IReadOnlyList<(string Title, string Summary, string Icon)> Services { get; } =
    [
        ("Web applications", "Customer portals, booking systems, dashboards and e-commerce built to be fast, secure and easy to use.", "web"),
        ("Mobile apps", "Native-quality iOS and Android apps with Arabic and English interfaces, published to the App Store and Google Play.", "mobile"),
        ("UI/UX design", "Clear, modern designs tested with real users, so your product is easy to use from the first tap.", "design"),
        ("Cloud & support", "Reliable hosting, monitoring and ongoing maintenance to keep your app running smoothly after launch.", "cloud"),
    ];

    /// <summary>Starter home page blocks. Samples that would be false claims are seeded unpublished.</summary>
    public static IReadOnlyList<(BlockKind Kind, ContentBlockFields Fields)> Blocks { get; } =
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
}
