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
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Windows;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Browser;
using MainLib.ViewModels;

namespace MainLib.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private IDialogService _dialogService;
        private IWindowService _windowService;
        private IBrowserService _browserService;

        public User CurrentUser { get; set; }

        public RelayCommand LoginCommand { get; set; }
        public RelayCommand RegisterCommand { get; set; }

        public LoginViewModel(IDialogService dialogService, IWindowService windowService, IBrowserService browserService)
        {
            this._dialogService = dialogService;
            this._windowService = windowService;
            this._browserService = browserService;

            CurrentUser = new User();
            LoginCommand = new RelayCommand(Login);
            RegisterCommand = new RelayCommand(Register);
        }

        public void Login(object input)
        {
            if ((new UserRepo()).Login(CurrentUser))
            {
                _windowService.OpenWindow(new NavigationViewModel(CurrentUser, _dialogService, _windowService, _browserService), WindowType.MainWindow, false, false);
                (input as ICommand).Execute(null);
            }
            else
            {
                _dialogService.OpenDialog(new DialogOkViewModel("Invalid username or password", "Error", DialogType.Error));
            }
        }

        public void Register(object Input = null)
        {
            int admin = CurrentUser.Username.Contains("_adminGfK") ? 1 : 0;
            CurrentUser.Username = CurrentUser.Username.Replace("_adminGfK", "");
            if ((new UserRepo()).Register(CurrentUser, admin))
            {
                _dialogService.OpenDialog(new DialogOkViewModel("New user created successfuly", "Result", DialogType.Success));
            }
            else
            {
                _dialogService.OpenDialog(new DialogOkViewModel("Username is already taken", "Warning", DialogType.Warning));
            }
        }
    }
}
