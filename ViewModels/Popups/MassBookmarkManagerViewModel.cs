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
using MainLib.ViewModels.Utils;
using Lib.DataAccessLayer.Info;

namespace MainLib.ViewModels.Popups
{
    public class MassBookmarkManagerViewModel : BaseViewModel
    {
        private User _user;
        private List<Article> _articles;
        private Shared services;

        public ObservableCollection<Bookmark> Bookmarks { get; set; }
        public CollectionViewSource _bookmarkBoxesCollection { get; set; }
        public ICollectionView BookmarkBoxesCollection
        {
            get { return _bookmarkBoxesCollection.View; }
        }
        public Bookmark SelectedBookmark { get; set; }

        public RelayCommand AddArticlesToBookmarkCommand { get; set; }

        public MassBookmarkManagerViewModel(List<Article> articles)
        {
            this.services = Shared.GetInstance();
            this.Title = "Save to...";

            // 1. Set starting state
            this._user = services.User;
            this._articles = articles;
            this.Bookmarks = new ObservableCollection<Bookmark>(PopulateBookmarks());
            _bookmarkBoxesCollection = new CollectionViewSource();
            _bookmarkBoxesCollection.Source = Bookmarks;

            // 2. Initialize commands
            AddArticlesToBookmarkCommand = new RelayCommand(AddArticlesToBookmark, CanAddArticlesToBookmark);
        }

        public async void AddArticlesToBookmark(object input)
        {
            try
            {
                // Close window
                (input as ICommand).Execute(null);

                services.IsWorking(true);

                await Task.Run(() =>
                {
                    BookmarkRepo repo = new BookmarkRepo();
                    foreach (Article article in _articles)
                        if (!repo.CheckArticleInBookmark(SelectedBookmark, article))
                        {
                            // 1. Add article to bookmark
                            repo.AddArticleToBookmark(SelectedBookmark, article);

                            // 2. Track
                            Couple info = new Couple("Bookmark", "Add", article.Title, SelectedBookmark.Name);
                            new Tracker(_user).TrackCoupling<Couple>(info);
                        }
                });

                services.IsWorking(false);

                services.ShowDialogWithOverlay(new DialogOkViewModel("Done", "Result", DialogType.Success));
            }
            catch(Exception e)
            {
                new BugTracker().Track("Mass Bookmark Manager", "Mass Bookmark", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
            }
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
