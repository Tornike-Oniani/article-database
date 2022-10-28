using MainLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            string themeSuffix = theme == "Bad Monitor" ? "BM" : "";

            // Gray color
            ((SolidColorBrush)Application.Current.Resources["GrayColorBrush"]).Color = getColor($"GrayColor{themeSuffix}");
            ((SolidColorBrush)Application.Current.Resources["GrayColorLightBrush"]).Color = getColor($"GrayColorLight{themeSuffix}");

            // Blue color
            ((SolidColorBrush)Application.Current.Resources["BlueColorLightBrush"]).Color = getColor($"BlueColorLight{themeSuffix}");
        }

        private Color getColor(string name)
        {
            return (Color)Application.Current.Resources[name];
        }
    }
}
