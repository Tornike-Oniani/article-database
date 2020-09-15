using ArticleDatabase.ViewModels;
using ArticleDatabase.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleDatabase.Windows.WindowService
{
    public static class WindowService
    {
        public static void OpenWindow(BaseViewModel vm, WindowType type = WindowType.Generic, bool dialog = true)
        {
            Window win = GetWindow(type);
            win.DataContext = vm;
            if (MainWindow.CurrentMain != null)
                win.Owner = MainWindow.CurrentMain;
            if (dialog)
                win.ShowDialog();
            else
                win.Show();

        }

        private static Window GetWindow(WindowType type)
        {
            switch (type)
            {
                case WindowType.MainWindow:
                    return new MainWindow();
                case WindowType.Generic:
                    return new GenericWindow();
                default:
                    throw new ArgumentException("Wrong window type");
            }
        }
    }
}
