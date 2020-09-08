using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using ArticleDatabase.Dialogs.DialogOk;
using ArticleDatabase.Dialogs.DialogService;
using ArticleDatabase.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleDatabase.ViewModels.Dialogs
{
    class MassBookmarkManagerViewModel : BaseWindow
    {
        private User _user;
        private List<Article> _articles;

        public ObservableCollection<Bookmark> Bookmarks { get; set; }
        public Bookmark SelectedBookmark { get; set; }

        public RelayCommand AddArticlesToBookmarkCommand { get; set; }

        public MassBookmarkManagerViewModel(Window window, User user, List<Article> articles) : base(window)
        {
            // 1. Set starting state
            this._user = user;
            this._articles = articles;
            this.Bookmarks = new ObservableCollection<Bookmark>(PopulateBookmarks());

            // 2. Initialize commands
            AddArticlesToBookmarkCommand = new RelayCommand(AddArticlesToBookmark, CanAddArticlesToBookmark);
        }

        public void AddArticlesToBookmark(object input = null)
        {
            foreach (Article article in _articles)
                if (!(new BookmarkRepo()).CheckArticleInBookmark(SelectedBookmark, article))
                    (new BookmarkRepo()).AddArticleToBookmark(SelectedBookmark, article);

            DialogService.OpenDialog(new DialogOkViewModel("Done", "Result", DialogType.Success), MainWindow.CurrentMain);
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
