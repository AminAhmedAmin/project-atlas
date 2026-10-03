using System.Net;
using System.Text;
using System.Text.Json;
using Atlas.Application.Abstractions;
using Atlas.Infrastructure.Notifications;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Tests;

public sealed class TelegramGatewayTests
{
    private const string Token = "123456:SECRET-TOKEN";

    private static (TelegramGateway Gateway, FakeTelegramServer Server) Create(string? token = Token, string? siteUrl = "https://example.com")
    {
        var server = new FakeTelegramServer();
        var gateway = new TelegramGateway(
            new HttpClient(server),
            Options.Create(new TelegramOptions { BotToken = token, SiteUrl = siteUrl, ApiBaseUrl = "https://telegram.test" }));
        return (gateway, server);
    }

    [Fact]
    public async Task SendAlert_posts_html_message_to_the_bot_api()
    {
        var (gateway, server) = Create();

        await gateway.SendAlertAsync(-100123, new TeamAlert("New chat", [new("Name", "Sara")], "Hi", "/admin/chat"), TestContext.Current.CancellationToken);

        var request = Assert.Single(server.Requests);
        Assert.NotNull(request.ContentLength);
        Assert.Equal("https://telegram.test/bot123456:SECRET-TOKEN/sendMessage", request.Uri);
        using var json = JsonDocument.Parse(request.Body!);
        Assert.Equal(-100123, json.RootElement.GetProperty("chat_id").GetInt64());
        Assert.Equal("HTML", json.RootElement.GetProperty("parse_mode").GetString());
        Assert.Contains("<b>Name:</b> Sara", json.RootElement.GetProperty("text").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Format_escapes_visitor_text_and_skips_empty_fields()
    {
        var text = TelegramGateway.Format(
            new TeamAlert("New message", [new("Name", "<script>x</script>"), new("Phone", null), new("Note", " ")], "a & b < c", "/admin/messages"),
            "https://example.com/");

        Assert.Contains("<b>Name:</b> &lt;script&gt;x&lt;/script&gt;", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Phone", text, StringComparison.Ordinal);
        Assert.Contains("a &amp; b &lt; c", text, StringComparison.Ordinal);
        Assert.EndsWith("<a href=\"https://example.com/admin/messages\">Open dashboard</a>", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_truncates_long_messages_and_omits_link_without_site_url()
    {
        var text = TelegramGateway.Format(new TeamAlert("T", [], new string('x', 5000), "/admin"), siteUrl: null);

        Assert.True(text.Length < 4096);
        Assert.EndsWith("…", text, StringComparison.Ordinal);
        Assert.DoesNotContain("href", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetRecentChats_lists_unique_chats_newest_first()
    {
        var (gateway, server) = Create();
        server.Respond(HttpStatusCode.OK, """
            {"ok":true,"result":[
              {"update_id":1,"message":{"chat":{"id":7,"type":"private","first_name":"Amin","username":"amin"}}},
              {"update_id":2,"my_chat_member":{"chat":{"id":-100,"type":"group","title":"Sales team"}}},
              {"update_id":3,"message":{"chat":{"id":7,"type":"private","first_name":"Amin","username":"amin"}}},
              {"update_id":4,"edited_message":{"chat":{"id":9}}}
            ]}
            """);

        var chats = await gateway.GetRecentChatsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["Amin (@amin)", "Sales team"], chats.Select(c => c.Title));
        Assert.Equal([7L, -100L], chats.Select(c => c.Id));
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, """{"ok":false,"description":"Unauthorized"}""", "rejected the bot token")]
    [InlineData(HttpStatusCode.BadRequest, """{"ok":false,"description":"Bad Request: chat not found"}""", "could not find this chat")]
    [InlineData(HttpStatusCode.Forbidden, """{"ok":false,"description":"Forbidden: bot was blocked by the user"}""", "cannot write to this chat")]
    [InlineData(HttpStatusCode.InternalServerError, "not json", "Telegram returned an error")]
    public async Task Errors_become_friendly_messages_without_the_token(HttpStatusCode status, string body, string expected)
    {
        var (gateway, server) = Create();
        server.Respond(status, body);

        var ex = await Assert.ThrowsAsync<AlertDeliveryException>(() =>
            gateway.SendAlertAsync(1, new TeamAlert("T", []), TestContext.Current.CancellationToken));

        Assert.Contains(expected, ex.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("SECRET", ex.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Network_failures_do_not_leak_the_token()
    {
        var (gateway, server) = Create();
        server.ThrowNetworkError = true;

        var ex = await Assert.ThrowsAsync<AlertDeliveryException>(() =>
            gateway.SendAlertAsync(1, new TeamAlert("T", []), TestContext.Current.CancellationToken));

        Assert.DoesNotContain("SECRET", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Without_a_token_nothing_is_sent()
    {
        var (gateway, server) = Create(token: " ");

        Assert.False(gateway.IsConfigured);
        await Assert.ThrowsAsync<AlertDeliveryException>(() => gateway.GetRecentChatsAsync(TestContext.Current.CancellationToken));
        Assert.Empty(server.Requests);
    }

    private sealed class FakeTelegramServer : HttpMessageHandler
    {
        private HttpStatusCode _status = HttpStatusCode.OK;
        private string _body = """{"ok":true,"result":{}}""";

        public List<(string Uri, string? Body, long? ContentLength)> Requests { get; } = [];

        public bool ThrowNetworkError { get; set; }

        public void Respond(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ThrowNetworkError)
            {
                throw new HttpRequestException("Connection refused (telegram.test:443)");
            }

            var length = request.Content?.Headers.ContentLength;
            var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add((request.RequestUri!.ToString(), body, length));
            return new HttpResponseMessage(_status) { Content = new StringContent(_body, Encoding.UTF8, "application/json") };
        }
    }
}
