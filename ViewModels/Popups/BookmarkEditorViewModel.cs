using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Pages;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Utils;

namespace MainLib.ViewModels.Popups
{
    public class BookmarkEditorViewModel : BaseViewModel
    {
        /**
         * Private memebers
         */
        private BookmarkListViewModel _parent;
        private User _user;
        private IDialogService _dialogService;

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
        public BookmarkEditorViewModel(Bookmark bookmark, BookmarkListViewModel parent, IDialogService dialogService)
        {
            this.Title = "Save as...";
            this._dialogService = dialogService;

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
                BookmarkRepo bookmarkRepo = new BookmarkRepo();

                // 0. Get old bookmark name
                string name = bookmarkRepo.GetBookmarkNameWithId(Bookmark.ID);

                // 1. Update new values to database
                bookmarkRepo.UpdateBookmark(Bookmark);

                // 1.1 Track bookmark update
                BookmarkInfo info = new BookmarkInfo(Bookmark.Name, User);
                new Tracker(User).TrackUpdate<BookmarkInfo>(info, name);

                // 2. Refresh collections in parent;
                _parent.PopulateBookmarks();
            }
            catch
            {
                _dialogService.OpenDialog(new DialogOkViewModel("Bookmark with that name already exists", "Edit bookmark", DialogType.Error));
            }

           // 3. Close window
           (input as ICommand).Execute(null);
        }
        public bool CanSaveBookmark(object input = null)
        {
            return !String.IsNullOrWhiteSpace(Bookmark.Name);
        }
    }
}
