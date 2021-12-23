using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Lib.ViewModels.Base;
using MainLib.ViewModels.Main;
using MainLib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;
using Lib.ViewModels.Services.Browser;
using MainLib.ViewModels.Utils;
using MainLib.ViewModels.Popups;
using Lib.ViewModels.Commands;

namespace MainLib.ViewModels
{
    public class NavigationViewModel : BaseViewModel
    {
        private BaseViewModel _selectedViewModel;
        private User _user;
        private bool _isBusy;
        private IWindowService _windowService;

        public BaseViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set { _selectedViewModel = value; OnPropertyChanged("SelectedViewModel"); }
        }
        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; OnPropertyChanged("IsBusy"); }
        }


        public ICommand UpdateViewCommand { get; set; }
        public ICommand OpenSettingsCommand { get; set; }

        public NavigationViewModel(User user, IDialogService dialogService, IWindowService windowService, IBrowserService browserService)
        {
            _windowService = windowService;
            UpdateViewCommand = new UpdateViewCommand(Navigate, WorkStatus, user, dialogService, windowService, browserService);
            UpdateViewCommand.Execute(ViewType.Home);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            this.Title = user.Username;
            // Set admin/user status
            user.Admin = new UserRepo().IsAdmin(user);
            this.User = user;
        }

        public void Navigate(BaseViewModel viewModel)
        {
            this.SelectedViewModel = viewModel;
        }
        public void OpenSettings(object input = null)
        {
            _windowService.OpenWindow(new SettingsViewModel(User), passWindow: true);
        }

        public void WorkStatus(bool isWorking)
        {
            this.IsBusy = isWorking;
        }
    }
}
