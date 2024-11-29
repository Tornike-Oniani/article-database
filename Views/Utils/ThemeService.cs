using MainLib.ViewModels.Utils;
using System.Windows;
using System.Windows.Media;

namespace MainLib.Views.Utils
{
    internal class ThemeService : IThemeService
    {
        public void ChangeTheme(string theme)
        {
            string themeSuffix = theme == "Dark" ? "_dark" : "";

            Application.Current.Resources["shadcn_background_brush"] = new SolidColorBrush(getColor($"shadcn_background{themeSuffix}"));
            Application.Current.Resources["shadcn_foreground_brush"] = new SolidColorBrush(getColor($"shadcn_foreground{themeSuffix}"));
            Application.Current.Resources["shadcn_muted_brush"] = new SolidColorBrush(getColor($"shadcn_muted{themeSuffix}"));
            Application.Current.Resources["shadcn_muted_foreground_brush"] = new SolidColorBrush(getColor($"shadcn_muted_foreground{themeSuffix}"));
            Application.Current.Resources["shadcn_popover_brush"] = new SolidColorBrush(getColor($"shadcn_popover{themeSuffix}"));
            Application.Current.Resources["shadcn_primary_brush"] = new SolidColorBrush(getColor($"shadcn_primary{themeSuffix}"));
            Application.Current.Resources["shadcn_primary_brush_hover"] = new SolidColorBrush(getColor($"shadcn_primary{themeSuffix}")) { Opacity=0.9 };
            Application.Current.Resources["shadcn_primary_foreground_brush"] = new SolidColorBrush(getColor($"shadcn_primary_foreground{themeSuffix}"));
            Application.Current.Resources["shadcn_secondary_brush"] = new SolidColorBrush(getColor($"shadcn_secondary{themeSuffix}"));
            Application.Current.Resources["shadcn_secondary_brush_hover"] = new SolidColorBrush(getColor($"shadcn_secondary{themeSuffix}")) { Opacity = 0.9 };
            Application.Current.Resources["shadcn_secondary_foreground_brush"] = new SolidColorBrush(getColor($"shadcn_secondary_foreground{themeSuffix}"));
            Application.Current.Resources["shadcn_accent_brush"] = new SolidColorBrush(getColor($"shadcn_accent{themeSuffix}"));
            Application.Current.Resources["shadcn_accent_foreground_brush"] = new SolidColorBrush(getColor($"shadcn_accent_foreground{themeSuffix}"));
            Application.Current.Resources["shadcn_destructive_brush"] = new SolidColorBrush(getColor($"shadcn_destructive{themeSuffix}"));
            Application.Current.Resources["shadcn_destructive_foreground_brush"] = new SolidColorBrush(getColor($"shadcn_destructive_foreground{themeSuffix}"));
            Application.Current.Resources["shadcn_border_brush"] = new SolidColorBrush(getColor($"shadcn_border{themeSuffix}"));
            Application.Current.Resources["shadcn_ring_brush"] = new SolidColorBrush(getColor($"shadcn_ring{themeSuffix}"));

            //GrayColorLight
            //Color color = getColor($"GrayColorLight");
            //string themeSuffix = theme == "Dim Monitor" ? "BM" : "";

            //// Gray color
            //Application.Current.Resources["GrayColorBrush"] = new SolidColorBrush(getColor($"GrayColor{themeSuffix}"));
            //Application.Current.Resources["GrayColorLightBrush"] = new SolidColorBrush(getColor($"GrayColorLight{themeSuffix}"));

            //// Blue color
            //Application.Current.Resources["BlueColorBrush"] = new SolidColorBrush(getColor($"BlueColor{themeSuffix}"));
            //Application.Current.Resources["BlueColorLightBrush"] = new SolidColorBrush(getColor($"BlueColorLight{themeSuffix}"));
        }

        private Color getColor(string name)
        {
            return (Color)Application.Current.Resources[name];
        }
    }
}
