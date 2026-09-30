namespace Atlas.Application.Abstractions;

/// <summary>Stores publicly served files (e.g. the logo) and returns their site-relative URL.</summary>
public interface IFileStorage
{
    Task<string> SaveAsync(string folder, string fileName, Stream content, CancellationToken cancellationToken = default);

    Task DeleteAsync(string url, CancellationToken cancellationToken = default);
}

public sealed record EmailMessage(string To, string Subject, string Body, string? ReplyTo = null);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
