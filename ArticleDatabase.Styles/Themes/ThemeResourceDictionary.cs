using System;
using System.Windows;

namespace Lib.Styles.Themes
{
    public class ThemeResourceDictionary : ResourceDictionary
    {
        private Uri _standardSource;
        private Uri _dimMonitorSource;

        public Uri StandardSource
        {
            get { return _standardSource; }
            set
            {
                _standardSource = value;
            }
        }
        public Uri DimMonitorSource
        {
            get { return _dimMonitorSource; }
            set
            {
                _dimMonitorSource = value;
            }
        }

        public void UpdateSource(string theme)
        {
            Uri themeSource = theme == "Standard" ? StandardSource : DimMonitorSource;
            if (themeSource != null && base.Source != themeSource)
                base.Source = themeSource;
        }
    }
}
