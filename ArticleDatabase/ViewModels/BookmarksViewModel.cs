using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ArticleDatabase.ViewModels
{
    public class BookmarksViewModel : BaseViewModel
    {
        private Page _currentPage;

        public User User { get; set; }
        public Page CurrentPage
        {
            get { return _currentPage; }
            set { _currentPage = value; OnPropertyChanged("CurrentPage"); }
        }

        public BookmarksViewModel(User user)
        {
            User = user;
        }
    }
}
