using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.DataAccessLayer.Utils;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Popups;
using MainLib.ViewModels.Utils;
using Newtonsoft.Json;
using NotificationService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SimMetricsUtilities;
using SimMetricsMetricUtilities;

namespace MainLib.ViewModels.Main
{
    public partial class DataViewViewModel : BaseViewModel
    {
        // Public property fields
        private List<string> _columns;
        private int _itemsPerPage;
        private int _totalPages;
        private int _currentPage;
        private int _userIndex;
        private string _currentSort;        
        private string _selectedSortProperty;
        private string _selectedSortDirection;
        private bool _isExportEnabled;
        private ViewTemplate _selectedViewType;
        private bool _isDataViewCompact;
        private bool _canSortColumns;

        // Private attributes
        private int _offset { get { return (CurrentPage - 1) * ItemsPerPage; } }
        // Pagination
        private string _selectedPage;
        private bool _isThereAnyPages;
        private int _startPageIndex = 1;
        private int _endPageIndex = 1;

        public ArticleDataManager ArticleDataManager { get; set; }
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
        // Which columns to show on datagrid (bound to CheckBox.IsChecked and converted with value converter)
        public List<string> Columns
        {
            get { return _columns; }
            set { _columns = value; OnPropertyChanged("Columns"); }
        }
        // Store fetched articles to show on data grid
        public ObservableCollection<Article> Articles { get; set; }
        // All existing users for combo box (Comment/SIC browsing)
        public ObservableCollection<User> Users { get; set; }
        // How to display data compact with datagrid or full with listview
        public ViewTemplate SelectedViewType
        {
            get { return _selectedViewType; }
            set { _selectedViewType = value; OnPropertyChanged("SelectedViewType"); }
        }
        public bool IsDataViewCompact
        {
            get { return _isDataViewCompact; }
            set { _isDataViewCompact = value; OnPropertyChanged("IsDataViewCompact"); }
        }
        // Pagination
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
            get { return _totalPages; }
            set { _totalPages = value; OnPropertyChanged("TotalPages"); }
        }
        public ObservableCollection<string> PageButtons { get; set; }
        public string SelectedPage
        {
            get { return _selectedPage; }
            set
            {
                _selectedPage = value;
                if (value != null && int.Parse(value) != CurrentPage)
                {
                    MoveToPage(int.Parse(value));
                }
                OnPropertyChanged("SelectedPage");
            }
        }
        public bool IsThereAnyPages
        {
            get { return _isThereAnyPages; }
            set { _isThereAnyPages = value; OnPropertyChanged("IsThereAnyPages"); }
        }
        // Export
        public bool CanExportP
        {
            get { return Articles.Where(article => article.Checked == true).ToList().Count != 0; }
        }
        public bool IsExportEnabled
        {
            get { return _isExportEnabled; }
            set { _isExportEnabled = value; OnPropertyChanged("IsExportEnabled"); }
        }
        // Sorting
        // If there are no articles in collection sorting should be disabled (it causes search bug otherwise)
        public bool CanSortColumns
        {
            get { return _canSortColumns; }
            set { _canSortColumns = value; OnPropertyChanged("CanSortColumns"); }
        }
        public List<string> SortableProperties { get; set; }
        public List<string> SortDirections { get; set; }
        public string SelectedSortProperty
        {
            get { return _selectedSortProperty; }
            set 
            {
                _selectedSortProperty = value; 
                OnPropertyChanged("SelectedSortProperty"); 
            }
        }
        public string SelectedSortDirection
        {
            get { return _selectedSortDirection; }
            set 
            {
                _selectedSortDirection = value; 
                OnPropertyChanged("SelectedSortDirection"); 
            }
        }
        public string SelectedSort { get { return SelectedSortProperty + " " + SelectedSortDirection; } }
        public Shared Services { get; set; }

        public ICommand SwitchDataViewCommand { get; set; }
        public ICommand LoadArticlesCommand { get; set; }
        public ICommand NextPageCommand { get; set; }
        public ICommand PreviousPageCommand { get; set; }
        public ICommand EnableExportCommand { get; set; }
        public ICommand UpdateExportStatusCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand SortFromDataGridCommand { get; set; }
        public ICommand SortFromRibbonCommand { get; set; }
        public ICommand CopyWordCommand { get; set; }
        public ICommand GetDuplicateArticlesCommand { get; set; }

        // Constructor
        public DataViewViewModel()
        {
            this.Services = Shared.GetInstance();
            this.User = Services.User;
            this._currentSort = "Title ASC";
            this.PageButtons = new ObservableCollection<string>();

            // Set neccessary columns to show on datagrid
            Columns = new List<string>()
            {
                "Authors",
                "Keywords",
                "Year"
            };

            if (User.IsAdmin) { Columns.Add("FileName"); }

            SortDirections = new List<string>() { "ASC", "DESC" };
            if (User.IsAdmin)
            {
                SortableProperties = new List<string>()
                {
                    "Title",
                    "Year",
                    "FileName",
                    "PersonalComment"
                };
            }
            else
            {
                SortableProperties = new List<string>()
                {
                    "Title",
                    "Year",
                    "PersonalComment"
                };
            }
            SelectedSortProperty = SortableProperties[0];
            SelectedSortDirection = SortDirections[0];

            // Initialize articles collection and paging
            Users = new ObservableCollection<User>(new UserRepo().GetUsers());
            Articles = new ObservableCollection<Article>();
            this.TitleSearchWords = new List<string>();
            this.TitleSearchPhrases = new List<string>();
            this.AbstractSearchWords = new List<string>();
            this.AbstractSearchPhrases = new List<string>();
            OnPropertyChanged("Articles");
            CurrentPage = 1;
            ItemsPerPage = 35;
            SelectedPage = "1";
            GenerateButtons();

            // Get the index of the logged in user and set it as selected index for combobox
            int index = 0;
            foreach (User userI in Users)
            {
                if (userI.ID == User.ID)
                    break;
                index++;
            }
            UserIndex = index;

            // Set up commands
            SwitchDataViewCommand = new RelayCommand(SwitchDataView);
            NextPageCommand = new RelayCommand(NextPage, CanNextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage, CanPreviousPage);
            LoadArticlesCommand = new RelayCommand(LoadArticles);
            EnableExportCommand = new RelayCommand(EnableExport);
            ExportCommand = new RelayCommand(Export, CanExport);
            SortFromDataGridCommand = new RelayCommand(SortFromDataGrid);
            SortFromRibbonCommand = new RelayCommand(SortFromRibbon);
            UpdateExportStatusCommand = new RelayCommand(UpdateExportStatus);
            CopyWordCommand = new RelayCommand(CopyWord);
            GetDuplicateArticlesCommand = new RelayCommand(GetDuplicateArticles);

            InitializeSearchOptions();
            this.SelectedViewType = new CompactViewTemplate();
            IsDataViewCompact = true;
            this.ArticleDataManager = new ArticleDataManager(LoadArticlesCommand);
        }

        // Command actions
        public void SwitchDataView(object input)
        {
            string view = input as string;
            if (view.Contains("Compact"))
            {
                SelectedViewType = new CompactViewTemplate();
                IsDataViewCompact = true;
            }
            else
            {
                SelectedViewType = new FullViewTemplate();
                IsDataViewCompact = false;
            }
        }
        public async void LoadArticles(object input = null)
        {
            OnPropertyChanged("FilterTitle");
            OnPropertyChanged("WordBreakMode");
            OnPropertyChanged("TitleHighlight");
            try
            {
                // This property is in SearchOptionsViewModel (partial class)
                // causes search options accordion to collapse on UI
                SearchOptionsIsChecked = false;

                string _filterTitle = FilterTitle;
                if (!String.IsNullOrEmpty(FilterTitle))
                {
                    FilterTitle = FilterTitle.Trim();
                }
                if (!String.IsNullOrEmpty(FilterAuthors))
                {
                    FilterAuthors = FilterAuthors.Trim();
                }
                if (!String.IsNullOrEmpty(FilterKeywords))
                {
                    FilterKeywords = FilterKeywords.Trim();
                }
                List<string> filterAuthorsFromString = String.IsNullOrEmpty(FilterAuthors) ? new List<string>() : FilterAuthors.Split(new string[] { ", " }, StringSplitOptions.None).ToList();
                List<string> filterKeywordsFromString = String.IsNullOrEmpty(FilterKeywords) ? new List<string>() : FilterKeywords.Split(new string[] { ", " }, StringSplitOptions.None).ToList();

                if (String.IsNullOrWhiteSpace(FilterTitle))
                {
                    this.TitleSearchPhrases= new List<string>();
                    this.TitleSearchWords = new List<string>();
                }
                else
                {
                    this.TitleSearchPhrases = Regex.Matches(FilterTitle, @"\[(.*?)\]").Cast<Match>().Select(m => m.Value.Substring(1, m.Value.Length - 2)).ToList();
                    this.TitleSearchWords = Regex.Replace(FilterTitle, @"\[(.*?)\]", "").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Except(dudWords).ToList();
                }
                if (String.IsNullOrWhiteSpace(FilterAbstract))
                {
                    this.AbstractSearchPhrases = new List<string>();
                    this.AbstractSearchWords = new List<string>();
                }
                else
                {
                    this.AbstractSearchPhrases = Regex.Matches(FilterAbstract, @"\[(.*?)\]").Cast<Match>().Select(m => m.Value.Substring(1, m.Value.Length - 2)).ToList();
                    this.AbstractSearchWords = Regex.Replace(FilterAbstract, @"\[(.*?)\]", "").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Except(dudWords).ToList();
                }

                OnPropertyChanged("AbstractWordsHighlight");
                OnPropertyChanged("AbstractPhrasesHighlight");

                FilterExstension.SetGlobalPairing(isLoose: false);

                Filter filter = new Filter();
                filter
                    .FilterTitle(TitleSearchWords.ToArray(), TitleSearchPhrases.ToArray())
                    .FilterAuthors(filterAuthorsFromString, SelectedAuthorPairing)
                    .FilterKeywords(filterKeywordsFromString, SelectedKeywordPairing)
                    .FilterYear(FilterYear)
                    .FilterPersonalComment(FilterPersonalComment)
                    .FilterAbstract(AbstractSearchWords.ToArray(), AbstractSearchPhrases.ToArray())
                    .FilterIds(GetFilterIds(IdFilter));

                Services.IsWorking(true);

                await Task.Run(() =>
                {
                    // 2. Calculate total pages
                    int record_count = new ArticleRepo().GetRecordCount(Users[UserIndex], filter.GetFilterString());
                    if ((record_count % ItemsPerPage) == 0)
                        TotalPages = record_count / ItemsPerPage;
                    else if (record_count == 0)
                        TotalPages = 0;
                    else
                        TotalPages = (record_count / ItemsPerPage) + 1;

                    IsThereAnyPages = TotalPages != 0;

                    CurrentPage = 1;
                });

                await PopulateArticles();

                OnPropertyChanged("AbstractHighlight");

                GenerateButtons();
                
                Services.IsWorking(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Load Articles", e.Message, e.StackTrace);
                Services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                Services.IsWorking(false);
            }

            OnPropertyChanged("SelectedSort");
        }
        // Pagination
        public void NextPage(object input = null)
        {
            CurrentPage++;
            MoveToPage(CurrentPage);
        }
        public bool CanNextPage(object input = null)
        {
            return CurrentPage < TotalPages;
        }
        public void PreviousPage(object input = null)
        {
            CurrentPage--;
            MoveToPage(CurrentPage);
        }
        public bool CanPreviousPage(object input = null)
        {
            return CurrentPage > 1;
        }
        // Export
        public void EnableExport(object input = null)
        {
            foreach (Article article in Articles)
                article.Checked = false;
            IsExportEnabled = !IsExportEnabled;
            UpdateExportStatus();
        }
        public async void Export(object input = null)
        {
            try
            {
                bool fileNotFound = false;
                string fileRootPath = String.Empty;

                // Destination will be the path chosen from dialog box (Where files should be exported)
                string destination = null;

                destination = Services.BrowserService.OpenFolderDialog(Services.LastExportFolderPath);

                // If path was chosen from the dialog box
                if (destination != null)
                {
                    Services.SaveExportPath(destination);

                    Services.IsWorking(true);

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
                                    fileRootPath = Path.Combine(Environment.CurrentDirectory, "Files\\") + article.FileName + ".pdf";
                                    if (!File.Exists(fileRootPath))
                                    {
                                        fileNotFound = true;
                                        continue;
                                    }
                                    File.Copy(fileRootPath, destination + "\\" + validName + "(" + article.FileName + ")" + ".pdf", true);
                                }
                                else
                                {
                                    string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars());
                                    Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                                    string validName = r.Replace(article.Title, "");
                                    fileRootPath = Path.Combine(Environment.CurrentDirectory, "Files\\") + article.FileName + ".pdf";
                                    if (!File.Exists(fileRootPath))
                                    {
                                        fileNotFound = true;
                                        continue;
                                    }
                                    File.Copy(fileRootPath, destination + "\\" + validName + "(" + article.FileName + ")" + ".pdf", true);
                                }
                            }
                        }
                    });

                    int exportedArticlesCount = 0;

                    // 3. Uncheck articles
                    foreach (Article article in Articles)
                    {
                        if (article.Checked == true)
                            exportedArticlesCount++;
                        article.Checked = false;
                    }

                    Services.IsWorking(false);

                    UpdateExportStatus();

                    //services.DialogService.OpenDialog(new DialogOkViewModel("Done", "Message", DialogType.Success));
                    if (fileNotFound)
                    {
                        Services.ShowNotification($"Some files couldn't be found, validate the database.", "Export", NotificationType.Error, "DataViewNotificationArea", new TimeSpan(0, 0, 4));
                        return;
                    }

                    Services.ShowNotification($"Exported {exportedArticlesCount} files successfully.", "Export", NotificationType.Success, "DataViewNotificationArea", new TimeSpan(0, 0, 4));
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Export", e.Message, e.StackTrace);
                Services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                Services.IsWorking(false);
            }

        }
        public bool CanExport(object input = null)
        {
            List<Article> checked_articles = Articles.Where(article => article.Checked == true).ToList();
            if (checked_articles.Count > 0)
                return true;

            return false;
        }
        public void UpdateExportStatus(object input = null)
        {
            OnPropertyChanged("CanExportP");
        }
        // Sorting
        public async void SortFromDataGrid(object input)
        {
            string header = input.ToString();
            header = header == "Comment" ? "PersonalComment" : header;

            // Ignore columns
            if (header == "Authors" || header == "Keywords" || header == "Export" || header == "Bookmark")
                return;


            if (Articles.Count == 0)
                return;

            if (_currentSort.Contains(header))
            {
                if (_currentSort.Contains("ASC"))
                {
                    SelectedSortDirection = "DESC";
                }
                else
                {
                    SelectedSortDirection = "ASC";
                }
            }
            else
            {
                SelectedSortDirection = "ASC";
            }
            SelectedSortProperty = header;

            _currentSort = SelectedSort;

            Services.IsWorking(true);
            await PopulateArticles();
            Services.IsWorking(false);

            OnPropertyChanged("SelectedSort");
        }
        public async void SortFromRibbon(object input = null)
        {
            _currentSort = SelectedSort;
            Services.IsWorking(true);
            await PopulateArticles();
            Services.IsWorking(false);
            OnPropertyChanged("SelectedSort");
        }
        public void CopyWord(object input)
        {
            Clipboard.SetText(input.ToString().Replace(",", ""));
        }
        public async void GetDuplicateArticles(object input = null)
        {
            Services.IsWorking(true);

            TotalPages = 1;
            CurrentPage = 1;
            IsThereAnyPages = true;

            // 1. Clear existing data grid source
            Articles.Clear();
            List<Article> articles = new List<Article>();
            await Task.Run(() =>
            {
                // 2. Fetch all artilces from database
                articles = FindDuplicateArticles(new ArticleRepo().GetAllArticles(Users[UserIndex], ""));
            });

            foreach (Article article in articles)
            {
                this.Articles.Add(article);
            }

            GenerateButtons();

            Services.IsWorking(false);
        }

        // Private helpers
        private async Task PopulateArticles()
        {
            List<string> filterAuthorsFromString = String.IsNullOrEmpty(FilterAuthors) ? new List<string>() : FilterAuthors.Split(new string[] { ", " }, StringSplitOptions.None).ToList();
            List<string> filterKeywordsFromString = String.IsNullOrEmpty(FilterKeywords) ? new List<string>() : FilterKeywords.Split(new string[] { ", " }, StringSplitOptions.None).ToList();

            FilterExstension.SetGlobalPairing(isLoose: false);
            Filter filter = new Filter();
            filter
                .FilterTitle(TitleSearchWords.ToArray(), TitleSearchPhrases.ToArray())
                .FilterAuthors(filterAuthorsFromString, SelectedAuthorPairing)
                .FilterKeywords(filterKeywordsFromString, SelectedKeywordPairing)
                .FilterYear(FilterYear)
                .FilterPersonalComment(FilterPersonalComment)
                .FilterAbstract(AbstractSearchWords.ToArray(), AbstractSearchPhrases.ToArray())
                .FilterIds(GetFilterIds(IdFilter))
                .Sort(_currentSort)
                .Paginate(ItemsPerPage, _offset);


            // 1. Clear existing data grid source
            Articles.Clear();
            List<Article> articles = new List<Article>();
            await Task.Run(() =>
            {
                string _filterTitle = FilterTitle;

                if (!string.IsNullOrWhiteSpace(_filterTitle))
                    _filterTitle = _filterTitle.Replace("'", "''");

                // 2. Fetch artilces from database
                articles = new ArticleRepo().LoadArticles(Users[UserIndex], filter.GetFilterString());
            });

            foreach (Article article in articles)
            {
                this.Articles.Add(article);
            }

            OnPropertyChanged("CanExportP");
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

        // Pagination
        private async void MoveToPage(int page)
        {
            try
            {
                CurrentPage = page;

                // 1. Check if next page is avaliable
                if (CurrentPage > TotalPages)
                    return;

                Services.IsWorking(true);

                await PopulateArticles();

                GenerateButtons();

                Services.IsWorking(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Move to page", e.Message, e.StackTrace);
                Services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                Services.IsWorking(false);
            }
        }
        private void GenerateButtons()
        {
            PageButtons.Clear();
            if (TotalPages == 0) { return; }

            // Default scenarion
            _startPageIndex = CurrentPage - 4;
            // Don't generate negative buttons
            if (_startPageIndex < 1) { _startPageIndex = 1; }
            // If we are reaching final page and the span is less than 10
            if (TotalPages - 9 > 1 && _startPageIndex > TotalPages - 9) { _startPageIndex = TotalPages - 9; }
            // If there is less than or exactly 10 pages to render don't slide the window cut
            if (TotalPages <= 10) { _startPageIndex = 1; }
            // Default scenario
            _endPageIndex = CurrentPage + 6;
            // If we are at start and span is less than 10
            if (_endPageIndex < 11 && TotalPages >= _endPageIndex)
            {
                if (TotalPages >= 10) { _endPageIndex = 11; }
                else { _endPageIndex = TotalPages + 1; }
            }
            // Don't go beyond total pages
            if (_endPageIndex > TotalPages) { _endPageIndex = TotalPages + 1; }

            for (int i = _startPageIndex; i < _endPageIndex; i++)
            {
                PageButtons.Add(i.ToString());
            }

            // Add last page button if necessary
            if (_endPageIndex <= TotalPages)
            {
                PageButtons.Add("...");
                PageButtons.Add(TotalPages.ToString());
            }

            SelectedPage = PageButtons.FirstOrDefault(p => p == CurrentPage.ToString());
        }

        // Levenstein
        private List<Article> FindDuplicateArticles(List<Article> articles)
        {
            Dictionary<long, Article> duplicateArticles = new Dictionary<long, Article>();

            for (int i = 0; i < articles.Count; i++)
            {
                for (int j = i + 1; j < articles.Count; j++)
                {
                    Services.IsWorking(true, $"Matching {i} to {j}");
                    // If duplciate was already found
                    if (duplicateArticles.ContainsKey((long)articles[j].ID))
                    {
                        continue;
                    }

                    double similarity = CalculateSimilarity(articles[i].Title, articles[j].Title);


                    // You can adjust the threshold value based on your requirements
                    if (similarity > 0.8)
                    {
                        if (!duplicateArticles.ContainsKey((long)articles[i].ID))
                        {
                            duplicateArticles.Add((long)articles[i].ID, articles[i]);
                        }
                        if (!duplicateArticles.ContainsKey((long)articles[j].ID))
                        {
                            duplicateArticles.Add((long)articles[j].ID, articles[j]);
                        }
                    }
                }
            }

            return duplicateArticles.Values.ToList();
        }
        private double CalculateSimilarity(string str1, string str2)
        {
            Levenstein levenshtein = new Levenstein();
            return levenshtein.GetSimilarity(str1, str2);
        }
    }
}
