using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Popups;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;
using MainLib.ViewModels.Utils;

namespace MainLib.ViewModels.Pages
{
    public class BookmarkListViewModel : BaseViewModel
    {
        // Private members
        IDialogService _dialogService;
        IWindowService _windowService;

        /**
         * Public properties
         */
        public CollectionViewSource _bookmarksCollection { get; set; }
        public CollectionViewSource _globalBookmarksCollection { get; set; }
        public User User { get; set; }
        // Container of local bookmarks
        public ObservableCollection<Bookmark> Bookmarks { get; set; }
        public ICollectionView BookmarksCollection { get { return _bookmarksCollection.View; } }
        // Container of global bookmarks
        public ObservableCollection<Bookmark> GlobalBookmarks { get; set; }
        public ICollectionView GlobalBookmarksCollection { get { return _globalBookmarksCollection.View; } }
        public Action<bool> WorkStatus { get; set; }

        /**
         * Commands:
         *  - Open bookmark
         *  [The action of this command is in code behind of BookmarkList view because we use NavigationService which is not accessible from view model]
         *  - Delete bookmark
         */
        public RelayCommand OpenBookmarkCommand { get; set; }
        public RelayCommand EditBookmarkCommand { get; set; }
        public RelayCommand DeleteBookmarkCommand { get; set; }

        // Constructor
        public BookmarkListViewModel(User user, Action<bool> workStatus, IDialogService dialogService, IWindowService windowService)
        {
            this.User = user;
            this.WorkStatus = workStatus;
            this._dialogService = dialogService;
            this._windowService = windowService;
            Bookmarks = new ObservableCollection<Bookmark>();
            _bookmarksCollection = new CollectionViewSource();
            _bookmarksCollection.Source = Bookmarks;
            GlobalBookmarks = new ObservableCollection<Bookmark>();
            _globalBookmarksCollection = new CollectionViewSource();
            _globalBookmarksCollection.Source = GlobalBookmarks;
            PopulateBookmarks();

            // Initialize commands
            EditBookmarkCommand = new RelayCommand(EditBookmark);
            DeleteBookmarkCommand = new RelayCommand(DeleteBookmark);
        }

        /**
         * Command actions
         */
        public void DeleteBookmark(object input)
        {
            try
            {
                // 1. Retrieve sent bookmark
                Bookmark selected_bookmark = input as Bookmark;

                // 2. Ask user if they are sure
                if (_dialogService.OpenDialog(new DialogYesNoViewModel("Delete following bookmark?\n" + selected_bookmark.Name, "Check", DialogType.Question)))
                {
                    // 3. Delete bookmark record from database
                    new BookmarkRepo().DeleteBookmark(selected_bookmark);

                    // 3.1 Track bookmark delete
                    new Tracker(User).TrackDelete("Bookmark", selected_bookmark.Name);

                    // 4. Refresh bookmark collections
                    PopulateBookmarks();
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Bookmark List", "Delete bookmark", e.Message);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
        }
        public void EditBookmark(object input)
        {
            _windowService.OpenWindow(new BookmarkEditorViewModel(input as Bookmark, this, _dialogService));
        }

        /**
         * Public methods
         */
        public async void PopulateBookmarks(bool global = false)
        {
            try
            {
                WorkStatus(true);

                // 1. Clear bookmarks
                Bookmarks.Clear();
                GlobalBookmarks.Clear();

                // 2. Populate local bookmarks
                await Populate(false);

                // 3. Populate global bookmarks
                await Populate(true);

                WorkStatus(false);
            }
            catch(Exception e)
            {
                new BugTracker().Track("Bookmark List", "Populate Bookmarks", e.Message);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                WorkStatus(false);
            }
        }

        private async Task Populate(bool global)
        {
            List<Bookmark> bookmarks = new List<Bookmark>();
            BookmarkRepo repo = new BookmarkRepo();

            if (global)
            {
                await Task.Run(() =>
                {
                    // 3. Populate global bookmarks
                    foreach (Bookmark bookmark in (new BookmarkRepo()).LoadGlobalBookmarks())
                    {
                        //bookmark.PopulateArticles(User);
                        bookmark.GetArticleCount(User);
                        bookmarks.Add(bookmark);
                    }
                });

                foreach (Bookmark bookmark in bookmarks)
                    GlobalBookmarks.Add(bookmark);
            }
            else
            {
                await Task.Run(() =>
                {
                    // 2. Populate local bookmarks
                    foreach (Bookmark bookmark in repo.LoadBookmarks(User))
                    {
                        // Populate articles colletion for each bookmark
                        //bookmark.PopulateArticles(User);
                        bookmark.GetArticleCount(User);
                        bookmarks.Add(bookmark);
                    }
                });

                foreach (Bookmark bookmark in bookmarks)
                    Bookmarks.Add(bookmark);
            }
        }
    }
}
