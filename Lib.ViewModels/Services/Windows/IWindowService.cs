using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ViewModels.Base;

namespace Lib.ViewModels.Services.Windows
{
    public interface IWindowService
    {
        void OpenWindow(BaseViewModel vm, 
                        WindowType type = WindowType.Generic, 
                        bool mainExists = true, 
                        bool dialog = true, 
                        bool passWindow = false);
    }
}
