using MainLib.ViewModels.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MainLib.Views.Utils
{
    internal class ThemeService : IThemeService
    {
        public void ChangeTheme(string theme)
        {
            string minimalisticTheme = "/Lib.Styles;component/Colors.xaml";
            string contrastTheme = "/Lib.Styles;component/ColorsContrast.xaml";
            string darkTheme = "/Lib.Styles;component/ColorsDark.xaml";

            var themeDictionary = 
                Application.Current.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source != null &&
                        (d.Source.OriginalString.Contains("Colors.xaml") ||
                        d.Source.OriginalString.Contains("ColorsDark.xaml") ||
                        d.Source.OriginalString.Contains("ColorsContrast.xaml")));

            if (themeDictionary == null) 
            {
                return;
            }

            // Replace the source of the existing theme dictionary
            if (theme == "Minimalistic")
            {
                themeDictionary.Source = new Uri(minimalisticTheme, UriKind.Relative);
            }
            else if (theme == "Contrast")
            {
                themeDictionary.Source = new Uri(contrastTheme, UriKind.Relative);
            }
            else if (theme == "Dark")
            {
                themeDictionary.Source = new Uri(darkTheme, UriKind.Relative);
            }
        }
        public void ChangeFont(string font, FontTarget target)
        {
            string targetName = target == FontTarget.UI ? "UIFont" : "ArticleFont";
            FontFamily customFont = Application.Current.Resources[font] as FontFamily;

            if (customFont != null)
            {
                Application.Current.Resources[targetName] = customFont;
                return;
            }

            Application.Current.Resources[targetName] = new FontFamily(font);
        }
    }
}
