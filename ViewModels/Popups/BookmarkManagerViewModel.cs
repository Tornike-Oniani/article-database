﻿using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ViewModels.Base;
using ViewModels.Commands;
using ViewModels.Main;
using ViewModels.Services;
using ViewModels.Services.Dialogs;
using ViewModels.UIStructs;

namespace ViewModels.Popups
{
    public class BookmarkManagerViewModel : BaseViewModel
    {
        // Private members
        private Visibility _newBookmarkVisibility;
        private Visibility _createVisibility;
        private string _name;
        private int _global;
        private DataViewViewModel _parent;
        private User _user;
        private Article _article;
        private double _initialHeight;
        private IDialogService _dialogService;

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
        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }

        // Commands
        public RelayCommand CreateNewBookmarkCommand { get; set; }
        public RelayCommand CreateCommand { get; set; }
        public RelayCommand CheckChangedCommand { get; set; }

        // Constructor
        public BookmarkManagerViewModel(DataViewViewModel parent, IDialogService dialogService, Article article = null)
        {
            this.Title = "Save to...";
            this._dialogService = dialogService;

            // 1. Initialize starting state
            NewBookmarkVisibility = Visibility.Visible;
            CreateVisibility = Visibility.Collapsed;
            this._parent = parent;
            this.User = _parent.User;
            this._article = article;
            BookmarkBoxes = new ObservableCollection<BookmarkBox>();
            _bookmarkBoxesCollection = new CollectionViewSource();
            _bookmarkBoxesCollection.Source = BookmarkBoxes;
            PopulateBookmarks();
            //this._initialHeight = MainWindow.CurrentMain.Height * 80 / 100;

            // 2. Set up actions
            CreateNewBookmarkCommand = new RelayCommand(CreateNewBookmark);
            CreateCommand = new RelayCommand(Create, CanCreate);
            CheckChangedCommand = new RelayCommand(CheckChanged);

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
        public void Create(object input = null)
        {
            // 1. Add new bookmark to database
            string trimmedName = Name.Trim();
            bool duplicate_check = (new BookmarkRepo()).AddBookmark(trimmedName, Global, _parent.User);

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
                _dialogService.OpenDialog(new DialogOkViewModel("This bookmark already exists.", "Warning", DialogType.Warning));
        }
        public bool CanCreate(object input = null)
        {
            return !String.IsNullOrWhiteSpace(Name);
        }
        public void CheckChanged(object input)
        {
            BookmarkBox current_bookmark_box = input as BookmarkBox;

            // 1. If user checked add article to bookmark
            if (current_bookmark_box.IsChecked)
            {
                (new BookmarkRepo()).AddArticleToBookmark(current_bookmark_box.Bookmark, _article);
            }
            // 2. If user unchecked remove article from bookmark
            else
            {
                (new BookmarkRepo()).RemoveArticleFromBookmark(current_bookmark_box.Bookmark, _article);
            }
        }

        /**
         * Private helpers:
         *  - Populate bookmarks 
         *  [Transforms bookmark models into BookmarkBox which have additional attribute to deremine if the bookmark checkbox on UI hast to be checked]
         */
        private void PopulateBookmarks()
        {
            foreach (Bookmark bookmark in (new BookmarkRepo()).LoadBookmarks(_parent.User, true))
                BookmarkBoxes.Add(new BookmarkBox(bookmark, _parent.SelectedArticle));
        }
        private void CheckBookmarkBoxes()
        {
            foreach (BookmarkBox bookmarkBox in BookmarkBoxes)
                bookmarkBox.HasArticle(_article);
        }
    }
}