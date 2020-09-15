using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.ViewModels;
using ArticleDatabase.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArticleDatabase
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
            _viewModel = new LoginViewModel();
            this.DataContext = _viewModel;
            txbUsername.Focus();

            ConfigurationManager.AppSettings["Attach"] = "ATTACH DATABASE \'" + Environment.CurrentDirectory.ToString() + "\\" + "User.sqlite3\'" + "AS user;";
        }
    }
}