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
        //private NavigationViewModel _mainViewModel;
        private Action<BaseViewModel> _navigate;
        private Action<bool> _workStatus;
        private User _user;
        private IDialogService _dialogService;
        private IWindowService _windowService;
        private IBrowserService _browserService;

        public UpdateViewCommand(Action<BaseViewModel> navigate, Action<bool> workStatus, User user, IDialogService dialogService, IWindowService windowService, IBrowserService browserService)
        {
            //_mainViewModel = mainViewModel;
            this._navigate = navigate;
            this._workStatus = workStatus;
            this._user = user;
            this._dialogService = dialogService;
            this._windowService = windowService;
            this._browserService = browserService;
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
                    _navigate(new HomeViewModel(_user, _workStatus, _dialogService, _windowService, _browserService));
                    break;
                case ViewType.DataEntry:
                    _navigate(new DataEntryViewModel(_user, _workStatus, _dialogService, _windowService, _browserService));
                    break;
                case ViewType.DataView:
                    _navigate(new DataViewViewModel(_user, _workStatus, _dialogService, _windowService, _browserService));
                    break;
                case ViewType.Bookmarks:
                    _navigate(new BookmarksViewModel(_user, _workStatus));
                    break;
                case ViewType.References:
                    _navigate(new ReferencesViewModel(_user, _workStatus));
                    break;
            }
        }
    }
}
