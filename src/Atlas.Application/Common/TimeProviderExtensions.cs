namespace Atlas.Application.Common;

internal static class TimeProviderExtensions
{
    public static DateTime UtcNow(this TimeProvider timeProvider) => timeProvider.GetUtcNow().UtcDateTime;
}
