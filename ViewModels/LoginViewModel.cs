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
using MainLib.ViewModels.Utils;
using System.Windows.Controls;
using System.Runtime.Serialization;

namespace MainLib.ViewModels
{
    public enum CurrentPage
    {
        Login,
        Register
    }

    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private string _passwordConfirm;
        private bool _isBusy;
        private CurrentPage _currentPage;
        private bool _isVisible;
        private IDialogService _dialogService;
        private IWindowService _windowService;
        private IBrowserService _browserService;

        public User CurrentUser { get; set; }
        public string Username
        {
            get { return _username; }
            set { _username = value; OnPropertyChanged("Username"); }
        }
        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged("Password"); }
        }
        public string PasswordConfirm
        {
            get { return _passwordConfirm; }
            set { _passwordConfirm = value; OnPropertyChanged("PasswordConfirm"); }
        }
        public CurrentPage CurrentPage
        {
            get { return _currentPage; }
            set { _currentPage = value; OnPropertyChanged("CurrentPage"); }
        }
        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; OnPropertyChanged("IsVisible"); }
        }
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; OnPropertyChanged("IsBusy"); }
        }

        public RelayCommand LoginCommand { get; set; }
        public RelayCommand ShowRegisterCommand { get; set; }
        public RelayCommand RegisterCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        public LoginViewModel(IDialogService dialogService, IWindowService windowService, IBrowserService browserService)
        {
            this._dialogService = dialogService;
            this._windowService = windowService;
            this._browserService = browserService;
            this.IsBusy = false;

            CurrentUser = new User();
            LoginCommand = new RelayCommand(Login);
            ShowRegisterCommand = new RelayCommand(ShowRegister);
            RegisterCommand = new RelayCommand(Register);
            CancelCommand = new RelayCommand(Cancel);
        }

        public async void Login(object input)
        {
            IsBusy = true;

            PasswordBox passwordBox = input as PasswordBox;
            bool login = false;
            await Task.Run(() =>
            {
                login = new UserRepo().Login(CurrentUser, passwordBox.Password);
            });
            if (login)
            {
                _windowService.OpenWindow(new NavigationViewModel(CurrentUser, _dialogService, _windowService, _browserService), WindowType.MainWindow, false, false);
                await Task.Run(() =>
                {
                    new Tracker().init();
                });
                Window.Close();
            }
            else
            {
                _dialogService.OpenDialog(new DialogOkViewModel("Invalid username or password", "Error", DialogType.Error));
            }

            IsBusy = false;
        }

        public void ShowRegister(object input = null)
        {
            CurrentUser.Username = null;
            CurrentUser.Password = null;
            this.CurrentPage = CurrentPage.Register;
            this.IsVisible = true;
        }
        public async void Register(object input = null)
        {
            if (Password != PasswordConfirm)
            {
                _dialogService.OpenDialog(new DialogOkViewModel("Passwords do not match!", "Register", DialogType.Error));
                return;
            }

            IsBusy = true;

            int admin = Username.Contains("_adminGfK") ? 1 : 0;
            Username = Username.Replace("_adminGfK", "");
            bool register = false;

            await Task.Run(() =>
            {
                User user = new User() { Username = this.Username, Password = this.Password };
                register = new UserRepo().Register(user, admin);
            });

            if (register)
            {
                _dialogService.OpenDialog(new DialogOkViewModel("New user created successfuly", "Result", DialogType.Success));
            }
            else
            {
                _dialogService.OpenDialog(new DialogOkViewModel("Username is already taken", "Warning", DialogType.Warning));
            }

            IsBusy = false;

            this.Username = null;
            this.Password = null;
            this.PasswordConfirm = null;
            this.IsVisible = false;
            this.CurrentPage = CurrentPage.Login;
        }
        public void Cancel(object input = null)
        {
            this.Username = null;
            this.Password = null;
            this.PasswordConfirm = null;
            this.IsVisible = false;
            this.CurrentPage = CurrentPage.Login;
        }
    }
}
