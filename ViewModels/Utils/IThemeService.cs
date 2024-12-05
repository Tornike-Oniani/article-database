using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Utils
{
    public enum FontTarget {
        UI,
        Article
    }

    public interface IThemeService
    {
        void ChangeTheme(string theme);
        void ChangeFont(string font, FontTarget target);
    }
}
