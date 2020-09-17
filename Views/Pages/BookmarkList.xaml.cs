using ArticleDatabase.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewModels.Commands;
using ViewModels.Pages;
using Views.Services.Dialogs;

namespace Views.Pages
{
    /// <summary>
    /// Interaction logic for BookmarkList.xaml
    /// </summary>
    public partial class BookmarkList : Page
    {
        public BookmarkList(BookmarkListViewModel vm)
        {
            InitializeComponent();

            // 1. Initialize commands
            vm.OpenBookmarkCommand = new RelayCommand(OpenBookmark);
        }

        /**
         * Command actions:
         *  - Open bookmark
         *  [The actual command is in viewmodel]
         */
        public void OpenBookmark(object input)
        {
            BookmarkListViewModel vm = (BookmarkListViewModel)this.DataContext;
            Bookmark sent_bookmark = input as Bookmark;
            sent_bookmark.PopulateArticles(vm.User);
            bool modify_rights = true;
            if (sent_bookmark.Global == 1 && !vm.User.IsAdmin)
                modify_rights = false;

            Page _bookmarkView = new BookmarkView();
            _bookmarkView.DataContext = new BookmarkViewViewModel(sent_bookmark, vm.User, new DialogService(), modify_rights);
            NavigationService.Navigate(_bookmarkView);
        }

        private void ListView_LostFocus(object sender, RoutedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}
