using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lib.Styles
{
    public class SkinResourceDictionary : ResourceDictionary
    {
        private Uri _standardSource;
        private Uri _dimMonitorSource;

        public Uri StandardSource
        {
            get { return _standardSource; }
            set { _standardSource = value; UpdateSource("Standard"); }
        }
        public Uri DimMonitorSource
        {
            get { return _dimMonitorSource; }
            set { _dimMonitorSource = value; UpdateSource("Dim Monitor"); }
        }

        private void UpdateSource(string themeName)
        {
            Uri theme = themeName == "Standard" ? StandardSource : DimMonitorSource;
            if (theme != null && base.Source != theme)
                base.Source = theme;
        }
    }
}
