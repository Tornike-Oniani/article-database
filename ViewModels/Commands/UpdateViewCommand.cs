using Lib.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MainLib.ViewModels.Main;
using MainLib.ViewModels;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;
using Lib.ViewModels.Services.Browser;
using Lib.ViewModels.Base;

namespace MainLib.ViewModels.Commands
{
    public class UpdateViewCommand : ICommand
    {
        private Action<BaseViewModel> _navigate;

        public UpdateViewCommand(Action<BaseViewModel> navigate)
        {
            this._navigate = navigate;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            switch ((ViewType)parameter)
            {
                case ViewType.Home:
                    _navigate(new HomeViewModel());
                    break;
                case ViewType.DataEntry:
                    _navigate(new DataEntryViewModel());
                    break;
                case ViewType.DataView:
                    _navigate(new DataViewViewModel());
                    break;
                case ViewType.Bookmarks:
                    _navigate(new BookmarksViewModel());
                    break;
                case ViewType.References:
                    _navigate(new ReferencesViewModel());
                    break;
            }
        }
    }
}
