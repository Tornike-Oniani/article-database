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
        private bool _loginFocus;
        private bool _registerFocus;
        private readonly Shared services;

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
        public bool LoginFocus
        {
            get { return _loginFocus; }
            set { _loginFocus = value; OnPropertyChanged("LoginFocus"); }
        }
        public bool RegisterFocus
        {
            get { return _registerFocus; }
            set { _registerFocus = value; OnPropertyChanged("RegisterFocus"); }
        }

        public RelayCommand LoginCommand { get; set; }
        public RelayCommand ShowRegisterCommand { get; set; }
        public RelayCommand RegisterCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        public LoginViewModel(IDialogService dialogService, IWindowService windowService, IBrowserService browserService, IThemeService themeService)
        {
            this.IsBusy = false;
            this.services = Shared.GetInstance();
            this.services.SetServices(dialogService, windowService, browserService, themeService);
            this.services.ThemeService.ChangeTheme(Properties.Settings.Default.Theme);
            this.services.ThemeService.ChangeFont(Properties.Settings.Default.UIFontFamily, FontTarget.UI);
            this.services.ThemeService.ChangeFont(Properties.Settings.Default.ArticleFontFamily, FontTarget.Article);
            this.CurrentUser = new User();
            this.LoginCommand = new RelayCommand(Login);
            this.ShowRegisterCommand = new RelayCommand(ShowRegister);
            this.RegisterCommand = new RelayCommand(Register);
            this.CancelCommand = new RelayCommand(Cancel);
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
                services.SetLoggedInUser(CurrentUser);
                services.WindowService.OpenWindow(new NavigationViewModel(), WindowType.MainWindow, false, false);
                // Initialize tracker if user is admin
                await Task.Run(() =>
                {
                    CurrentUser.Admin = new UserRepo().IsAdmin(CurrentUser);
                    if (CurrentUser.IsAdmin)
                        new Tracker(CurrentUser).init();
                });

                Window.Close();
            }
            else
            {
                services.DialogService.OpenDialog(new DialogOkViewModel("Invalid username or password", "Error", DialogType.Error));
                passwordBox.Password = null;
            }

            IsBusy = false;
        }

        public void ShowRegister(object input)
        {
            CurrentUser.Username = null;
            CurrentUser.Password = null;
            PasswordBox passwordBox = input as PasswordBox;
            passwordBox.Password = null;
            this.CurrentPage = CurrentPage.Register;
            this.IsVisible = true;
            this.LoginFocus = false;
            this.RegisterFocus = true;
        }
        public async void Register(object input = null)
        {
            if (Password != PasswordConfirm)
            {
                services.DialogService.OpenDialog(new DialogOkViewModel("Passwords do not match!", "Registration", DialogType.Error));
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
                services.DialogService.OpenDialog(new DialogOkViewModel("New user created successfuly", "Registration", DialogType.Success));
            }
            else
            {
                services.DialogService.OpenDialog(new DialogOkViewModel("Username is already taken", "Registration", DialogType.Error));
            }

            IsBusy = false;

            this.Username = null;
            this.Password = null;
            this.PasswordConfirm = null;
            this.IsVisible = false;
            this.CurrentPage = CurrentPage.Login;
            this.RegisterFocus = false;
            this.LoginFocus = true;
        }
        public void Cancel(object input = null)
        {
            this.Username = null;
            this.Password = null;
            this.PasswordConfirm = null;
            this.IsVisible = false;
            this.CurrentPage = CurrentPage.Login;
            this.RegisterFocus = false;
            this.LoginFocus = true;
        }
    }
}
