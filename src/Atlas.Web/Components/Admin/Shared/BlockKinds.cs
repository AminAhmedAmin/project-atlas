using Atlas.Domain.Content;

namespace Atlas.Web.Components.Admin.Shared;

/// <summary>Dashboard labels and help text for each kind of home page block.</summary>
public sealed record BlockKindInfo(
    BlockKind Kind,
    string Tab,
    string Singular,
    string Description,
    string TitleLabel,
    string? SubtitleLabel,
    string TextLabel,
    string? TitleHelp,
    bool UsesImage);

public static class BlockKinds
{
    public static IReadOnlyList<BlockKindInfo> All { get; } =
    [
        new(BlockKind.Stat, "Numbers", "number",
            "Short facts shown in a band under the hero, e.g. \"50+\" projects delivered. Use real numbers.",
            "Number", null, "Label", "Keep it short: 50+, 98%, 24/7, 2 weeks…", UsesImage: false),
        new(BlockKind.ClientLogo, "Client logos", "client",
            "Clients or partners shown as \"Trusted by…\". Upload a logo, or leave it empty to show the name as text.",
            "Client name", null, "Notes (not shown)", "Also used as the logo's alt text.", UsesImage: true),
        new(BlockKind.ProcessStep, "How we work", "step",
            "The steps of your process, shown in order. The Home page body text is used as the introduction.",
            "Step name", null, "Description", null, UsesImage: false),
        new(BlockKind.Testimonial, "Testimonials", "testimonial",
            "Quotes from real clients. Ask for permission before publishing a name.",
            "Person's name", "Role and company", "Quote", null, UsesImage: false),
        new(BlockKind.Faq, "FAQ", "question",
            "Questions customers often ask. They are also published as structured data for Google.",
            "Question", null, "Answer", null, UsesImage: false),
    ];

    public static BlockKindInfo For(BlockKind kind) => All.First(k => k.Kind == kind);
}
