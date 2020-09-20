using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Main.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Lib.ViewModels.Base;
using MainLib.ViewModels.Main;
using Lib.Views.Services.Browser;
using Lib.Views.Services.Dialogs;
using Lib.Views.Services.Windows;

namespace Main.ViewModels
{
    public class MainViewModel : BaseViewModel
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

        public MainViewModel(User user)
        {
            UpdateViewCommand = new UpdateViewCommand(this, user);
            this.SelectedViewModel = new HomeViewModel(user, new DialogService(), new WindowService(), new BrowserService());
            this.Title = user.Username;
            // Set admin/user status
            user.Admin = (new UserRepo()).IsAdmin(user);
            this.User = user;
        }
    }
}
