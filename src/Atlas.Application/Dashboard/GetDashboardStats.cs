using Atlas.Application.Abstractions;
using Atlas.Application.Common;

namespace Atlas.Application.Dashboard;

public sealed record GetDashboardStatsQuery(int Days = 30);

public sealed record DailyCount(DateOnly Date, int Count);

public sealed record DashboardStatsDto(
    int UnreadMessages,
    int TotalMessages,
    int ServicesCount,
    IReadOnlyList<DailyCount> MessagesPerDay);

public sealed class GetDashboardStatsHandler(
    IContactMessageRepository messages,
    IServiceRepository services,
    TimeProvider timeProvider) : IQueryHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    public async Task<DashboardStatsDto> HandleAsync(GetDashboardStatsQuery query, CancellationToken cancellationToken = default)
    {
        var days = Math.Clamp(query.Days, 1, 366);
        var today = DateOnly.FromDateTime(timeProvider.UtcNow());
        var firstDay = today.AddDays(-(days - 1));
        var fromUtc = firstDay.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        // Sequential awaits: a scoped DbContext does not support concurrent operations.
        var unread = await messages.CountAsync(unreadOnly: true, cancellationToken);
        var total = await messages.CountAsync(unreadOnly: false, cancellationToken);
        var servicesCount = await services.CountAsync(cancellationToken);
        var received = await messages.GetReceivedTimesSinceAsync(fromUtc, cancellationToken);

        var countsByDay = received
            .GroupBy(DateOnly.FromDateTime)
            .ToDictionary(g => g.Key, g => g.Count());

        var series = Enumerable.Range(0, days)
            .Select(offset => firstDay.AddDays(offset))
            .Select(day => new DailyCount(day, countsByDay.GetValueOrDefault(day)))
            .ToList();

        return new DashboardStatsDto(unread, total, servicesCount, series);
    }
}
