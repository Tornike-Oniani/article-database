using Lib.DataAccessLayer.Info;
using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Browser;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.UIStructs;
using MainLib.ViewModels.Utils;
using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Input;

namespace MainLib.ViewModels.Popups
{
    public class ArticleEditorViewModel : BaseViewModel
    {
        // Property fields
        private string _selectedFile;

        // Private members
        private readonly ICommand loadArticlesCommand;
        private readonly Shared services;

        // Public properties
        public Article SelectedArticle { get; set; }
        public User User { get; set; }
        public Article Article { get; set; }
        public ArticleForm ArticleForm { get; set; }
        public string SelectedFile
        {
            get { return _selectedFile; }
            set { _selectedFile = value; OnPropertyChanged("SelectedFile"); }
        }

        // Commands
        public RelayCommand SelectFileCommand { get; set; }
        public RelayCommand UpdateArticleCommand { get; set; }

        // Constructor
        public ArticleEditorViewModel(Article selectedArticle, ICommand loadArticlesCommand)
        {
            this.services = Shared.GetInstance();
            this.Title = "Edit Article";
            this.loadArticlesCommand = loadArticlesCommand;

            // 1. Set parent view model
            this.SelectedArticle = selectedArticle;
            this.User = services.User;

            // 2. Create article instance
            Article = new Article();
            this.ArticleForm = new ArticleForm();

            // 3. Copy article properties from parent's selected article
            Article.CopyByValue(SelectedArticle, true, false);
            this.ArticleForm.Title = selectedArticle.Title;
            this.ArticleForm.Year = selectedArticle.Year.ToString();
            this.ArticleForm.PersonalComment = selectedArticle.PersonalComment;
            this.ArticleForm.SIC = selectedArticle.SIC == 1;
            this.ArticleForm.FileContainsOnlyAbstract = selectedArticle.AbstractOnly == 1;
            foreach (string author in Article.AuthorsCollection)
            {
                this.ArticleForm.Authors.Add(author);
            }
            foreach (string keyword in Article.KeywordsCollection)
            {
                this.ArticleForm.Keywords.Add(keyword);
            }
            

            // 4. Initialize commands
            SelectFileCommand = new RelayCommand(SelectFile);
            UpdateArticleCommand = new RelayCommand(UpdateArticle, CanUpdateArticle);
        }

        public void SelectFile(object input = null)
        {
            string result = services.BrowserService.OpenFileDialog(".pdf", "PDF files (*.pdf)|*.pdf");

            // Get the selected file
            SelectedFile = result;
        }
        public void UpdateArticle(object input)
        {
            try
            {
                ArticleRepo articleRepo = new ArticleRepo();

                // 0. Retrieve old article id so we can track which article was updated
                string oldName = articleRepo.GetArticleTitleWithId((int)Article.ID);
                // 1. Update article record in database
                int? intYear = null;
                if (!String.IsNullOrEmpty(ArticleForm.Year))
                {
                    intYear = int.Parse(ArticleForm.Year);
                }
                Article.Title = ArticleForm.Title.Trim();
                Article.Year = intYear;
                Article.SIC = ArticleForm.SIC ? 1 : 0;
                Article.PersonalComment = String.IsNullOrEmpty(ArticleForm.PersonalComment) ? null : ArticleForm.PersonalComment.Trim();
                Article.AbstractOnly = ArticleForm.FileContainsOnlyAbstract ? 1 : 0;
                Article.AuthorsCollection.Clear();
                foreach (string author in ArticleForm.Authors)
                {
                    Article.AuthorsCollection.Add(author);
                }
                Article.KeywordsCollection.Clear();
                foreach (string keyword in ArticleForm.Keywords)
                {
                    Article.KeywordsCollection.Add(keyword);
                }
                articleRepo.UpdateArticle(Article, User);

                // 1.1 Track article update
                ArticleInfo info = new ArticleInfo(User, Article.Title);
                Tracker tracker = new Tracker(User);

                // 2. If new file was selected overwrite it to older one
                if (SelectedFile != null)
                {
                    File.Copy(SelectedFile, Path.Combine(Environment.CurrentDirectory, "Files\\") + Article.FileName + ".pdf", true);
                    // If file was changed track it also
                    string changedFileName = Article.FileName + "_new" + ".pdf";
                    info.SetChangedFile(changedFileName);
                    File.Copy(SelectedFile, tracker.GetFilesPath() + "\\" + changedFileName);
                }

                tracker.TrackUpdate<ArticleInfo>(info, oldName);

                // 3. Copy new article properties to parent's selected article (so that the values will be updated on data grid)
                //SelectedArticle.CopyByValue(Article, false, true);
                OnPropertyChanged("SelectedArticle");
            }

            catch (Exception e)
            {
                // Message = "constraint failed\r\nUNIQUE constraint failed: tblArticle.Title"
                if (e.Message.Contains("UNIQUE") && e.Message.Contains("Title"))
                {
                    services.ShowDialogWithOverlay(new DialogOkViewModel("Article with that name already exists", "Duplicate", DialogType.Warning));

                }
                else
                {
                    new BugTracker().Track("Data View (sub window)", "Update Article", e.Message, e.StackTrace);
                    services.ShowDialogWithOverlay(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
                }
            }

            loadArticlesCommand.Execute(null);
            // Close window
            (input as ICommand).Execute(null);
        }
        public bool CanUpdateArticle(object input = null)
        {
            if (this.ArticleForm.ErrorCollection == null)
            {
                return true;
            }

            if (this.ArticleForm.ErrorCollection.ContainsKey("Title") && this.ArticleForm.ErrorCollection["Title"] != null)
            {
                return false;
            }

            if (this.ArticleForm.ErrorCollection.ContainsKey("Year") && this.ArticleForm.ErrorCollection["Year"] != null)
            {
                return false;
            }

            return true;
        }
    }
}
