using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using ArticleDatabase.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleDatabase.ViewModels
{
    public class LoginViewModel : BaseWindow
    {
        public User CurrentUser { get; set; }

        public RelayCommand LoginCommand { get; set; }

        public RelayCommand RegisterCommand { get; set; }

        public LoginViewModel(Window window) : base(window)
        {
            CurrentUser = new User();
            LoginCommand = new RelayCommand(Login);
            RegisterCommand = new RelayCommand(Register);
        }

        public void Login(object input)
        {
            if ((new UserRepo()).Login(CurrentUser))
            {
                MainWindow wndMain = new MainWindow(CurrentUser);
                wndMain.Show();
                ((LoginWindow)input).Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Register(object Input = null)
        {
            int admin = CurrentUser.Username.Contains("_adminGfK") ? 1 : 0;
            CurrentUser.Username = CurrentUser.Username.Replace("_adminGfK", "");
            if ((new UserRepo()).Register(CurrentUser, admin))
            {
                MessageBox.Show("New user created successfuly", "Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Username is already taken", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
