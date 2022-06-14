using Lib.Views;
using Lib.Views.Services.Browser;
using Lib.Views.Services.Dialogs;
using Lib.Views.Services.Windows;
using MainLib.ViewModels;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Main
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : WindowBase
    {
        LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();

            this.Loaded += LoginWindow_Loaded;
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = new LoginViewModel(new DialogService(), new WindowService(), new BrowserService());
            _viewModel.Window = this;
            this.DataContext = _viewModel;
            txbUsername.Focus();
            // We have to set initial tag to make it work
            txbPassword.Tag = (!string.IsNullOrEmpty(txbPassword.Password)).ToString();


            ConfigurationManager.AppSettings["Attach"] = "ATTACH DATABASE \'" + Environment.CurrentDirectory.ToString() + "\\" + "User.sqlite3\'" + "AS user;";
        }

        // Since PasswordBox Password property can't be checked in XAML
        // we set Tag property to True or False base on if Password is
        // empty or not and then we use Tag property to check for it
        // in XAML triggers
        private void txbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            passwordBox.Tag = (!string.IsNullOrEmpty(passwordBox.Password)).ToString();
        }
    }
}