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
using Lib.ViewModels.Popups;
using MainLib.ViewModels.Utils;
using System.Windows.Input;
using Newtonsoft.Json;

namespace MainLib.ViewModels.Main
{
    public partial class DataViewViewModel : BaseViewModel
    {
        #region Private members
        private List<string> _columns;
        private int _itemsPerPage;
        private int _totalPages;
        private int _currentPage;
        private int _userIndex;
        private Article _selectedArticle;
        private string _selectedSection;
        private bool _isSectionSelected;
        private Action<bool> _workStatus;
        private string _currentSort;
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
                if (LoadArticlesCommand != null && Articles.Count > 0)
                    LoadArticlesCommand.Execute(null);
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
        public ObservableCollection<string> Sections { get; set; }
        public string SelectedSection
        {
            get { return _selectedSection; }
            set { _selectedSection = value; OnPropertyChanged("SelectedSection"); }
        }
        public bool IsSectionSelected
        {
            get { return _isSectionSelected; }
            set { _isSectionSelected = value; OnPropertyChanged("IsSectionSelected"); }
        }

        #region Commands
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand NextPageCommand { get; set; }
        public RelayCommand PreviousPageCommand { get; set; }
        public RelayCommand LoadArticlesCommand { get; set; }
        public ICommand EnableExportCommand { get; set; }
        public RelayCommand ExportCommand { get; set; }
        public RelayCommand DeleteArticleCommand { get; set; }
        public RelayCommand MassBookmarkCommand { get; set; }
        public RelayCommand SortCommand { get; set; }

        public RelayCommand OpenSearchDialogCommand { get; set; }
        public RelayCommand OpenAddPersonalCommand { get; set; }
        public RelayCommand OpenEditCommand { get; set; }
        public RelayCommand OpenBookmarkManagerCommand { get; set; }
        public RelayCommand OpenMassBookmarkManagerCommand { get; set; }
        public RelayCommand OpenReferenceManagerCommand { get; set; }
        #endregion

        // Constructor
        public DataViewViewModel(
            User user, 
            Action<bool> workStatus, 
            IDialogService dialogService, 
            IWindowService windowService, 
            IBrowserService browserService)
        {
            this.User = user;
            this._currentSort = "Title ASC";
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

            if (User.IsAdmin) { Columns.Add("FileName"); }

            // Initialize sections collection
            this.Sections = new ObservableCollection<string>() {};

            // 2. Initialize articles collection and paging
            Users = new ObservableCollection<User>(new UserRepo().GetUsers());
            Articles = new ObservableCollection<Article>();
            CurrentPage = 1;
            ItemsPerPage = 35;

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
            LoadArticlesCommand = new RelayCommand(LoadArticles);
            EnableExportCommand = new RelayCommand(EnableExport);
            ExportCommand = new RelayCommand(Export, CanExport);
            DeleteArticleCommand = new RelayCommand(DeleteArticle, CanDeleteArticle);
            MassBookmarkCommand = new RelayCommand(MassBookmark);
            SortCommand = new RelayCommand(Sort);

            OpenSearchDialogCommand = new RelayCommand(OpenSearchDialog);
            OpenAddPersonalCommand = new RelayCommand(OpenAddPersonal, IsArticleSelected);
            OpenEditCommand = new RelayCommand(OpenEditDialog, IsArticleSelected);
            OpenBookmarkManagerCommand = new RelayCommand(OpenBookmarkManager, IsArticleSelected);
            OpenMassBookmarkManagerCommand = new RelayCommand(OpenMassBookmarkManager, CanOpenMassBookmarkManager);
            OpenReferenceManagerCommand = new RelayCommand(OpenReferenceManager, IsArticleSelected);

            InitializeSearchOptions();

            // Set up section selector
            this.FinishCommand = new RelayCommand(Finish);

            // Populate sections collection from json file
            PopulateSections();
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
        public async void NextPage(object input = null)
        {
            try
            {
                // 1. Check if next page is avaliable
                if (CurrentPage >= TotalPages)
                    return;

                // 2. Increment current page
                CurrentPage++;

                _workStatus(true);

                // 3. Populate article collection
                await PopulateArticles();

                _workStatus(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Next Page", e.Message, e.StackTrace);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                _workStatus(false);
            }
        }
        public async void PreviousPage(object input = null)
        {
            try
            {
                // 1. Check if previous page is avaliable
                if (CurrentPage <= 1)
                    return;

                // 2. Decrement current page
                CurrentPage--;

                _workStatus(true);

                // 3. Populate articles collection
                await PopulateArticles();

                _workStatus(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Previous Page", e.Message, e.StackTrace);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                _workStatus(false);
            }
        }
        public async void LoadArticles(object input = null)
        {
            OnPropertyChanged("FilterTitle");
            try
            {
                // This property is in SearchOptionsViewModel (partial class)
                // causes search options accordion to collapse on UI
                SearchOptionsIsChecked = false;

                string _filterTitle = FilterTitle;

                if (!string.IsNullOrWhiteSpace(_filterTitle))
                    _filterTitle = _filterTitle.Replace("'", "''");

                _workStatus(true);

                List<Article> articles = new List<Article>();
                await Task.Run(() =>
                {
                    // 2. Calculate total pages
                    int record_count = new ArticleRepo().GetRecordCount(
                        Users[UserIndex],
                        _filterTitle,
                        FilterAuthors.ToList(),
                        SelectedAuthorPairing,
                        FilterKeywords.ToList(),
                        SelectedKeywordPairing,
                        FilterYear,
                        FilterPersonalComment,
                        SelectedSection,
                        WordBreakMode,
                        GetFilterIds(IdFilter));
                    if ((record_count % ItemsPerPage) == 0)
                        TotalPages = record_count / ItemsPerPage;
                    else
                        TotalPages = (record_count / ItemsPerPage) + 1;

                    CurrentPage = 1;
                });

                await PopulateArticles();

                if (!string.IsNullOrEmpty(SelectedSection) && SelectedSection != "None")
                    this.IsSectionSelected = true;
                else
                    this.IsSectionSelected = false;

                _workStatus(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Load Articles", e.Message, e.StackTrace);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                _workStatus(false);
            }
        }
        public void EnableExport(object input = null)
        {
            foreach (Article article in Articles)
                article.Checked = false;
        }
        public async void Export(object input = null)
        {
            try
            {
                // Destination will be the path chosen from dialog box (Where files should be exported)
                string destination = null;

                destination = _browserService.OpenFolderDialog();

                // If path was chosen from the dialog box
                if (destination != null)
                {
                    _workStatus(true);

                    await Task.Run(() =>
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
                    });

                    // 3. Uncheck articles
                    foreach (Article article in Articles)
                        article.Checked = false;

                    _workStatus(false);

                    _dialogService.OpenDialog(new DialogOkViewModel("Done", "Message", DialogType.Success));
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Export", e.Message, e.StackTrace);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                _workStatus(false);
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
            try
            {
                // 1. Ask user if they are sure
                if (_dialogService.OpenDialog(
                    new DialogYesNoViewModel("Delete following record?\n" + SelectedArticle.Title, "Warning", DialogType.Question)
                   ))
                {
                    // 2. Delete article record from database
                    new ArticleRepo().DeleteArticle(SelectedArticle);

                    // 2.2 Track article delete
                    new Tracker(User).TrackDelete("Article", SelectedArticle.Title);

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
            catch(Exception e)
            {
                new BugTracker().Track("Data View", "Delete Article", e.Message, e.StackTrace);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
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
        public async void Sort(object input)
        {
            string header = input.ToString();
            header = header == "Comment" ? "PersonalComment" : header;

            // Ignore columns
            if (header == "Authors" || header == "Keywords" || header == "Export" || header == "Bookmark")
                return;


            if (_currentSort.Contains(header))
            {
                if (_currentSort.Contains("ASC"))
                    _currentSort = $"{header} DESC";
                else
                    _currentSort = $"{header} ASC";
            }
            else
            {
                _currentSort = $"{header} ASC";
            }

            _workStatus(true);

            await PopulateArticles();

            _workStatus(false);
        }

        // Window open commands
        public void OpenSearchDialog(object input = null)
        {
            _windowService.OpenWindow(new SearchDialogViewModel(this), passWindow: true);
        }
        public void OpenAddPersonal(object input)
        {
            _windowService.OpenWindow(new MainLib.ViewModels.Popups.AddPersonalDialogViewModel(SelectedArticle, User, _dialogService));
        }
        public void OpenBookmarkManager(object input = null)
        {
            _windowService.OpenWindow(new BookmarkManagerViewModel(User, ViewType.DataView, _dialogService,input as Article));
        }
        public void OpenEditDialog(object input = null)
        {
            _windowService.OpenWindow(new MainLib.ViewModels.Popups.ArticleEditorViewModel(SelectedArticle, User, _dialogService, _browserService));
        }
        public void OpenMassBookmarkManager(object input = null)
        {
            _windowService.OpenWindow(new MassBookmarkManagerViewModel(User, _workStatus, BMCheckedArticles, _dialogService));
        }
        public void OpenReferenceManager(object input)
        {
            _windowService.OpenWindow(new ReferenceManagerViewModel(ViewType.DataView, _dialogService, input as Article));
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

        #region Section selector setup
        public ICommand FinishCommand { get; set; }

        // [Obsolete]
        public void Select(string selectedItem)
        {
            // 1. Set selected section
            this.SelectedSection = selectedItem;

            // 2. Fetch section articles from database

            // 3. Refresh articles collection
        }

        public async void Finish(object input = null)
        {
            if (
                _dialogService.OpenDialog(new DialogYesNoViewModel("Are you sure you want to remove this section from pending?", "Finish Section", DialogType.Warning)))
            {
                try
                {
                    _workStatus(true);

                    await Task.Run(() =>
                    {
                        // 1. Remove pending status from selected section articles
                        new GlobalRepo().RemovePending(SelectedSection);

                        // Track Pending
                        new Tracker(this.User).TrackPending(new Lib.DataAccessLayer.Info.PendingInfo(SelectedSection));

                        // 2. Remove section from json
                        string path = Path.Combine(Environment.CurrentDirectory, "sections.json");
                        string info = File.ReadAllText(path);
                        List<string> sections = JsonConvert.DeserializeObject<List<string>>(info);
                        sections.Remove(SelectedSection);
                        info = JsonConvert.SerializeObject(sections);
                        File.WriteAllText(path, info);
                    });

                    _workStatus(false);

                    // 3. Clear articles collection
                    this.Sections.Remove(SelectedSection);
                    this.SelectedSection = this.Sections.First();
                    this.Articles.Clear();
                    this.IsSectionSelected = false;
                }
                catch (Exception e)
                {
                    new BugTracker().Track("DataView", "Finish", e.Message, e.StackTrace);
                    _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
                }
                finally
                {
                    _workStatus(false);
                }
            }
        }
        #endregion

        // Private helpers
        private async Task PopulateArticles()
        {
            // 1. Clear existing data grid source
            Articles.Clear();

            List<Article> articles = new List<Article>();

            await Task.Run(() =>
            {
                string _filterTitle = FilterTitle;

                if (!string.IsNullOrWhiteSpace(_filterTitle))
                    _filterTitle = _filterTitle.Replace("'", "''");

                // 2. Fetch artilces from database
                foreach (Article article in new ArticleRepo().LoadArticles(
                    Users[UserIndex],
                    _filterTitle,
                    FilterAuthors.ToList(),
                    SelectedAuthorPairing,
                    FilterKeywords.ToList(),
                    SelectedKeywordPairing,
                    FilterYear,
                    FilterPersonalComment,
                    _offset,
                    ItemsPerPage,
                    SelectedSection,
                    WordBreakMode,
                    GetFilterIds(IdFilter),
                    _currentSort))
                {
                    articles.Add(article);
                }
            });

            // 3. Populate article collection
            foreach (Article article in articles)
                this.Articles.Add(article);
        }
        private async Task PopulateSections()
        {
            this.Sections.Clear();

            try
            {
                List<string> sections = new List<string>();

                _workStatus(true);
                
                await Task.Run(() =>
                {
                    string info = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "sections.json"));

                    JsonConvert.DeserializeObject<List<string>>(info).ForEach((cur) =>
                    {
                        sections.Add(cur);
                    });
                });

                _workStatus(false);

                this.Sections.Add("None");
                sections.ForEach((cur) =>
                {
                    this.Sections.Add(cur);
                });
                this.SelectedSection = this.Sections.First();
            }
            catch(Exception e)
            {
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong", "Error", DialogType.Error));
                new BugTracker().Track("Data View", "Reading sections from json", e.Message, e.StackTrace);
            }
            finally
            {
                _workStatus(false);
            }
        }
        private int[] GetFilterIds(string filterId)
        {
            if (String.IsNullOrEmpty(filterId)) return null;

            int[] idFilters = null;

            if (filterId.Contains('-'))
            {
                string[] split = filterId.Split('-');
                if (split.Length == 2 && int.TryParse(split[0], out _) && int.TryParse(split[1], out _))
                {
                    idFilters = new int[2] { int.Parse(split[0]), int.Parse(split[1]) };
                }
            }

            return idFilters;
        }
    }
}
