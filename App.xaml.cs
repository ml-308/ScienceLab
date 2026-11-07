using System.Windows;
using System.Windows.Media;

namespace ScienceLab;

public partial class App : Application
{
    public static bool IsDark { get; private set; } = true;

    public static void SetTheme(bool dark)
    {
        IsDark = dark;
        var brushes = dark
            ? new (string, string)[]
            {
                ("BgBrush", "#0D0D10"), ("SurfaceBrush", "#141419"), ("SurfaceHoverBrush", "#1C1C23"),
                ("BorderBrush", "#24242C"), ("PrimaryBrush", "#4D6BFE"), ("PrimaryHoverBrush", "#5B77FF"),
                ("PrimarySoftBrush", "#1A2440"),
                ("TextBrush", "#ECECF1"), ("TextSecondaryBrush", "#8E8E9A"), ("TextFaintBrush", "#5C5C66"),
                ("AccentGreenBrush", "#3ECF8E"), ("AccentOrangeBrush", "#F0A13A"),
                ("AccentRedBrush", "#F0645C"), ("AccentPurpleBrush", "#A78BFA"),
            }
            : new (string, string)[]
            {
                ("BgBrush", "#F7F7F9"), ("SurfaceBrush", "#FFFFFF"), ("SurfaceHoverBrush", "#F0F1F5"),
                ("BorderBrush", "#E6E7EC"), ("PrimaryBrush", "#4D6BFE"), ("PrimaryHoverBrush", "#3D5BF0"),
                ("PrimarySoftBrush", "#E9EDFF"),
                ("TextBrush", "#1A1B1F"), ("TextSecondaryBrush", "#6B6F7A"), ("TextFaintBrush", "#9BA0AA"),
                ("AccentGreenBrush", "#1E9E5A"), ("AccentOrangeBrush", "#D98A16"),
                ("AccentRedBrush", "#DC2626"), ("AccentPurpleBrush", "#8B5CF6"),
            };

        foreach (var (key, hex) in brushes)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            if (Current.Resources[key] is SolidColorBrush b)
                b.Color = color;
        }
    }
}
