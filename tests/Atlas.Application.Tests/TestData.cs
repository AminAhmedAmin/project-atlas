using Microsoft.Extensions.Time.Testing;

namespace Atlas.Application.Tests;

internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);

    public static FakeTimeProvider Clock() => new(Now);
}
