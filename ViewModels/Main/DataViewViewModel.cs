using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Popups;
using Lib.ViewModels.Services.Browser;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;

namespace MainLib.ViewModels.Main
{
    public class DataViewViewModel : BaseViewModel
    {
        #region Private members
        private List<string> _columns;
        private int _itemsPerPage;
        private int _totalPages;
        private int _currentPage;
        private int _userIndex;
        private Article _selectedArticle;
        private IDialogService _dialogService;
        private IWindowService _windowService;
        private IBrowserService _browserService;

        private int _offset { get { return (CurrentPage - 1) * ItemsPerPage; } }
        #endregion

        public User User { get; set; }
        // Selected user in combobox (To see Comments/SIC)
        public int UserIndex
        {
            get { return _userIndex; }
            set
            {
                _userIndex = value;

                // Change comments and sic on data grid
                if (RefreshCommand != null && Articles.Count > 0)
                    RefreshCommand.Execute(null);
            }
        }
        // User should only allowed to add comment/SIC if they are browsing their own comments and SIC
        public bool CanAddPersonal { get { return Users[UserIndex].Username == User.Username; } }
        // Selected article in data grid
        public Article SelectedArticle
        {
            get { return _selectedArticle; }
            set { _selectedArticle = value; OnPropertyChanged("SelectedArticle"); }
        }
        // Which columns to show on datagrid (bound to CheckBox.IsChecked and converted with value converter)
        public List<string> Columns
        {
            get { return _columns; }
            set { _columns = value; OnPropertyChanged("Columns"); }
        }
        // Store fetched articles to show on data grid
        public ObservableCollection<Article> Articles { get; set; }
        public List<Article> BMCheckedArticles
        {
            get { return Articles.Where(cur => cur.BMChecked == true).ToList(); }
        }
        // All existing users for combo box (Comment/SIC browsing)
        public ObservableCollection<User> Users { get; set; }
        public int ItemsPerPage
        {
            get { return _itemsPerPage; }
            // Prevent setting 0
            set
            {
                if (value == 0)
                    _itemsPerPage = 1;
                else
                    _itemsPerPage = value;

                OnPropertyChanged("ItemsPerPage");
            }
        }
        public int CurrentPage
        {
            get { return _currentPage; }
            set { _currentPage = value; OnPropertyChanged("CurrentPage"); }
        }
        public int TotalPages
        {
            get
            {
                if (_totalPages == 0)
                    return 1;

                return _totalPages;
            }
            set { _totalPages = value; OnPropertyChanged("TotalPages"); }
        }

        #region Commands
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand NextPageCommand { get; set; }
        public RelayCommand PreviousPageCommand { get; set; }
        public RelayCommand RefreshCommand { get; set; }
        public RelayCommand ExportCommand { get; set; }
        public RelayCommand DeleteArticleCommand { get; set; }
        public RelayCommand MassBookmarkCommand { get; set; }

        public RelayCommand OpenSearchDialogCommand { get; set; }
        public RelayCommand OpenAddPersonalCommand { get; set; }
        public RelayCommand OpenEditCommand { get; set; }
        public RelayCommand OpenBookmarkManagerCommand { get; set; }
        public RelayCommand OpenMassBookmarkManagerCommand { get; set; }
        public RelayCommand OpenReferenceManagerCommand { get; set; }
        #endregion

        // Constructor
        public DataViewViewModel(User user, IDialogService dialogService, IWindowService windowService, IBrowserService browserService)
        {
            User = user;
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
            Users = new ObservableCollection<User>((new UserRepo()).GetUsers());
            Articles = new ObservableCollection<Article>();
            CurrentPage = 1;
            ItemsPerPage = 25;

            // 3. Get the index of the logged in user and set it as selected index for combobox
            int index = 0;
            foreach (User userI in Users)
            {
                if (userI.ID == User.ID)
                    break;
                index++;
            }

            UserIndex = index;

            // 4. Set up commands
            OpenFileCommand = new RelayCommand(OpenFile);
            NextPageCommand = new RelayCommand(NextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage);
            RefreshCommand = new RelayCommand(Refresh);
            ExportCommand = new RelayCommand(Export, CanExport);
            DeleteArticleCommand = new RelayCommand(DeleteArticle, CanDeleteArticle);
            MassBookmarkCommand = new RelayCommand(MassBookmark);

            OpenSearchDialogCommand = new RelayCommand(OpenSearchDialog);
            OpenAddPersonalCommand = new RelayCommand(OpenAddPersonal, IsArticleSelected);
            OpenEditCommand = new RelayCommand(OpenEditDialog, IsArticleSelected);
            OpenBookmarkManagerCommand = new RelayCommand(OpenBookmarkManager, IsArticleSelected);
            OpenMassBookmarkManagerCommand = new RelayCommand(OpenMassBookmarkManager, CanOpenMassBookmarkManager);
            OpenReferenceManagerCommand = new RelayCommand(OpenReferenceManager, IsArticleSelected);

            // Set up filter dialog window

            // 1. Initialize collections and fields
            FilterAuthors = new ObservableCollection<string>();
            FilterKeywords = new ObservableCollection<string>();

            // 2. Set up commands
            LoadArticlesCommand = new RelayCommand(LoadArticles);
            ClearCommand = new RelayCommand(Clear, CanClear);
        }

        #region Command Actions
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
        public void NextPage(object input = null)
        {
            // 1. Check if next page is avaliable
            if (CurrentPage >= TotalPages)
                return;

            // 2. Increment current page
            CurrentPage++;

            // 3. Clear existing data grid source
            Articles.Clear();

            // 4. Fetch artilces from database
            foreach (Article article in (new ArticleRepo()).LoadArticles(Users[UserIndex], FilterTitle, FilterAuthors.ToList(), FilterKeywords.ToList(), _offset, ItemsPerPage))
            {
                Articles.Add(article);
            }
        }
        public void PreviousPage(object input = null)
        {
            // 1. Check if previous page is avaliable
            if (CurrentPage <= 1)
                return;

            // 2. Decrement current page
            CurrentPage--;

            // 3. Clear existing data grid source
            Articles.Clear();

            // 4. Fetch artilces from database
            foreach (Article article in (new ArticleRepo()).LoadArticles(Users[UserIndex], FilterTitle, FilterAuthors.ToList(), FilterKeywords.ToList(), _offset, ItemsPerPage))
            {
                Articles.Add(article);
            }
        }
        public void Refresh(object input = null)
        {

            // 1. Calculate total pages
            int record_count = (new ArticleRepo()).GetRecordCount(Users[UserIndex], FilterTitle, FilterAuthors.ToList(), FilterKeywords.ToList());
            if ((record_count % ItemsPerPage) == 0)
                TotalPages = record_count / ItemsPerPage;
            else
                TotalPages = (record_count / ItemsPerPage) + 1;

            CurrentPage = 1;

            // 2. Clear existing data grid source
            Articles.Clear();

            // 3. Fetch artilces from database
            foreach (Article article in (new ArticleRepo()).LoadArticles(Users[UserIndex], FilterTitle, FilterAuthors.ToList(), FilterKeywords.ToList(), _offset, ItemsPerPage))
            {
                Articles.Add(article);
            }
        }
        public void Export(object input = null)
        {
            // Destination will be the path chosen from dialog box (Where files should be exported)
            string destination = null;

            destination = _browserService.OpenFolderDialog();

            // If path was chosen from the dialog box
            if (destination != null)
            {
                // 2. Get the list of articles which were checked for export
                List<Article> checked_articles = Articles.Where(article => article.Checked == true).ToList();

                foreach (Article article in checked_articles)
                {
                    if (!string.IsNullOrEmpty(article.FileName))
                    {
                        // If title is too long just get the substring to name the .pdf file
                        if (article.Title.Length > 40)
                        {
                            string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars());
                            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                            string validName = r.Replace(article.Title.Substring(0, 40), "");
                            File.Copy(Path.Combine(Environment.CurrentDirectory, "Files\\") + article.FileName + ".pdf", destination + "\\" + validName + "(" + article.FileName + ")" + ".pdf", true);
                        }
                        else
                        {
                            string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars());
                            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                            string validName = r.Replace(article.Title, "");
                            File.Copy(Path.Combine(Environment.CurrentDirectory, "Files\\") + article.FileName + ".pdf", destination + "\\" + validName + "(" + article.FileName + ")" + ".pdf", true);
                        }
                    }
                }

                foreach (Article article in Articles)
                    article.Checked = false;

                _dialogService.OpenDialog(new DialogOkViewModel("Done", "Message", DialogType.Success));
            }
        }
        public bool CanExport(object input = null)
        {
            List<Article> checked_articles = Articles.Where(article => article.Checked == true).ToList();
            if (checked_articles.Count > 0)
                return true;

            return false;
        }
        public void DeleteArticle(object input = null)
        {
            // 1. Ask user if they are sure
            if (_dialogService.OpenDialog(
                new DialogYesNoViewModel("Delete following record?\n" + SelectedArticle.Title, "Warning", DialogType.Question)
               ))
            {
                // 2. Delete article record from database
                (new ArticleRepo()).DeleteArticle(SelectedArticle);

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
                RefreshCommand.Execute(null);
            }
        }
        public bool CanDeleteArticle(object input = null)
        {
            if (SelectedArticle != null)
                return true;

            return false;
        }
        public void MassBookmark(object input)
        {
            foreach (Article article in Articles)
                article.BMChecked = (bool)input;
        }


        public void OpenSearchDialog(object input = null)
        {
            _windowService.OpenWindow(new SearchDialogViewModel(this));
        }
        public void OpenAddPersonal(object input)
        {
            _windowService.OpenWindow(new AddPersonalDialogViewModel(this));
        }
        public void OpenBookmarkManager(object input = null)
        {
            _windowService.OpenWindow(new BookmarkManagerViewModel(this, _dialogService,input as Article));
        }
        public void OpenEditDialog(object input = null)
        {
            _windowService.OpenWindow(new ArticleEditorViewModel(this, _dialogService));
        }
        public void OpenMassBookmarkManager(object input = null)
        {
            _windowService.OpenWindow(new MassBookmarkManagerViewModel(User, BMCheckedArticles, _dialogService));
        }
        public void OpenReferenceManager(object input)
        {
            _windowService.OpenWindow(new ReferenceManagerViewModel(input as Article, _dialogService));
        }

        public bool IsArticleSelected(object input = null)
        {
            if (this == null)
                return false;

            if (SelectedArticle != null)
                return true;

            return false;
        }
        public bool CanOpenMassBookmarkManager(object input = null)
        {
            if (this == null) return false;

            return BMCheckedArticles.Count > 0;
        }
        #endregion

        #region Filter dialog setup

        // We bind dialog datacontext to this (DataViewViewModel) because copying columns list with reference didn't work on UI)
        // NOT ANYMORE, we just pass this datacontext as parent

        private string _filterTitle;
        private string _filterAuthor;
        private string _filterKeyword;

        public string FilterTitle
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_filterTitle))
                    return null;

                return _filterTitle;
            }
            set { _filterTitle = value; OnPropertyChanged("FilterTitle"); }
        }
        public string FilterAuthor
        {
            get { return _filterAuthor; }
            set { _filterAuthor = value; OnPropertyChanged("FilterAuthor"); }
        }
        public string FilterKeyword
        {
            get { return _filterKeyword; }
            set { _filterKeyword = value; OnPropertyChanged("FilterKeyword"); }
        }
        public ObservableCollection<string> FilterAuthors { get; set; }
        public ObservableCollection<string> FilterKeywords { get; set; }

        public RelayCommand LoadArticlesCommand { get; set; }
        public RelayCommand ClearCommand { get; set; }

        public void LoadArticles(object input = null)
        {
            // 1. Calculate total pages
            int record_count = (new ArticleRepo()).GetRecordCount(Users[UserIndex], FilterTitle, FilterAuthors.ToList(), FilterKeywords.ToList());
            if ((record_count % ItemsPerPage) == 0)
                TotalPages = record_count / ItemsPerPage;
            else
                TotalPages = (record_count / ItemsPerPage) + 1;

            CurrentPage = 1;

            // 2. Clear existing data grid source
            Articles.Clear();

            // 3. Fetch artilces from database
            foreach (Article article in (new ArticleRepo()).LoadArticles(Users[UserIndex], FilterTitle, FilterAuthors.ToList(), FilterKeywords.ToList(), _offset, ItemsPerPage))
            {
                Articles.Add(article);
            }
        }
        public void Clear(object input = null)
        {
            FilterTitle = null;
            FilterAuthors.Clear();
            FilterKeywords.Clear();
            Articles.Clear();
        }
        public bool CanClear(object input = null)
        {
            if (FilterTitle == null && FilterAuthors.Count == 0 && FilterKeywords.Count == 0)
                return false;

            return true;
        }

        #endregion
    }
}
