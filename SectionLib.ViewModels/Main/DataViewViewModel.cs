using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Popups;
using Lib.ViewModels.Services.Browser;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;
using SectionLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectionLib.ViewModels.Main
{
    public class DataViewViewModel : BaseViewModel
    {
        #region Private members
        private List<string> _columns;
        private string _filterTitle;
        private Action<bool> _workStatus;
        private IDialogService _dialogService;
        private IWindowService _windowService;
        private IBrowserService _browserService;
        #endregion

        public User User { get; set; }

        // Selected article in data grid
        public Article SelectedArticle { get; set; }
        // Which columns to show on datagrid (bound to CheckBox.IsChecked and converted with value converter)
        public List<string> Columns
        {
            get { return _columns; }
            set { _columns = value; OnPropertyChanged("Columns"); }
        }
        // Store fetched articles to show on data grid
        public ObservableCollection<Article> Articles { get; set; }
        public string FilterTitle
        {
            get { return _filterTitle; }
            set { _filterTitle = value; OnPropertyChanged("FilterTitle"); }
        }


        #region Commands
        public RelayCommand LoadArticlesCommand { get; set; }
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand DeleteArticleCommand { get; set; }

        public RelayCommand OpenAddPersonalCommand { get; set; }
        public RelayCommand OpenEditCommand { get; set; }
        #endregion

        // Constructor
        public DataViewViewModel(User user, Action<bool> workStatus, IDialogService dialogService, IWindowService windowService, IBrowserService browserService)
        {
            this.User = user;
            this._workStatus = workStatus;
            this._dialogService = dialogService;
            this._windowService = windowService;
            this._browserService = browserService;

            // 1. Set neccessary columns to show on datagrid
            Columns = new List<string>()
            {
                "Authors",
                "Keywords",
                "Year"
            };

            // 2. Initialize articles collection and paging
            Articles = new ObservableCollection<Article>();

            // 3. Set up commands
            LoadArticlesCommand = new RelayCommand(LoadArticles);
            OpenFileCommand = new RelayCommand(OpenFile);
            DeleteArticleCommand = new RelayCommand(DeleteArticle);

            OpenAddPersonalCommand = new RelayCommand(OpenAddPersonal, IsArticleSelected);
            OpenEditCommand = new RelayCommand(OpenEditDialog, IsArticleSelected);
        }

        #region Command Actions
        public async void LoadArticles(object input = null)
        {
            // 1. Clear existing data grid source
            Articles.Clear();

            // 2. Fetch artilces from database
            List<Article> articles = new List<Article>();
            _workStatus(true);

            await Task.Run(() =>
            {
                new ArticleRepo().GetAllArticles(User, FilterTitle).ForEach((article) => 
                {
                    articles.Add(article);
                });
            });

            _workStatus(false);

            // 3. Populate collection
            foreach (Article article in articles)
                Articles.Add(article);
        }
        public void OpenFile(object input = null)
        {
            // 1. If no item was selected return
            if (SelectedArticle == null)
                return;

            // 2. Get full path of the file
            string full_path = Program.GetSectionFilesPath() + SelectedArticle.FileName + ".pdf";

            // 3. Open file with default program
            try
            {
                Process.Start(full_path);
            }

            // 4. Catch if file doesn't exist physically
            catch
            {
                Console.WriteLine("File was not found");
            }
        }
        public void DeleteArticle(object input = null)
        {
            // 1. Ask user if they are sure
            if (_dialogService.OpenDialog(new DialogYesNoViewModel(
                "Delete following record?\n" + SelectedArticle.Title,
                "Delete article",
                DialogType.Warning
                )))
            {
                // 2. Delete article record from database
                new ArticleRepo().DeleteArticle(SelectedArticle);

                // 3. Delete physical .pdf file
                try
                {
                    File.Delete(Path.Combine(Environment.CurrentDirectory, "Files\\") + SelectedArticle.FileName + ".pdf");
                }

                catch
                {
                    _dialogService.OpenDialog(
                        new DialogOkViewModel("The file is missing, validate your database.", "Error", DialogType.Error));
                }

                // 4. Refresh the data grid
                LoadArticlesCommand.Execute(null);
            }
        }

        public void OpenAddPersonal(object input)
        {
           _windowService.OpenWindow(new AddPersonalDialogViewModel(SelectedArticle, User));
        }
        public void OpenEditDialog(object input = null)
        {
           _windowService.OpenWindow(new ArticleEditorViewModel(SelectedArticle, User, _dialogService, _browserService));
        }
        public bool IsArticleSelected(object input = null)
        {
            if (this == null)
                return false;

            if (SelectedArticle != null)
                return true;

            return false;
        }
        #endregion
    }
}
