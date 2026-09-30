using Atlas.Application.Dashboard;
using Atlas.Application.Tests.Fakes;
using Atlas.Domain.Common;
using Atlas.Domain.Contact;
using Atlas.Domain.Content;

namespace Atlas.Application.Tests;

public sealed class DashboardHandlerTests
{
    [Fact]
    public async Task Stats_include_kpis_and_a_zero_filled_daily_series()
    {
        var messages = new InMemoryContactMessageRepository();
        var services = new InMemoryServiceRepository();
        var now = TestData.Now.UtcDateTime;

        messages.Add(NewMessage(now));
        messages.Add(NewMessage(now.AddHours(-1)));
        messages.Add(NewMessage(now.AddDays(-3)));
        messages.Add(NewMessage(now.AddDays(-45)));
        messages.Messages[0].MarkAsRead(now);
        services.Add(Service.Create("Web", "s", null, null, 0, true, now));

        var stats = await new GetDashboardStatsHandler(messages, services, TestData.Clock())
            .HandleAsync(new GetDashboardStatsQuery(30), TestContext.Current.CancellationToken);

        Assert.Equal(3, stats.UnreadMessages);
        Assert.Equal(4, stats.TotalMessages);
        Assert.Equal(1, stats.ServicesCount);
        Assert.Equal(30, stats.MessagesPerDay.Count);
        Assert.Equal(DateOnly.FromDateTime(now), stats.MessagesPerDay[^1].Date);
        Assert.Equal(2, stats.MessagesPerDay[^1].Count);
        Assert.Equal(1, stats.MessagesPerDay[^4].Count);
        Assert.Equal(3, stats.MessagesPerDay.Sum(d => d.Count));
    }

    private static ContactMessage NewMessage(DateTime receivedAtUtc) =>
        ContactMessage.Create("Jane", EmailAddress.Create("jane@example.com"), null, "Hi", receivedAtUtc);
}
