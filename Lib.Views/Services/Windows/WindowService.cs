using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Lib.ViewModels.Base;
using Lib.ViewModels.Services;
using Lib.ViewModels.Services.Windows;

namespace Lib.Views.Services.Windows
{
    public class WindowService : IWindowService
    {
        public void OpenWindow(BaseViewModel vm, 
                               WindowType type = WindowType.Generic, 
                               bool mainExists = true, 
                               bool dialog = true,
                               bool passWindow = false)
        {
            Window win = GetWindow(type);
            win.DataContext = vm;
            if (mainExists && Application.Current.MainWindow != null)
                win.Owner = Application.Current.MainWindow;
            if (passWindow)
                vm.Window = (IWindow)win;
            if (dialog)
                win.ShowDialog();
            else
                win.Show();

        }

        private Window GetWindow(WindowType type)
        {
            switch (type)
            {
                case WindowType.MainWindow:
                    return new MainWindow();
                case WindowType.Generic:
                    return new GenericWindow();
                case WindowType.Medium:
                    return new MediumWindow();
                default:
                    throw new ArgumentException("Wrong window type");
            }
        }
    }
}
