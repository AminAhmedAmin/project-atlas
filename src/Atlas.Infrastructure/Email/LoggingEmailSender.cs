using Atlas.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Atlas.Infrastructure.Email;

/// <summary>Placeholder sender that only logs. Replace with an SMTP or API-based sender for production.</summary>
public sealed partial class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        LogEmail(logger, message.To, message.Subject);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "E-mail to {To} with subject '{Subject}' (not sent: no e-mail provider configured)")]
    private static partial void LogEmail(ILogger logger, string to, string subject);
}
