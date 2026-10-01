using MudBlazor;

namespace Atlas.Web.Branding;

/// <summary>Icon keys that can be assigned to services from the dashboard.</summary>
public static class ServiceIcons
{
    public static IReadOnlyDictionary<string, string> All { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["web"] = Icons.Material.Outlined.Language,
        ["cloud"] = Icons.Material.Outlined.Cloud,
        ["code"] = Icons.Material.Outlined.Code,
        ["mobile"] = Icons.Material.Outlined.PhoneIphone,
        ["lightbulb"] = Icons.Material.Outlined.Lightbulb,
        ["security"] = Icons.Material.Outlined.Security,
        ["analytics"] = Icons.Material.Outlined.Insights,
        ["design"] = Icons.Material.Outlined.Brush,
        ["support"] = Icons.Material.Outlined.SupportAgent,
        ["data"] = Icons.Material.Outlined.Storage,
        ["ai"] = Icons.Material.Outlined.AutoAwesome,
        ["rocket"] = Icons.Material.Outlined.RocketLaunch,
    };

    public static string For(string? key) =>
        key is not null && All.TryGetValue(key, out var icon) ? icon : Icons.Material.Outlined.Star;
}
