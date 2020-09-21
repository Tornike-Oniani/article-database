using Lib.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;
using Lib.ViewModels.Services.Browser;
using SectionLib.ViewModels.Main;

namespace SectionLib.ViewModels.Commands
{
    public class UpdateViewCommand : ICommand
    {
        private NavigationViewModel _navigationViewModel;
        private User _user;
        private IDialogService _dialogService;
        private IWindowService _windowService;
        private IBrowserService _browserService;

        public UpdateViewCommand(NavigationViewModel navigationViewModel, User user, IDialogService dialogService, IWindowService windowService, IBrowserService browserService)
        {
            _navigationViewModel = navigationViewModel;
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
                    this._navigationViewModel.SelectedViewModel = new HomeViewModel(_navigationViewModel, _dialogService, _browserService);
                    break;
                case ViewType.DataEntry:
                    this._navigationViewModel.SelectedViewModel = new DataEntryViewModel(_user, _browserService);
                    break;
                case ViewType.DataView:
                    this._navigationViewModel.SelectedViewModel = new DataViewViewModel(_user, _dialogService, _windowService, _browserService);
                    break;
            }
        }
    }
}
