using MainLib.ViewModels.Utils;
using System.Windows;
using System.Windows.Media;

namespace MainLib.Views.Utils
{
    internal class ThemeService : IThemeService
    {
        public void ChangeTheme(string theme)
        {
            //GrayColorLight
            //Color color = getColor($"GrayColorLight");
            string themeSuffix = theme == "Dim Monitor" ? "BM" : "";

            // Gray color
            Application.Current.Resources["GrayColorBrush"] = new SolidColorBrush(getColor($"GrayColor{themeSuffix}"));
            Application.Current.Resources["GrayColorLightBrush"] = new SolidColorBrush(getColor($"GrayColorLight{themeSuffix}"));

            // Blue color
            Application.Current.Resources["BlueColorBrush"] = new SolidColorBrush(getColor($"BlueColor{themeSuffix}"));
            Application.Current.Resources["BlueColorLightBrush"] = new SolidColorBrush(getColor($"BlueColorLight{themeSuffix}"));
        }

        private Color getColor(string name)
        {
            return (Color)Application.Current.Resources[name];
        }
    }
}
