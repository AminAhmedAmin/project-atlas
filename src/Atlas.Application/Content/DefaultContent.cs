using Atlas.Domain.Content;

namespace Atlas.Application.Content;

/// <summary>
/// Starter text used before an admin edits a page, and for seeding.
/// "{company}" is replaced with the configured company name when a page is rendered.
/// </summary>
public static class DefaultContent
{
    public const string CompanyToken = "{company}";

    public static PageText For(PageKey key) => key switch
    {
        PageKey.Home => new PageText(
            Title: "Software that moves your business forward",
            Subtitle: "We design, build and run reliable web and cloud applications, from first idea to production.",
            Body: "Whether you are launching a new product or modernising an existing system, our team helps you ship faster with confidence.",
            CallToActionText: "Start a project",
            CallToActionUrl: "/contact",
            MetaDescription: "Custom software development, cloud solutions and consulting."),
        PageKey.Services => new PageText(
            Title: "Services",
            Subtitle: "End-to-end expertise for every stage of your product.",
            MetaDescription: "Explore our software development, cloud and consulting services."),
        PageKey.About => new PageText(
            Title: "About us",
            Subtitle: "A small team of engineers and designers who care about quality.",
            Body: "{company} was founded to help organisations turn ideas into dependable software.\n\nWe work in close partnership with our clients, favour simple solutions, and measure success by the outcomes we deliver.",
            MetaDescription: "Learn about our team, our values and how we work."),
        PageKey.Contact => new PageText(
            Title: "Contact us",
            Subtitle: "Tell us about your project and we will get back to you within one business day.",
            MetaDescription: "Get in touch to discuss your next software project."),
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown page."),
    };

    public static IReadOnlyList<(string Title, string Summary, string Icon)> Services { get; } =
    [
        ("Web applications", "Fast, accessible web apps built with modern frameworks and clean architecture.", "web"),
        ("Cloud & DevOps", "Scalable cloud infrastructure, CI/CD pipelines and observability on Azure.", "cloud"),
        ("Consulting", "Architecture reviews, technical due diligence and team coaching.", "lightbulb"),
    ];
}
