using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Base;

namespace ViewModels.Services.Windows
{
    public interface IWindowService
    {
        void OpenWindow(BaseViewModel vm, WindowType type = WindowType.Generic, bool dialog = true);
    }
}
