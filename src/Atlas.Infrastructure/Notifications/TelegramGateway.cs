using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Atlas.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Notifications;

/// <summary>Minimal Telegram Bot API client: send formatted alerts and list chats that messaged the bot.</summary>
public sealed class TelegramGateway(HttpClient httpClient, IOptions<TelegramOptions> options) : ITelegramGateway
{
    /// <summary>Telegram's limit is 4096 characters per message; keep room for the title and fields.</summary>
    private const int MaxBodyLength = 3000;

    private readonly TelegramOptions _options = options.Value;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.BotToken);

    public async Task<IReadOnlyList<TelegramChat>> GetRecentChatsAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            client => client.GetAsync(Method("getUpdates") + "?limit=100", cancellationToken),
            cancellationToken);

        var updates = await response.Content.ReadFromJsonAsync<TelegramResponse<List<Update>>>(cancellationToken);
        // Updates arrive oldest first; list each chat once, most recently active first.
        return (updates?.Result ?? [])
            .Select(u => u.Message?.Chat ?? u.ChannelPost?.Chat ?? u.MyChatMember?.Chat)
            .OfType<Chat>()
            .Reverse()
            .DistinctBy(c => c.Id)
            .Select(c => new TelegramChat(c.Id, c.DisplayName))
            .ToList();
    }

    public async Task SendAlertAsync(long chatId, TeamAlert alert, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alert);

        var payload = new
        {
            chat_id = chatId,
            text = Format(alert, _options.SiteUrl),
            parse_mode = "HTML",
            link_preview_options = new { is_disabled = true },
        };

        // Buffer the JSON so the request has a Content-Length (no chunked upload), which every proxy accepts.
        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        await SendAsync(client => client.PostAsync(Method("sendMessage"), content, cancellationToken), cancellationToken);
    }

    /// <summary>Formats an alert with Telegram's HTML markup. All user-provided text is escaped.</summary>
    public static string Format(TeamAlert alert, string? siteUrl)
    {
        ArgumentNullException.ThrowIfNull(alert);

        var text = new StringBuilder();
        text.Append("<b>").Append(Escape(alert.Title)).Append("</b>\n");

        foreach (var (label, value) in alert.Fields)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                text.Append("<b>").Append(Escape(label)).Append(":</b> ").Append(Escape(value.Trim())).Append('\n');
            }
        }

        if (!string.IsNullOrWhiteSpace(alert.Body))
        {
            var body = alert.Body.Trim();
            if (body.Length > MaxBodyLength)
            {
                body = body[..MaxBodyLength] + "…";
            }

            text.Append('\n').Append(Escape(body)).Append('\n');
        }

        if (!string.IsNullOrWhiteSpace(siteUrl) && !string.IsNullOrWhiteSpace(alert.DashboardPath)
            && Uri.TryCreate(siteUrl.TrimEnd('/') + alert.DashboardPath, UriKind.Absolute, out var link)
            && link.Scheme is "https" or "http")
        {
            text.Append('\n').Append("<a href=\"").Append(Escape(link.AbsoluteUri)).Append("\">Open dashboard</a>");
        }

        return text.ToString().TrimEnd();
    }

    private static string Escape(string value) =>
        value.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal);

    private Uri Method(string name) => new($"{_options.ApiBaseUrl.TrimEnd('/')}/bot{_options.BotToken}/{name}");

    private async Task<HttpResponseMessage> SendAsync(Func<HttpClient, Task<HttpResponseMessage>> send, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            throw new AlertDeliveryException("Telegram is not set up: the bot token is missing (Telegram__BotToken).");
        }

        HttpResponseMessage response;
        try
        {
            response = await send(httpClient);
        }
        catch (HttpRequestException ex)
        {
            // Never include the request URL: it contains the bot token.
            throw new AlertDeliveryException("Could not reach Telegram. Check the server's internet access.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AlertDeliveryException("Telegram did not respond in time.", ex);
        }

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        string? description = null;
        try
        {
            description = (await response.Content.ReadFromJsonAsync<TelegramResponse<object>>(cancellationToken))?.Description;
        }
        catch (JsonException)
        {
        }

        response.Dispose();
        throw new AlertDeliveryException(response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.NotFound => "Telegram rejected the bot token. Check Telegram__BotToken.",
            HttpStatusCode.Forbidden => "The bot cannot write to this chat (it was blocked or removed). Send /start to the bot again.",
            HttpStatusCode.BadRequest when description?.Contains("chat not found", StringComparison.OrdinalIgnoreCase) == true =>
                "Telegram could not find this chat. Send /start to the bot and try again.",
            HttpStatusCode.Conflict => "This bot uses a webhook, so chats cannot be listed. Remove the webhook or use another bot.",
            _ => $"Telegram returned an error{(description is null ? "." : $": {description}")}",
        });
    }

    private sealed record TelegramResponse<T>(
        [property: JsonPropertyName("ok")] bool Ok,
        [property: JsonPropertyName("result")] T? Result,
        [property: JsonPropertyName("description")] string? Description);

    private sealed record Update(
        [property: JsonPropertyName("message")] MessageEnvelope? Message,
        [property: JsonPropertyName("channel_post")] MessageEnvelope? ChannelPost,
        [property: JsonPropertyName("my_chat_member")] MessageEnvelope? MyChatMember);

    private sealed record MessageEnvelope([property: JsonPropertyName("chat")] Chat? Chat);

    private sealed record Chat(
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("first_name")] string? FirstName,
        [property: JsonPropertyName("last_name")] string? LastName,
        [property: JsonPropertyName("username")] string? Username)
    {
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Title))
                {
                    return Title;
                }

                var name = string.Join(' ', new[] { FirstName, LastName }.Where(n => !string.IsNullOrWhiteSpace(n)));
                if (name.Length > 0)
                {
                    return Username is null ? name : $"{name} (@{Username})";
                }

                return Username is null ? Id.ToString(System.Globalization.CultureInfo.InvariantCulture) : "@" + Username;
            }
        }
    }
}
