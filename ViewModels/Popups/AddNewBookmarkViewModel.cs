using Lib.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MainLib.ViewModels.Utils;
using MainLib.ViewModels.Pages;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using Lib.DataAccessLayer.Repositories;
using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Info;

namespace MainLib.ViewModels.Popups
{
    public class AddNewBookmarkViewModel : BaseViewModel
    {
        // Private attributes
        private Shared _services;
        private readonly BookmarkListViewModel _parent;

        // Binded properties
        public string BookmarkName { get; set; } = String.Empty;
        public bool IsGlobal { get; set; } = false;
        public User User { get; set; }

        // Commands
        public ICommand CreateCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        // Constructor
        public AddNewBookmarkViewModel(BookmarkListViewModel parent)
        {
            this._parent = parent;
            this._services = Shared.GetInstance();
            this.User = _services.User;
            this.Title = "New bookmark...";

            this.CreateCommand = new RelayCommand(Create);
            this.CancelCommand = new RelayCommand(Cancel);
        }

        // Command actions
        public void Create(object input = null)
        {
            BookmarkRepo repo = new BookmarkRepo();
            BookmarkName = BookmarkName.Trim();

            // Check for blank bookmark title
            if (String.IsNullOrEmpty(BookmarkName))
            {
                _services.DialogService.OpenDialog(new DialogOkViewModel("Bookmark name can not be blank", "Error", DialogType.Error));
                return;
            }

            // Check if bookmark already exists
            if (repo.GetBookmark(BookmarkName, _services.User) != null)
            {
                _services.DialogService.OpenDialog(new DialogOkViewModel("Bookmark already exists", "Error", DialogType.Error));
                return;
            }

            // Create bookmark
            repo.AddBookmark(BookmarkName, IsGlobal ? 1 : 0, _services.User);

            // Track change
            BookmarkInfo info = new BookmarkInfo(BookmarkName, User);
            new Tracker(User).TrackCreate<BookmarkInfo>(info);

            // Refresh bookmark list and close dialog
            _parent.PopulateBookmarks();
            this.Window.Close();
        }
        public void Cancel(object input = null)
        {
            this.Window.Close();
        }
    }
}
