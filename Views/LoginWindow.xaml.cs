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

            ConfigurationManager.AppSettings["Attach"] = "ATTACH DATABASE \'" + Environment.CurrentDirectory.ToString() + "\\" + "User.sqlite3\'" + "AS user;";
        }

        // Animate watermark when password is changed
        // we do this in code behind because we can't 
        // check if password is empty in XAML
        private void txbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passBox = sender as PasswordBox;

            DoubleAnimation moveAnimation = new DoubleAnimation(0, -24, new Duration(TimeSpan.FromSeconds(0.1)));
            Border watermark = (Border)passBox.Template.FindName("watermark", passBox);
            watermark.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
        }
    }
}