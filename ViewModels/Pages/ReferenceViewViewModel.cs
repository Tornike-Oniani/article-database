using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services;
using Lib.ViewModels.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Navigation;
using MainLib.ViewModels.Utils;
using Lib.DataAccessLayer.Info;

namespace MainLib.ViewModels.Pages
{
    public class ReferenceViewViewModel : BaseViewModel
    {
        private User _user;
        private Action<bool> _workStatus;
        private IDialogService _dialogService;

        /**
         * Public properties:
         *  - Bookmark
         *  - Selected article
         *  [Selected article in data grid]
         */
        public Reference Reference { get; set; }
        public Article SelectedArticle { get; set; }
        public ObservableCollection<Article> Articles { get; set; }
        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }


        /**
         * Commands:
         *  - Open file
         *  [Opens selected file in .pdf browser]
         *  - Export bookmark
         *  [Exports all files in bookmark into selected folder]
         *  - Remove article
         *  [Removes article from bookmark]
         *  - Copy
         *  [Copy's article name to clipboar]
         */
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand RemoveArticleCommand { get; set; }
        public RelayCommand CopyCommand { get; set; }

        // Constructor
        public ReferenceViewViewModel(Reference reference, User user, Action<bool> workStatus, IDialogService dialogService)
        {
            this.Reference = reference;
            this.User = user;
            this.Articles = new ObservableCollection<Article>();
            this._workStatus = workStatus;
            this._dialogService = dialogService;

            // Initialize commands
            OpenFileCommand = new RelayCommand(OpenFile);
            RemoveArticleCommand = new RelayCommand(RemoveArticle, CanOnSelectedArticle);
            CopyCommand = new RelayCommand(Copy, CanOnSelectedArticle);
        }

        /**
         * Command actions
         */
        public async Task PopulateReferenceArticles()
        {
            try
            {
                _workStatus(true);

                Articles.Clear();

                List<Article> articles = new List<Article>();

                await Task.Run(() =>
                {
                    foreach (Article article in (new ReferenceRepo()).LoadArticlesForReference(Reference))
                        articles.Add(article);
                });

                foreach (Article article in articles)
                    Articles.Add(article);

                _workStatus(false);
            }
            catch(Exception e)
            {
                new BugTracker().Track("Reference View", "Populate references", e.Message);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                _workStatus(false);
            }
        }
        public void OpenFile(object input = null)
        {
            // 1. If no item was selected return
            if (SelectedArticle == null)
                return;

            // 2. Get full path of the file
            string full_path = Environment.CurrentDirectory + "\\Files\\" + SelectedArticle.FileName + ".pdf";

            // 3. Open file with default program
            try
            {
                Process.Start(full_path);
            }

            // 4. Catch if file doesn't exist physically
            catch
            {
                _dialogService.OpenDialog(new DialogOkViewModel("File was not found", "Error", DialogType.Error));
            }
        }
        public async void RemoveArticle(object input = null)
        {
            try
            {
                // 1. Remove article from bookmark in database
                new ReferenceRepo().RemoveArticleFromReference(Reference, SelectedArticle);

                // 1.1 Track removing article from reference
                Couple info = new Couple("Reference", "Remove", SelectedArticle.Title, Reference.Name);
                new Tracker(User).TrackCoupling<Couple>(info);

                // 2. Refresh articles collection
                await PopulateReferenceArticles();
            }
            catch (Exception e)
            {
                new BugTracker().Track("Reference List", "Remove article", e.Message);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
        }
        public void Copy(object input = null)
        {
            Clipboard.SetText(SelectedArticle.Title);
        }
        public bool CanOnSelectedArticle(object input = null)
        {
            return SelectedArticle != null;
        }
    }
}
