using Lib.DataAccessLayer.Info;
using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Popups;
using MainLib.ViewModels.UIStructs;
using MainLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainLib.ViewModels.Main
{
    public class DataEntryViewModel : BaseViewModel
    {
        // Private memebers
        private readonly Shared services;

        // Public properties
        public User User { get; set; }
        public List<Bookmark> Bookmarks { get; set; }
        public List<Reference> References { get; set; }
        public ArticleForm ArticleForm { get; set; }

        // Commands
        public ICommand SelectFileCommand { get; set; }
        public ICommand SaveArticleCommand { get; set; }
        public ICommand ClearArticleAttributesCommand { get; set; }
        public ICommand ClearTitleCommand { get; set; }
        public ICommand OpenBookmarkManagerCommand { get; set; }
        public ICommand OpenReferenceManagerCommand { get; set; }

        // Constructor
        public DataEntryViewModel()
        {
            // 1. Initialize article and User
            this.services = Shared.GetInstance();
            this.User = services.User;
            this.ArticleForm = new ArticleForm();
            this.Bookmarks = new List<Bookmark>();
            this.References = new List<Reference>();

            // 2. Initialize commands
            SelectFileCommand = new RelayCommand(SelectFile);
            SaveArticleCommand = new RelayCommand(SaveArticle, CanSaveArticle);
            ClearArticleAttributesCommand = new RelayCommand(ClearArticleAttributes);
            ClearTitleCommand = new RelayCommand(ClearTitle);
            OpenBookmarkManagerCommand = new RelayCommand(OpenBookmarkManager);
            OpenReferenceManagerCommand = new RelayCommand(OpenReferenceManager);            
        }

        // Command actions
        public void SelectFile(object input = null)
        {
            try
            {
                string result = services.BrowserService.OpenFileDialog(".pdf", "PDF files (*.pdf)|*.pdf");

                // Get the selected file
                ArticleForm.FilePath = result;
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data Entry", "Select file", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
        }
        public async void SaveArticle(object input = null)
        {
            try
            {
                services.IsWorking(true);

                await Task.Run(() =>
                {
                    ArticleRepo articleRepo = new ArticleRepo();

                    // Create article from form
                    Article article = new Article()
                    {
                        Title = ArticleForm.Title,
                        Year = ArticleForm.Year,
                        PersonalComment = ArticleForm.PersonalComment,
                        SIC = ArticleForm.SIC ? 1 : 0,
                        AbstractOnly = ArticleForm.FileContainsOnlyAbstract ? 1 : 0
                    };
                    // Copy authors and keywords
                    foreach (string author in ArticleForm.Authors)
                    {
                        article.AuthorsCollection.Add(author);
                    }
                    foreach (string keyword in ArticleForm.Keywords)
                    {
                        article.KeywordsCollection.Add(keyword);
                    }

                    // Save base info to database
                    articleRepo.SaveArticle(article, User);

                    // 3. Copy selected file to root folder with the new ID-based name
                    File.Copy(ArticleForm.FilePath, Path.Combine(Environment.CurrentDirectory, "Files\\") + article.FileName + ".pdf");

                    // 4.1 Retrieve id (in reality we retrieve whole article) of newly added article
                    Article currently_added_article = articleRepo.GetArticleWithTitle(article.Title);

                    // 4.2 Add bookmarks
                    foreach (Bookmark bookmark in Bookmarks)
                        new BookmarkRepo().AddArticleToBookmark(bookmark, currently_added_article);

                    // 4.3 Add references
                    foreach (Reference reference in References)
                        new ReferenceRepo().AddArticleToReference(reference, currently_added_article);

                    // 4.4 Add abstract
                    new AbstractRepo().AddAbstract((int)currently_added_article.ID, ArticleForm.Abstract);

                    // 5. Tracking
                    ArticleInfo info = new ArticleInfo(User, article.Title, Bookmarks, References);
                    Tracker tracker = new Tracker(User);
                    tracker.TrackCreate<ArticleInfo>(info);
                    File.Copy(ArticleForm.FilePath, tracker.GetFilesPath() + "\\" + article.FileName + ".pdf");
                    tracker.TrackCreate(new AbstractInfo() { ArticleTitle = article.Title, AbstractBody = ArticleForm.Abstract });

                    // 6. Move the selected file into "Done" subfolder
                    string done_path = Path.GetDirectoryName(ArticleForm.FilePath) + "\\Done\\";
                    Directory.CreateDirectory(Path.GetDirectoryName(ArticleForm.FilePath) + "\\Done");
                    File.Move(ArticleForm.FilePath, done_path + System.IO.Path.GetFileName(ArticleForm.FilePath));
                });

                // 5. Clear article attributes
                ClearArticleAttributesCommand.Execute(null);
                Bookmarks.Clear();
                References.Clear();

                services.IsWorking(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data Entry", "Add article", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
            }
        }
        public void ClearArticleAttributes(object input = null)
        {
            try
            {
                ArticleForm.ClearForm();
                Bookmarks.Clear();
                References.Clear();
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data Entry", "Clear Article Attributes", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
        }
        public void ClearTitle(object input = null)
        {
            ArticleForm.Title = null;
        }
        public void OpenBookmarkManager(object input = null)
        {
            services.WindowService.OpenWindow(new BookmarkManagerViewModel(ViewType.DataEntry, bookmarks: Bookmarks));
        }
        public void OpenReferenceManager(object input = null)
        {
            services.WindowService.OpenWindow(new ReferenceManagerViewModel(ViewType.DataEntry, references: References));
        }
        public bool CanSaveArticle(object input = null)
        {
            return ArticleForm.IsArticleValid();
        }

    }
}
