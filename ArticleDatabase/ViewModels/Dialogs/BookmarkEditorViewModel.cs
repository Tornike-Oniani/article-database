﻿using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using ArticleDatabase.ViewModels.Base;
using ArticleDatabase.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleDatabase.ViewModels.Dialogs
{
    public class BookmarkEditorViewModel : BaseWindow
    {
        /**
         * Private memebers
         */
        private BookmarkListViewModel _parent;
        private User _user;

        /**
         * Public properties
         */
        public Bookmark Bookmark { get; set; }
        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }


        /**
         * Commands:
         *  - Save bookmark
         *  [Updates bookmark with new values]
         */
        public RelayCommand SaveBookmarkCommand { get; set; }

        // Constructor
        public BookmarkEditorViewModel(Bookmark bookmark, BookmarkListViewModel parent, Window window) : base(window)
        {
            // 1. Copy sent bookmark values and set the parent
            Bookmark = new Bookmark();
            Bookmark.CopyByValue(bookmark);
            this._parent = parent;
            this.User = parent.User;

            // 2. Initialize commands
            SaveBookmarkCommand = new RelayCommand(SaveBookmark, CanSaveBookmark);
        }

        /**
         * Command actions
         */
         public void SaveBookmark(object input = null)
        {
            // 1. Update new values to database
            (new BookmarkRepo()).UpdateBookmark(Bookmark);

            // 2. Refresh collections in parent;
            _parent.PopulateBookmarks();
        }
        public bool CanSaveBookmark(object input = null)
        {
            return !String.IsNullOrWhiteSpace(Bookmark.Name);
        }
    }
}
