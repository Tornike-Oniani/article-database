using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ViewModels.Services;
using ViewModels.Services.Dialogs;

namespace Views.Services.Dialogs
{
    public class DialogService : IDialogService
    {
        public bool OpenDialog(DialogViewModelBase vm)
        {
            DialogWindow win = new DialogWindow();
            if (Application.Current.MainWindow != null)
                win.Owner = Application.Current.MainWindow;
            else
                win.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            win.DataContext = vm;
            win.ShowDialog();
            return (win.DataContext as DialogViewModelBase).UserDialogResult;
        }
    }
}
