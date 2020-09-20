using Lib.DataAccessLayer.Models;
using Main.ViewModels;
using Main.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MainLib.ViewModels.Main;
using Lib.Views.Services.Browser;
using Lib.Views.Services.Dialogs;
using Lib.Views.Services.Windows;

namespace Main.Commands
{
    public class UpdateViewCommand : ICommand
    {
        private MainViewModel _mainViewModel;
        private User _user;
        private DialogService _dialogService;
        private WindowService _windowService;
        private BrowserService _browserService;

        public UpdateViewCommand(MainViewModel mainViewModel, User user)
        {
            _mainViewModel = mainViewModel;
            _user = user;
            this._dialogService = new DialogService();
            this._windowService = new WindowService();
            this._browserService = new BrowserService();
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
                    this._mainViewModel.SelectedViewModel = new HomeViewModel(_user, _dialogService, _windowService, _browserService);
                    break;
                case ViewType.DataEntry:
                    this._mainViewModel.SelectedViewModel = new DataEntryViewModel(_user, _browserService);
                    break;
                case ViewType.DataView:
                    this._mainViewModel.SelectedViewModel = new DataViewViewModel(_user, _dialogService, _windowService, _browserService);
                    break;
                case ViewType.Bookmarks:
                    this._mainViewModel.SelectedViewModel = new BookmarksViewModel(_user);
                    break;
                case ViewType.References:
                    this._mainViewModel.SelectedViewModel = new ReferencesViewModel(_user);
                    break;
            }
        }
    }
}
