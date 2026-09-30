using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Application.Contact;
using Atlas.Application.Dashboard;
using Atlas.Application.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Infrastructure.Tests;

public sealed class RepositoryTests
{
    [Fact]
    public async Task Contact_messages_round_trip_search_and_stats()
    {
        await using var host = await SqliteTestHost.CreateAsync();
        var ct = TestContext.Current.CancellationToken;

        Guid janeId;
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var submit = scope.ServiceProvider.GetRequiredService<ICommandHandler<SubmitContactMessageCommand, Guid>>();
            janeId = (await submit.HandleAsync(new SubmitContactMessageCommand("Jane", "jane@example.com", "Pricing", "How much?"), ct)).Value;
            Assert.True((await submit.HandleAsync(new SubmitContactMessageCommand("Bob", "bob@example.org", null, "Hello"), ct)).IsSuccess);
        }

        await using (var scope = host.Services.CreateAsyncScope())
        {
            var markRead = scope.ServiceProvider.GetRequiredService<ICommandHandler<SetContactMessageReadCommand>>();
            Assert.True((await markRead.HandleAsync(new SetContactMessageReadCommand(janeId, true), ct)).IsSuccess);
        }

        await using (var scope = host.Services.CreateAsyncScope())
        {
            var search = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetContactMessagesQuery, PagedResult<ContactMessageDto>>>();

            var byEmail = await search.HandleAsync(new GetContactMessagesQuery("example.org"), ct);
            Assert.Equal("Bob", Assert.Single(byEmail.Items).Name);

            var unread = await search.HandleAsync(new GetContactMessagesQuery(UnreadOnly: true), ct);
            Assert.Equal("Bob", Assert.Single(unread.Items).Name);

            var jane = (await search.HandleAsync(new GetContactMessagesQuery("Pricing"), ct)).Items.Single();
            Assert.True(jane.IsRead);
            Assert.Equal(DateTimeKind.Utc, jane.ReceivedAtUtc.Kind);

            var stats = await scope.ServiceProvider.GetRequiredService<IQueryHandler<GetDashboardStatsQuery, DashboardStatsDto>>()
                .HandleAsync(new GetDashboardStatsQuery(), ct);
            Assert.Equal(2, stats.TotalMessages);
            Assert.Equal(1, stats.UnreadMessages);
            Assert.Equal(2, stats.MessagesPerDay[^1].Count);
        }
    }

    [Fact]
    public async Task Site_settings_value_objects_are_persisted()
    {
        await using var host = await SqliteTestHost.CreateAsync();
        var ct = TestContext.Current.CancellationToken;

        await using (var scope = host.Services.CreateAsyncScope())
        {
            var update = scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdateSiteSettingsCommand, SiteSettingsDto>>();
            Assert.True((await update.HandleAsync(new UpdateSiteSettingsCommand("Acme", "Tag", "#FF0000", "hi@acme.com"), ct)).IsSuccess);
        }

        await using (var scope = host.Services.CreateAsyncScope())
        {
            var settings = await scope.ServiceProvider.GetRequiredService<IQueryHandler<GetSiteSettingsQuery, SiteSettingsDto>>()
                .HandleAsync(new GetSiteSettingsQuery(), ct);

            Assert.Equal("Acme", settings.CompanyName);
            Assert.Equal("#ff0000", settings.PrimaryColor);
            Assert.Equal("hi@acme.com", settings.ContactEmail);
        }
    }
}
