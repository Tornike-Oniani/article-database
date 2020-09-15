using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using ArticleDatabase.Dialogs.DialogService;
using ArticleDatabase.Dialogs.DialogYesNo;
using ArticleDatabase.ViewModels.Base;
using ArticleDatabase.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ArticleDatabase.ViewModels
{
    public class BookmarkEditorViewModel : BaseViewModel
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
        public BookmarkEditorViewModel(Bookmark bookmark, BookmarkListViewModel parent)
        {
            this.Title = "Save as...";

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
         public void SaveBookmark(object input)
        {
            try
            {
                // 1. Update new values to database
                (new BookmarkRepo()).UpdateBookmark(Bookmark);

                // 2. Refresh collections in parent;
                _parent.PopulateBookmarks();
            }
            catch
            {
                if (DialogService.OpenDialog(new DialogYesNoViewModel("Bookmark with that name already exists, do you want to merge bookmarks?",
                    "Duplicate", DialogType.Question), MainWindow.CurrentMain))
                {
                    Console.WriteLine("Yes was clicked");

                    // 1. Get duplicate bookmark

                    // 2. Merge bookmarks

                }
            }

            // 3. Close window
            (input as ICommand).Execute(null);
            //this.Close();
        }
        public bool CanSaveBookmark(object input = null)
        {
            return !String.IsNullOrWhiteSpace(Bookmark.Name);
        }
    }
}
