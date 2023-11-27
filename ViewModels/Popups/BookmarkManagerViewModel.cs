using Lib.DataAccessLayer.Info;
using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Main;
using MainLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using ViewModels.UIStructs;

namespace MainLib.ViewModels.Popups
{
    public class BookmarkManagerViewModel : BaseViewModel
    {
        // Private members
        private ViewType _parent;
        private Visibility _newBookmarkVisibility;
        private Visibility _createVisibility;
        private string _name;
        private int _global;
        private User _user;
        private Article _article;
        private List<Bookmark> _bookmarks;
        private bool _sortBySelection;
        private Tracker _tracker;
        private Shared services;

        // Public properties
        public Visibility NewBookmarkVisibility
        {
            get { return _newBookmarkVisibility; }
            set { _newBookmarkVisibility = value; OnPropertyChanged("NewBookmarkVisibility"); }
        }
        public Visibility CreateVisibility
        {
            get { return _createVisibility; }
            set { _createVisibility = value; OnPropertyChanged("CreateVisibility"); }
        }
        public ObservableCollection<BookmarkBox> BookmarkBoxes { get; set; }
        public CollectionViewSource _bookmarkBoxesCollection { get; set; }
        public ICollectionView BookmarkBoxesCollection
        {
            get { return _bookmarkBoxesCollection.View; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }
        public int Global
        {
            get { return _global; }
            set { _global = value; OnPropertyChanged("Global"); }
        }
        public bool SortBySelection
        {
            get { return _sortBySelection; }
            set 
            { 
                _sortBySelection = value;
                if (_sortBySelection)
                {
                    _bookmarkBoxesCollection.SortDescriptions.Clear();
                    _bookmarkBoxesCollection.SortDescriptions.Add(new SortDescription("IsChecked", ListSortDirection.Descending));
                }
                else
                {
                    _bookmarkBoxesCollection.SortDescriptions.Clear();
                }
                OnPropertyChanged("SortBySelection");
            }
        }

        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }

        // Commands
        public RelayCommand CreateNewBookmarkCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }
        public RelayCommand CreateCommand { get; set; }
        public RelayCommand CheckChangedCommand { get; set; }

        // Constructor
        public BookmarkManagerViewModel(ViewType parent, Article article = null, List<Bookmark> bookmarks = null)
        {
            this.services = Shared.GetInstance();
            this.Title = "Save to...";
            this._parent = parent;
            this._tracker = new Tracker(User);

            // 1. Initialize starting state
            NewBookmarkVisibility = Visibility.Visible;
            CreateVisibility = Visibility.Collapsed;
            this.User = services.User;
            this._article = article;
            this._bookmarks = bookmarks;
            BookmarkBoxes = new ObservableCollection<BookmarkBox>();
            _bookmarkBoxesCollection = new CollectionViewSource();
            _bookmarkBoxesCollection.Source = BookmarkBoxes;
            PopulateBookmarks();
            //this._initialHeight = MainWindow.CurrentMain.Height * 80 / 100;

            // 2. Set up actions
            CreateNewBookmarkCommand = new RelayCommand(CreateNewBookmark);
            CancelCommand = new RelayCommand(Cancel);
            CreateCommand = new RelayCommand(Create, CanCreate);

            // Case 1: Bookmark manager was opened by DataEntry
            if (_parent == ViewType.DataEntry)
                CheckChangedCommand = new RelayCommand(CheckChangedDataEntry);
            // Case 2: Reference manager was opened by DataView
            else if (_parent == ViewType.DataView)
                CheckChangedCommand = new RelayCommand(CheckChangedDataView);

            // 3. Check bookmark boxes
            CheckBookmarkBoxes();
        }

        // Command actions
        public void CreateNewBookmark(object input = null)
        {
            //Win.Height = _initialHeight + 40;
            NewBookmarkVisibility = Visibility.Collapsed;
            CreateVisibility = Visibility.Visible;
        }
        public void Cancel(object input = null)
        {
            Name = String.Empty;
            Global = 0;
            CreateVisibility = Visibility.Collapsed;
            NewBookmarkVisibility = Visibility.Visible;
        }
        public void Create(object input = null)
        {
            try
            {
                // 1. Add new bookmark to database
                string trimmedName = Name.Trim();
                bool duplicate_check = (new BookmarkRepo()).AddBookmark(trimmedName, Global, User);

                // 1.1 Track change
                BookmarkInfo info = new BookmarkInfo(trimmedName, User);
                new Tracker(User).TrackCreate<BookmarkInfo>(info);

                // 2. Refresh Bookmarks collection
                BookmarkBoxes.Clear();
                PopulateBookmarks();
                Name = "";

                // 3. Hide detailed view
                //Win.Height = _initialHeight;
                CreateVisibility = Visibility.Collapsed;
                NewBookmarkVisibility = Visibility.Visible;

                // 4. Check bookmark boxes
                CheckBookmarkBoxes();

                Global = 0;

                if (!duplicate_check)
                    services.DialogService.OpenDialog(new DialogOkViewModel("This bookmark already exists.", "Warning", DialogType.Warning));
            }
            catch (Exception e)
            {
                new BugTracker().Track("Bookmark Manager", "Create Bookmark", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
        }
        public bool CanCreate(object input = null)
        {
            return !String.IsNullOrWhiteSpace(Name);
        }
        public void CheckChangedDataView(object input)
        {
            BookmarkBox current_bookmark_box = input as BookmarkBox;

            // 1. If user checked add article to bookmark
            if (current_bookmark_box.IsChecked)
            {
                try
                {
                    new BookmarkRepo().AddArticleToBookmark(current_bookmark_box.Bookmark, _article);

                    // Track
                    Couple info = new Couple("Bookmark", "Add", _article.Title, current_bookmark_box.Bookmark.Name);
                    new Tracker(User).TrackCoupling<Couple>(info);
                }
                catch (Exception e)
                {
                    new BugTracker().Track("Bookmark Manager", "Add article to bookmark", e.Message, e.StackTrace);
                    services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
                }
            }
            // 2. If user unchecked remove article from bookmark
            else
            {
                try
                {
                    new BookmarkRepo().RemoveArticleFromBookmark(current_bookmark_box.Bookmark, _article);

                    // Track
                    Couple info = new Couple("Bookmark", "Remove", _article.Title, current_bookmark_box.Bookmark.Name);
                    new Tracker(User).TrackCoupling<Couple>(info);
                }
                catch (Exception e)
                {
                    new BugTracker().Track("Bookmark Manager", "Remove article from bookmark", e.Message, e.StackTrace);
                    services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
                }
            }
        }
        public void CheckChangedDataEntry(object input)
        {
            BookmarkBox current_bookmark_box = input as BookmarkBox;

            // If Bookmarks contains the input remove it
            if (_bookmarks.Exists(el => el.Name == current_bookmark_box.Bookmark.Name))
            {
                try
                {
                    int index = _bookmarks.FindIndex(el => el.Name == current_bookmark_box.Bookmark.Name);
                    _bookmarks.RemoveAt(index);
                }
                catch (Exception e)
                {
                    new BugTracker().Track("Bookmark Manager (Data Entry)", "Remove article from bookmark", e.Message, e.StackTrace);
                    services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
                }
            }
            // If Bookmarks doesn't contain the input add it
            else
            {
                try
                {
                    _bookmarks.Add(current_bookmark_box.Bookmark);
                }
                catch (Exception e)
                {
                    new BugTracker().Track("Bookmark Manager (Data Entry)", "Add article to bookmark", e.Message, e.StackTrace);
                    services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
                }
            }
        }

        /**
         * Private helpers:
         *  - Populate bookmarks 
         *  [Transforms bookmark models into BookmarkBox which have additional attribute to deremine if the bookmark checkbox on UI hast to be checked]
         */
        private void PopulateBookmarks()
        {
            foreach (Bookmark bookmark in (new BookmarkRepo()).LoadBookmarks(User, true))
                BookmarkBoxes.Add(new BookmarkBox(bookmark));
        }
        private void CheckBookmarkBoxes()
        {

            if (_parent == ViewType.DataEntry)
            {
                if (_bookmarks.Count != 0)
                    foreach (BookmarkBox bookmarkBox in BookmarkBoxes)
                    {
                        if (_bookmarks.Exists(el => el.Name == bookmarkBox.Bookmark.Name))
                            bookmarkBox.IsChecked = true;
                        else
                            bookmarkBox.IsChecked = false;
                    }
            }
            else if (_parent == ViewType.DataView)
            {
                foreach (BookmarkBox bookmarkBox in BookmarkBoxes)
                    bookmarkBox.HasArticle(_article);
            }
        }
    }
}
