﻿using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using ArticleDatabase.Dialogs.DialogOk;
using ArticleDatabase.Dialogs.DialogService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ArticleDatabase.Windows.WindowService;
using System.Windows.Input;
using ViewModels.Base;
using ViewModels.Commands;
using ViewModels.Services.Windows;
using ViewModels.Services.Dialogs;

namespace ArticleDatabase.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public User CurrentUser { get; set; }

        public RelayCommand LoginCommand { get; set; }
        public RelayCommand RegisterCommand { get; set; }

        public LoginViewModel()
        {
            CurrentUser = new User();
            LoginCommand = new RelayCommand(Login);
            RegisterCommand = new RelayCommand(Register);
        }

        public void Login(object input)
        {
            if ((new UserRepo()).Login(CurrentUser))
            {
                new WindowService().OpenWindow(new MainViewModel(CurrentUser), WindowType.MainWindow, false);
                (input as ICommand).Execute(null);
            }
            else
            {
                new DialogService().OpenDialog(new DialogOkViewModel("Invalid username or password", "Error", DialogType.Error));
            }
        }

        public void Register(object Input = null)
        {
            int admin = CurrentUser.Username.Contains("_adminGfK") ? 1 : 0;
            CurrentUser.Username = CurrentUser.Username.Replace("_adminGfK", "");
            if ((new UserRepo()).Register(CurrentUser, admin))
            {
                new DialogService().OpenDialog(new DialogOkViewModel("New user created successfuly", "Result", DialogType.Success));
            }
            else
            {
                new DialogService().OpenDialog(new DialogOkViewModel("Username is already taken", "Warning", DialogType.Warning));
            }
        }
    }
}
