namespace Atlas.Domain.Content;

/// <summary>The editable parts of a <see cref="PageContent"/>.</summary>
public sealed record PageText(
    string Title,
    string? Subtitle = null,
    string? Body = null,
    string? CallToActionText = null,
    string? CallToActionUrl = null,
    string? MetaDescription = null);
