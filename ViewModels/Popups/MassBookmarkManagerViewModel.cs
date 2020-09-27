using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using System.Windows.Data;
using System.ComponentModel;

namespace MainLib.ViewModels.Popups
{
    public class MassBookmarkManagerViewModel : BaseViewModel
    {
        private User _user;
        private List<Article> _articles;
        private IDialogService _dialogService;

        public ObservableCollection<Bookmark> Bookmarks { get; set; }
        public CollectionViewSource _bookmarkBoxesCollection { get; set; }
        public ICollectionView BookmarkBoxesCollection
        {
            get { return _bookmarkBoxesCollection.View; }
        }
        public Bookmark SelectedBookmark { get; set; }

        public RelayCommand AddArticlesToBookmarkCommand { get; set; }

        public MassBookmarkManagerViewModel(User user, List<Article> articles, IDialogService dialogService)
        {
            this.Title = "Save to...";
            this._dialogService = dialogService;

            // 1. Set starting state
            this._user = user;
            this._articles = articles;
            this.Bookmarks = new ObservableCollection<Bookmark>(PopulateBookmarks());
            _bookmarkBoxesCollection = new CollectionViewSource();
            _bookmarkBoxesCollection.Source = Bookmarks;

            // 2. Initialize commands
            AddArticlesToBookmarkCommand = new RelayCommand(AddArticlesToBookmark, CanAddArticlesToBookmark);
        }

        public void AddArticlesToBookmark(object input)
        {
            foreach (Article article in _articles)
                if (!(new BookmarkRepo()).CheckArticleInBookmark(SelectedBookmark, article))
                    (new BookmarkRepo()).AddArticleToBookmark(SelectedBookmark, article);

            _dialogService.OpenDialog(new DialogOkViewModel("Done", "Result", DialogType.Success));

            // Close window
            (input as ICommand).Execute(null);
        }
        public bool CanAddArticlesToBookmark(object input = null)
        {
            return SelectedBookmark != null;
        }

        private List<Bookmark> PopulateBookmarks()
        {
            return (new BookmarkRepo()).LoadBookmarks(_user, true);
        }
    }
}
