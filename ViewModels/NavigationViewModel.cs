﻿using Lib.DataAccessLayer.Models;
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

namespace MainLib.ViewModels
{
    public class NavigationViewModel : BaseViewModel
    {
        private string _title;
        private BaseViewModel _selectedViewModel;
        private User _user;

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

        public ICommand UpdateViewCommand { get; set; }

        public NavigationViewModel(User user, IDialogService dialogService, IWindowService windowService, IBrowserService browserService)
        {
            UpdateViewCommand = new UpdateViewCommand(this, user, dialogService, windowService, browserService);
            //this.SelectedViewModel = new HomeViewModel(user, new DialogService(), new WindowService(), new BrowserService());
            UpdateViewCommand.Execute(ViewType.Home);
            this.Title = user.Username;
            // Set admin/user status
            user.Admin = (new UserRepo()).IsAdmin(user);
            this.User = user;

            // Initialize tracker
            //new Tracker().init();
        }
    }
}
