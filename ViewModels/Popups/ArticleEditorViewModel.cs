﻿using Lib.DataAccessLayer.Info;
using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Browser;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MainLib.ViewModels.Popups
{
    public class ArticleEditorViewModel : BaseViewModel
    {
        private string _author;
        private string _keyword;
        private string _selectedFile;
        private IDialogService _dialogService;
        private IBrowserService _browserService;

        public Article SelectedArticle { get; set; }
        public User User { get; set; }
        public Article Article { get; set; }
        public string Author
        {
            get { return _author; }
            set { _author = value; OnPropertyChanged("Author"); }
        }
        public string Keyword
        {
            get { return _keyword; }
            set { _keyword = value; OnPropertyChanged("Keyword"); }
        }
        public string SelectedFile
        {
            get { return _selectedFile; }
            set { _selectedFile = value; OnPropertyChanged("SelectedFile"); }
        }

        public RelayCommand SelectFileCommand { get; set; }
        public RelayCommand UpdateArticleCommand { get; set; }

        public ArticleEditorViewModel(Article selectedArticle, User user, IDialogService dialogService, IBrowserService browserService)
        {
            this.Title = "Edit Article";
            this._dialogService = dialogService;
            this._browserService = browserService;

            // 1. Set parent view model
            this.SelectedArticle = selectedArticle;
            this.User = user;

            // 2. Create article instance
            Article = new Article();

            // 3. Copy article properties from parent's selected article
            Article.CopyByValue(SelectedArticle, true, false);

            // 4. Initialize commands
            SelectFileCommand = new RelayCommand(SelectFile);
            UpdateArticleCommand = new RelayCommand(UpdateArticle);
        }

        public void SelectFile(object input = null)
        {
            string result = _browserService.OpenFileDialog(".pdf", "PDF files (*.pdf)|*.pdf");

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
                articleRepo.UpdateArticle(Article, User);

                // 1.1 Track article update
                ArticleInfo info = new ArticleInfo(User, Article.Title);
                new Tracker(User).TrackUpdate<ArticleInfo>(info, oldName);

                // 2. If new file was selected overwrite it to older one
                if (SelectedFile != null)
                {
                    File.Copy(SelectedFile, Path.Combine(Environment.CurrentDirectory, "Files\\") + Article.FileName + ".pdf", true);
                }

                // 3. Copy new article properties to parent's selected article (so that the values will be updated on data grid)
                SelectedArticle.CopyByValue(Article, false, true);
                OnPropertyChanged("SelectedArticle");
            }

            catch (Exception e)
            {
                // Message = "constraint failed\r\nUNIQUE constraint failed: tblArticle.Title"
                if (e.Message.Contains("UNIQUE") && e.Message.Contains("Title"))
                {
                    _dialogService.OpenDialog(new DialogOkViewModel("Article with that name already exists", "Duplicate", DialogType.Warning));

                }
                else
                {
                    new BugTracker().Track("Data View (sub window)", "Update Article", e.Message, e.StackTrace);
                    _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
                }
            }

            // Close window
            (input as ICommand).Execute(null);
        }
    }
}
