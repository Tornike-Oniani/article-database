using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using ArticleDatabase.Dialogs.DialogService;
using ArticleDatabase.Dialogs.DialogYesNo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace ArticleDatabase.ViewModels.Pages
{
    public class BookmarkListViewModel : BaseViewModel
    {
        private CollectionViewSource _bookmarksCollection;
        private CollectionViewSource _globalBookmarksCollection;
        private string _filterText;
        private string _filterTextGlobal;

        private string _bookmarkTitle;

        public string BookmarkTitle
        {
            get { return _bookmarkTitle; }
            set { _bookmarkTitle = value; OnPropertyChanged("BookmarkTitle"); }
        }


        /**
         * Public properties
         */
        public User User { get; set; }
        // Container of local bookmarks
        public ObservableCollection<Bookmark> Bookmarks { get; set; }
        public ICollectionView BookmarksCollection { get { return _bookmarksCollection.View; } }
        // Container of global bookmarks
        public ObservableCollection<Bookmark> GlobalBookmarks { get; set; }
        public ICollectionView GlobalBookmarksCollection { get { return _globalBookmarksCollection.View; } }
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                _bookmarksCollection.View.Refresh();
                OnPropertyChanged("FilterText");
            }
        }
        public string FilterTextGlobal
        {
            get { return _filterTextGlobal; }
            set
            {
                _filterTextGlobal = value;
                _globalBookmarksCollection.View.Refresh();
                OnPropertyChanged("FilterTextGlobal");
            }
        }

        /**
         * Commands:
         *  - Open bookmark
         *  [The action of this command is in code behind of BookmarkList view because we use NavigationService which is not accessible from view model]
         *  - Delete bookmark
         */
        public RelayCommand OpenBookmarkCommand { get; set; }
        public RelayCommand DeleteBookmarkCommand { get; set; }

        // Constructor
        public BookmarkListViewModel(User user)
        {
            this.User = user;
            Bookmarks = new ObservableCollection<Bookmark>();
            _bookmarksCollection = new CollectionViewSource();
            _bookmarksCollection.Source = Bookmarks;
            _bookmarksCollection.Filter += _bookmarksCollection_Filter;
            GlobalBookmarks = new ObservableCollection<Bookmark>();
            _globalBookmarksCollection = new CollectionViewSource();
            _globalBookmarksCollection.Source = GlobalBookmarks;
            _globalBookmarksCollection.Filter += _globalBookmarksCollection_Filter;
            PopulateBookmarks();

            // Initialize commands
            DeleteBookmarkCommand = new RelayCommand(DeleteBookmark);
        }

        private void _bookmarksCollection_Filter(object sender, FilterEventArgs e)
        {
            // Edge case: filter text is empty
            if (String.IsNullOrWhiteSpace(FilterText))
            {
                e.Accepted = true;
                return;
            }

            Bookmark current = e.Item as Bookmark;
            if (current.Name.ToUpper().Contains(FilterText.ToUpper()))
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }

        private void _globalBookmarksCollection_Filter(object sender, FilterEventArgs e)
        {
            // Edge case: filter text is empty
            if (String.IsNullOrWhiteSpace(FilterTextGlobal))
            {
                e.Accepted = true;
                return;
            }

            Bookmark current = e.Item as Bookmark;
            if (current.Name.ToUpper().Contains(FilterTextGlobal.ToUpper()))
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }

        /**
         * Command actions
         */
        public void DeleteBookmark(object input)
        {
            // 1. Retrieve sent bookmark
            Bookmark selected_bookmark = input as Bookmark;

            // 2. Ask user if they are sure
            if (DialogService.OpenDialog(new DialogYesNoViewModel("Delete following bookmark?\n" + selected_bookmark.Name, "Check", DialogType.Question), MainWindow.CurrentMain))
            {
                // 3. Delete bookmark record from database
                (new BookmarkRepo()).DeleteBookmark(selected_bookmark);

                // 4. Refresh bookmark collections
                PopulateBookmarks();
            }
        }

        /**
         * Public methods
         */
        public void PopulateBookmarks(bool global = false)
        {
            // 1. Clear bookmarks
            Bookmarks.Clear();
            GlobalBookmarks.Clear();

            // 2. Populate local bookmarks
            foreach (Bookmark bookmark in (new BookmarkRepo()).LoadBookmarks(User))
            {
                // Populate articles colletion for each bookmark
                //bookmark.PopulateArticles(User);
                bookmark.GetArticleCount(User);
                Bookmarks.Add(bookmark);
            }

            // 3. Populate global bookmarks
            foreach (Bookmark bookmark in (new BookmarkRepo()).LoadGlobalBookmarks())
            {
                //bookmark.PopulateArticles(User);
                bookmark.GetArticleCount(User);
                GlobalBookmarks.Add(bookmark);
            }
        }

    }
}
