using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using ArticleDatabase.Dialogs.DialogOk;
using ArticleDatabase.Dialogs.DialogService;
using ArticleDatabase.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ArticleDatabase.Windows.WindowService;

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
                WindowService.OpenWindow(new MainViewModel(CurrentUser), WindowType.MainWindow, false);
                ((LoginWindow)input).Close();
            }
            else
            {
                DialogService.OpenDialog(new DialogOkViewModel("Invalid username or password", "Error", DialogType.Error), input as Window);
            }
        }

        public void Register(object Input = null)
        {
            int admin = CurrentUser.Username.Contains("_adminGfK") ? 1 : 0;
            CurrentUser.Username = CurrentUser.Username.Replace("_adminGfK", "");
            if ((new UserRepo()).Register(CurrentUser, admin))
            {
                DialogService.OpenDialog(new DialogOkViewModel("New user created successfuly", "Result", DialogType.Success), Input as Window);
            }
            else
            {
                DialogService.OpenDialog(new DialogOkViewModel("Username is already taken", "Warning", DialogType.Warning), Input as Window);
            }
        }
    }
}
