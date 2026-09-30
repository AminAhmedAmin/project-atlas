using MudBlazor;

namespace Atlas.Web.Branding;

public static class ThemeFactory
{
    private static readonly string[] FontStack = ["Inter", "Segoe UI", "Helvetica Neue", "Arial", "sans-serif"];

    public static MudTheme Create(string primaryColor) => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = primaryColor,
            AppbarBackground = "#ffffff",
            AppbarText = "#1f2430",
            Background = "#ffffff",
            BackgroundGray = "#f6f7fb",
            DrawerBackground = "#ffffff",
            TextPrimary = "#1f2430",
            TextSecondary = "#5b6275",
        },
        PaletteDark = new PaletteDark
        {
            Primary = primaryColor,
            AppbarBackground = "#1a1d26",
            Background = "#12141b",
            BackgroundGray = "#181b23",
            Surface = "#1e212b",
            DrawerBackground = "#1a1d26",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = FontStack },
            H1 = new H1Typography { FontFamily = FontStack, FontWeight = "700" },
            H2 = new H2Typography { FontFamily = FontStack, FontWeight = "700" },
            H3 = new H3Typography { FontFamily = FontStack, FontWeight = "700" },
            H4 = new H4Typography { FontFamily = FontStack, FontWeight = "700" },
            H5 = new H5Typography { FontFamily = FontStack, FontWeight = "600" },
            H6 = new H6Typography { FontFamily = FontStack, FontWeight = "600" },
            Button = new ButtonTypography { FontFamily = FontStack, FontWeight = "600", TextTransform = "none" },
        },
        LayoutProperties = new LayoutProperties { DefaultBorderRadius = "10px" },
    };
}
