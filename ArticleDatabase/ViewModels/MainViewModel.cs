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
using System.Windows.Input;

namespace ArticleDatabase.ViewModels
{
    public class MainViewModel : BaseWindow
    {

        private BaseViewModel _selectedViewModel;
        private User _user;

        public BaseViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set { _selectedViewModel = value; OnPropertyChanged("SelectedViewModel"); }
        }

        public ICommand UpdateViewCommand { get; set; }

        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }


        public MainViewModel(User user)
        {
            UpdateViewCommand = new UpdateViewCommand(this, user);
            this.SelectedViewModel = new HomeViewModel(user);
            this.Title = user.Username;
            // Set admin/user status
            user.Admin = (new UserRepo()).IsAdmin(user);
            this.User = user;
        }
    }
}
