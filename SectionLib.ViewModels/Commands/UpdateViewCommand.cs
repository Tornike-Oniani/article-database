using Lib.DataAccessLayer.Models;
using Lib.ViewModels.Services.Browser;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;
using SectionLib.ViewModels.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SectionLib.ViewModels.Commands
{
    public class UpdateViewCommand : ICommand
    {
        private NavigationViewModel _mainViewModel;
        private User _user;
        private IDialogService _dialogService;
        private IWindowService _windowService;
        private IBrowserService _browserService;

        public UpdateViewCommand(NavigationViewModel mainViewModel, User user, IDialogService dialogService, IWindowService windowService, IBrowserService browserService)
        {
            _mainViewModel = mainViewModel;
            _user = user;
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
                    this._mainViewModel.SelectedViewModel = new HomeViewModel(_mainViewModel, _dialogService, _windowService, _browserService);
                    break;
                case ViewType.DataEntry:
                    this._mainViewModel.SelectedViewModel = new DataEntryViewModel(_user, _browserService);
                    break;
                case ViewType.DataView:
                    this._mainViewModel.SelectedViewModel = new DataViewViewModel(_user, _dialogService, _windowService, _browserService);
                    break;
            }
        }
    }
}
