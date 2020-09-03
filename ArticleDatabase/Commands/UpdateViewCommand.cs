using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.ViewModels;
using ArticleDatabase.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ArticleDatabase.Commands
{
    public class UpdateViewCommand : ICommand
    {
        private MainViewModel _mainViewModel;
        private User _user;

        public UpdateViewCommand(MainViewModel mainViewModel, User user)
        {
            _mainViewModel = mainViewModel;
            _user = user;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            switch ((View)parameter)
            {
                case View.Home:
                    this._mainViewModel.SelectedViewModel = new HomeViewModel(_user);
                    break;
                case View.DataEntry:
                    this._mainViewModel.SelectedViewModel = new DataEntryViewModel(_user);
                    break;
                case View.DataView:
                    this._mainViewModel.SelectedViewModel = new DataViewViewModel(_user);
                    break;
                case View.Bookmarks:
                    this._mainViewModel.SelectedViewModel = new BookmarksViewModel(_user);
                    break;
                case View.References:
                    this._mainViewModel.SelectedViewModel = new ReferencesViewModel(_user);
                    break;
            }
        }
    }
}
