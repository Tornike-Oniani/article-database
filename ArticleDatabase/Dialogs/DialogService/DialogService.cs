using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleDatabase.Dialogs.DialogService
{
    public static class DialogService
    {
        public static bool OpenDialog(DialogViewModelBase vm, Window owner)
        {
            DialogWindow win = new DialogWindow();
            if (owner != null)
                win.Owner = owner;
            win.DataContext = vm;
            win.ShowDialog();
            return (win.DataContext as DialogViewModelBase).UserDialogResult;
        }
    }
}
