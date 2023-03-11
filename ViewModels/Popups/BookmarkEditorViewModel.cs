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
using Lib.DataAccessLayer.Info;
using System.Data.SqlTypes;

namespace MainLib.ViewModels.Popups
{
    public class BookmarkEditorViewModel : BaseViewModel
    {
        /**
         * Private memebers
         */
        private BookmarkListViewModel _parent;
        private User _user;
        private string _name;

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
                BookmarkRepo bookmarkRepo = new BookmarkRepo();

                // 0. Get old bookmark name
                _name = bookmarkRepo.GetBookmarkNameWithId(Bookmark.ID);

                // 1. Update new values to database
                Bookmark.Name = Bookmark.Name.Trim();
                bookmarkRepo.UpdateBookmark(Bookmark);

                // 1.1 Track bookmark update
                BookmarkInfo info = new BookmarkInfo(Bookmark.Name, User);
                new Tracker(User).TrackUpdate<BookmarkInfo>(info, _name);
            }
            catch(Exception e)
            {
                // Message = "constraint failed\r\nUNIQUE constraint failed: tblBookmark.Name, tblBookmark.UserID"}	System.Exception {System.Data.SQLite.SQLiteException}
                if (e.Message.Contains("UNIQUE"))
                {
                    if (
                        Shared.GetInstance().DialogService.OpenDialog(new DialogYesNoViewModel("Bookmark with that name already exists, do you want to merge?", "Merge reference", DialogType.Question))
                       )
                    {
                        BookmarkRepo repo = new BookmarkRepo();
                        Bookmark bookmarkFrom = repo.GetBookmark(_name, User);
                        Bookmark bookmarkInto = repo.GetBookmark(Bookmark.Name, User);
                        List<Article> articlesFrom = repo.LoadArticlesForBookmark(User, bookmarkFrom);
                        List<Article> articlesInto = repo.LoadArticlesForBookmark(User, bookmarkInto);

                        // 1. Get unique articles between 2 references
                        List<Article> uniques = articlesFrom.Where(art => !articlesInto.Exists(el => el.Title == art.Title)).ToList();

                        // 2. Add those articles into merged reference
                        repo.AddListOfArticlesToBookmark(bookmarkInto, uniques);

                        // 3. Delete old reference
                        repo.DeleteBookmark(bookmarkFrom);
                    }
                }
                else
                {
                    new BugTracker().Track("Bookmark Editor", "Save Bookmark", e.Message, e.StackTrace);
                    Shared.GetInstance().DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
                }
            }
            finally
            {
                // 2. Refresh collections in parent;
                _parent.PopulateBookmarks();

                // 3. Close window
                (input as ICommand).Execute(null);
            }
        }
        public bool CanSaveBookmark(object input = null)
        {
            return !String.IsNullOrWhiteSpace(Bookmark.Name);
        }
    }
}
