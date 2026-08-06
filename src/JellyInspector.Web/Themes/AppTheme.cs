using MudBlazor;

namespace JellyInspector.Web.Themes;

public static class AppTheme
{
    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Colors.Blue.Darken2,
            Secondary = Colors.Green.Accent4,
            Background = "#f5f5f5",
            Surface = Colors.Shades.White,
            AppbarBackground = Colors.Blue.Darken4,
            DrawerBackground = Colors.Shades.White,
            DrawerText = Colors.Gray.Darken3
        },

        PaletteDark = new PaletteDark
        {
            Primary = Colors.Blue.Lighten1,
            Secondary = Colors.Green.Accent4,
            Background = "#1e1e1e",
            Surface = "#252526",
            AppbarBackground = "#202020",
            DrawerBackground = "#252526"
        },

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px"
        }
    };
}