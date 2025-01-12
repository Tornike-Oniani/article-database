﻿using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Popups;
using MainLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace MainLib.ViewModels.Pages
{
    public class BookmarkListViewModel : BaseViewModel
    {
        // Private members
        private readonly Shared services;
        private string _searchString;

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
        public string SearchString
        {
            get { return _searchString; }
            set 
            { 
                _searchString = value;
                _bookmarksCollection.View.Refresh();
                _globalBookmarksCollection.View.Refresh();
                OnPropertyChanged("SearchString"); 
            }
        }

        /**
         * Commands:
         *  [The action of this command is in code behind of BookmarkList view because we use NavigationService which is not accessible from view model]
         *  - Open bookmark
         *  - Delete bookmark
         */
        public RelayCommand OpenBookmarkCommand { get; set; }
        public ICommand AddNewBookmarkCommand { get; set; }
        public RelayCommand EditBookmarkCommand { get; set; }
        public RelayCommand DeleteBookmarkCommand { get; set; }
        public ICommand ClearSearchCommand { get; set; }

        // Constructor
        public BookmarkListViewModel()
        {
            this.services = Shared.GetInstance();
            this.User = services.User;
            Bookmarks = new ObservableCollection<Bookmark>();
            _bookmarksCollection = new CollectionViewSource();
            _bookmarksCollection.Source = Bookmarks;
            GlobalBookmarks = new ObservableCollection<Bookmark>();
            _globalBookmarksCollection = new CollectionViewSource();
            _globalBookmarksCollection.Source = GlobalBookmarks;
            PopulateBookmarks();

            // Initialize commands
            this.AddNewBookmarkCommand = new RelayCommand(AddNewBookmark);
            EditBookmarkCommand = new RelayCommand(EditBookmark);
            DeleteBookmarkCommand = new RelayCommand(DeleteBookmark);
            this.ClearSearchCommand = new RelayCommand(ClearSearch);

            this._bookmarksCollection.Filter += FilterBookmarks;
            this._globalBookmarksCollection.Filter += FilterBookmarks;
        }

        /**
         * Command actions
         */
        public void AddNewBookmark(object input = null)
        {
            this.services.ShowWindowWithOverlay(new AddNewBookmarkViewModel(this), passWindow: true);
        }
        public void EditBookmark(object input)
        {
            this.services.ShowWindowWithOverlay(new BookmarkEditorViewModel(input as Bookmark, this));
        }
        public void DeleteBookmark(object input)
        {
            try
            {
                // 1. Retrieve sent bookmark
                Bookmark selected_bookmark = input as Bookmark;

                // 2. Ask user if they are sure
                if (services.ShowDialogWithOverlay(new DialogYesNoViewModel("Delete selected bookmark?", "Check", DialogType.Question)))
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
                new BugTracker().Track("Bookmark List", "Delete bookmark", e.Message, e.StackTrace);
                services.ShowDialogWithOverlay(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
        }
        public void ClearSearch(object input = null)
        {
            this.SearchString = String.Empty;
        }

        /**
         * Public methods
         */
        public async void PopulateBookmarks(bool global = false)
        {
            try
            {
                services.IsWorking(true);

                // 1. Clear bookmarks
                Bookmarks.Clear();
                GlobalBookmarks.Clear();

                // 2. Populate local bookmarks
                await Populate(false);

                // 3. Populate global bookmarks
                await Populate(true);

                services.IsWorking(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Bookmark List", "Populate Bookmarks", e.Message, e.StackTrace);
                services.ShowDialogWithOverlay(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
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

        /**
         * Private helpers
         */
        private void FilterBookmarks(object sender, FilterEventArgs e)
        {
            // Edge case: filter text is empty
            if (String.IsNullOrWhiteSpace(SearchString))
            {
                e.Accepted = true;
                return;
            }

            e.Accepted = false;

            Bookmark current = e.Item as Bookmark;
            if (current.Name.ToLower().Contains(SearchString.ToLower()))
            {
                e.Accepted = true;
            }
        }
    }
}
