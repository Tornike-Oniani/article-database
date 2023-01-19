﻿using Lib.DataAccessLayer.Models;
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

namespace MainLib.ViewModels.Main
{
    public partial class DataViewViewModel : BaseViewModel
    {
        #region Private members
        // Property attributes
        private List<string> _columns;
        private int _itemsPerPage;
        private int _totalPages;
        private int _currentPage;
        private int _userIndex;
        private Article _selectedArticle;
        private string _selectedSection;
        private bool _isSectionSelected;
        private string _currentSort;
        private bool _canSortColumns;
        private bool _isViewCompact;
        private string _selectedSortProperty;
        private string _selectedSortDirection;
        private bool _isExportEnabled;
        private Shared services;

        private int _offset { get { return (CurrentPage - 1) * ItemsPerPage; } }
        // Check if fetched data was done by simple search, so that pagination commands will work
        // correctly both on detailed and simple searches
        private bool _isSearchSimple = false;

        // Pagination
        private string _selectedPage;
        private bool _isThereAnyPages;
        private int _startPageIndex = 1;
        private int _endPageIndex = 1;
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
            get { return _totalPages; }
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
        // Pagination
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
        public bool CanSortColumns
        {
            get { return _canSortColumns; }
            set { _canSortColumns = value; OnPropertyChanged("CanSortColumns"); }
        }
        public bool IsViewCompact
        {
            get { return _isViewCompact; }
            set { _isViewCompact = value; OnPropertyChanged("IsViewCompact"); }
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
        public bool IsExportEnabled
        {
            get { return _isExportEnabled; }
            set { _isExportEnabled = value; OnPropertyChanged("IsExportEnabled"); }
        }
        public Shared Services { get; set; }

        #region Commands
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand NextPageCommand { get; set; }
        public RelayCommand PreviousPageCommand { get; set; }
        public RelayCommand LoadArticlesCommand { get; set; }
        public ICommand EnableExportCommand { get; set; }
        public RelayCommand ExportCommand { get; set; }
        public RelayCommand DeleteArticleCommand { get; set; }
        public RelayCommand MassBookmarkCommand { get; set; }
        public RelayCommand SortFromDataGridCommand { get; set; }
        public ICommand SortFromRibbonCommand { get; set; }
        public ICommand UpdateExportStatusCommand { get; set; }
        public ICommand CopyTitleCommand { get; set; }
        public ICommand SwitchDataViewCommand { get; set; }
        public ICommand ExpandAbstractCommand { get; set; }

        public RelayCommand OpenSearchDialogCommand { get; set; }
        public RelayCommand OpenAddPersonalCommand { get; set; }
        public RelayCommand OpenEditCommand { get; set; }
        public ICommand OpenAbstractEditorCommand { get; set; }
        public RelayCommand OpenBookmarkManagerCommand { get; set; }
        public RelayCommand OpenMassBookmarkManagerCommand { get; set; }
        public RelayCommand OpenReferenceManagerCommand { get; set; }
        #endregion

        // Constructor
        public DataViewViewModel()
        {
            this.services = Shared.GetInstance();
            this.Services = Shared.GetInstance();
            this.User = services.User;
            this._currentSort = "Title ASC";

            this.PageButtons = new ObservableCollection<string>();

            // 1. Set neccessary columns to show on datagrid
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

            // Initialize sections collection
            this.Sections = new ObservableCollection<string>() { };

            // 2. Initialize articles collection and paging
            Users = new ObservableCollection<User>(new UserRepo().GetUsers());
            Articles = new ObservableCollection<Article>();
            CurrentPage = 1;
            ItemsPerPage = 35;
            SelectedPage = "1";
            IsViewCompact = true;
            GenerateButtons();

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
            SwitchDataViewCommand = new RelayCommand(SwitchDataView);
            OpenFileCommand = new RelayCommand(OpenFile);
            NextPageCommand = new RelayCommand(NextPage, CanNextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage, CanPreviousPage);
            LoadArticlesCommand = new RelayCommand(LoadArticles);
            EnableExportCommand = new RelayCommand(EnableExport);
            ExportCommand = new RelayCommand(Export, CanExport);
            DeleteArticleCommand = new RelayCommand(DeleteArticle, CanDeleteArticle);
            MassBookmarkCommand = new RelayCommand(MassBookmark);
            SortFromDataGridCommand = new RelayCommand(SortFromDataGrid);
            SortFromRibbonCommand = new RelayCommand(SortFromRibbon);
            UpdateExportStatusCommand = new RelayCommand(UpdateExportStatus);
            this.CopyTitleCommand = new RelayCommand(CopyTitle);
            ExpandAbstractCommand = new RelayCommand(ExpandAbstract);

            OpenSearchDialogCommand = new RelayCommand(OpenSearchDialog);
            OpenAddPersonalCommand = new RelayCommand(OpenAddPersonal, IsArticleSelected);
            OpenEditCommand = new RelayCommand(OpenEditDialog, IsArticleSelected);
            OpenAbstractEditorCommand = new RelayCommand(OpenAbstractEditor);
            OpenBookmarkManagerCommand = new RelayCommand(OpenBookmarkManager, IsArticleSelected);
            OpenMassBookmarkManagerCommand = new RelayCommand(OpenMassBookmarkManager, CanOpenMassBookmarkManager);
            OpenReferenceManagerCommand = new RelayCommand(OpenReferenceManager, IsArticleSelected);

            InitializeSearchOptions();

            // Set up section selector
            this.FinishCommand = new RelayCommand(Finish);

            CanSortColumns = false;

            // Populate sections collection from json file
            PopulateSections();
        }

        #region Command Actions
        public void ExpandAbstract(object input)
        {
            Article article = input as Article;
            article.AbstractExpanded = true;
        }
        public void SwitchDataView(object input)
        {
            string view = input as string;
            IsViewCompact = view.Contains("Compact");
        }
        public void OpenFile(object input)
        {
            // 1. If no item was selected return
            if (SelectedArticle == null && input == null)
                return;

            string full_path = "";
            if (input != null)
            {
                // 2. Get full path of the file
                 full_path = Environment.CurrentDirectory + "\\Files\\" + ((Article)input).FileName + ".pdf";
            }
            else
            {
                full_path = Environment.CurrentDirectory + "\\Files\\" + SelectedArticle.FileName + ".pdf";
            }           

            // 3. Open file with default program
            try
            {
                Process.Start(full_path);
            }
            // 4. Catch if file doesn't exist physically
            catch
            {
                services.DialogService.OpenDialog(new DialogOkViewModel("File was not found", "Error", DialogType.Error));
            }
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
        // Fetching articles
        public async void LoadArticles(object input = null)
        {
            // Notify state that articles were fetched by detailed options
            _isSearchSimple = false;
            SimpleSearch = "";

            OnPropertyChanged("FilterTitle");
            try
            {
                // This property is in SearchOptionsViewModel (partial class)
                // causes search options accordion to collapse on UI
                SearchOptionsIsChecked = false;

                string _filterTitle = FilterTitle;
                List<string> filterAuthorsFromString = String.IsNullOrEmpty(FilterAuthors) ? new List<string>() : FilterAuthors.Split(new string[] { ", " }, StringSplitOptions.None).ToList();
                List<string> filterKeywordsFromString = String.IsNullOrEmpty(FilterKeywords) ? new List<string>() : FilterKeywords.Split(new string[] { ", " }, StringSplitOptions.None).ToList();

                FilterExstension.SetGlobalPairing(isLoose: false);

                Filter filter = new Filter();
                filter
                    .FilterTitle(_filterTitle, WordBreakMode)
                    .FilterAuthors(filterAuthorsFromString, SelectedAuthorPairing)
                    .FilterKeywords(filterKeywordsFromString, SelectedKeywordPairing)
                    .FilterYear(FilterYear)
                    .FilterPersonalComment(FilterPersonalComment)
                    .FilterIds(GetFilterIds(IdFilter));

                services.IsWorking(true);

                List<Article> articles = new List<Article>();
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

                GenerateButtons();

                if (!string.IsNullOrEmpty(SelectedSection) && SelectedSection != "None")
                    this.IsSectionSelected = true;
                else
                    this.IsSectionSelected = false;
                
                services.IsWorking(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Load Articles", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
            }

            OnPropertyChanged("SelectedSort");
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

                destination = services.BrowserService.OpenFolderDialog(services.LastExportFolderPath);

                // If path was chosen from the dialog box
                if (destination != null)
                {
                    services.SaveExportPath(destination);

                    services.IsWorking(true);

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

                    services.IsWorking(false);

                    UpdateExportStatus();

                    //services.DialogService.OpenDialog(new DialogOkViewModel("Done", "Message", DialogType.Success));
                    if (fileNotFound)
                    {
                        services.ShowNotification($"Some files couldn't be found, validate the database.", "Export", NotificationType.Error, "DataViewNotificationArea", new TimeSpan(0, 0, 4));
                        return;
                    }

                    services.ShowNotification($"Exported {exportedArticlesCount} files successfully.", "Export", NotificationType.Success, "DataViewNotificationArea", new TimeSpan(0, 0, 4));
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Export", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
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
        public void DeleteArticle(object input = null)
        {
            try
            {
                // 1. Ask user if they are sure
                if (services.DialogService.OpenDialog(
                    new DialogYesNoViewModel("Are you sure you want to delete selected article?", "Warning", DialogType.Question)
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
                        services.DialogService.OpenDialog(
                            new DialogOkViewModel("The file is missing, validate your database.", "Error", DialogType.Error));
                    }

                    // 4. Refresh the data grid
                    LoadArticlesCommand.Execute(null);
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Delete Article", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
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
                    //_currentSort = $"{header} DESC";
                    SelectedSortDirection = "DESC";
                }
                else
                {
                    //_currentSort = $"{header} ASC";
                    SelectedSortDirection = "ASC";
                }
            }
            else
            {
                //_currentSort = $"{header} ASC";
                SelectedSortDirection = "ASC";
            }
            SelectedSortProperty = header;

            _currentSort = SelectedSort;

            services.IsWorking(true);
            await PopulateArticles();
            services.IsWorking(false);

            OnPropertyChanged("SelectedSort");
        }
        public async void SortFromRibbon(object input = null)
        {
            _currentSort = SelectedSort;
            services.IsWorking(true);
            await PopulateArticles();
            services.IsWorking(false);
            OnPropertyChanged("SelectedSort");
        }
        public void CopyTitle(object input)
        {
            Clipboard.SetText(((Article)input).Title);
        }

        // Window open commands
        public void OpenSearchDialog(object input = null)
        {
            services.WindowService.OpenWindow(new SearchDialogViewModel(this), passWindow: true);
        }
        public void OpenAddPersonal(object input)
        {
            services.WindowService.OpenWindow(new MainLib.ViewModels.Popups.AddPersonalDialogViewModel(SelectedArticle));
        }
        public void OpenBookmarkManager(object input = null)
        {
            services.WindowService.OpenWindow(new BookmarkManagerViewModel(ViewType.DataView, input as Article));
        }
        public void OpenEditDialog(object input = null)
        {
            services.WindowService.OpenWindow(new MainLib.ViewModels.Popups.ArticleEditorViewModel(SelectedArticle));
        }
        public void OpenAbstractEditor(object input)
        {
            Article articleToPass = input == null ? SelectedArticle : input as Article;
            services.WindowService.OpenWindow(new AbstractEditorViewModel(articleToPass), passWindow: true);
        }
        public void OpenMassBookmarkManager(object input = null)
        {
            services.WindowService.OpenWindow(new MassBookmarkManagerViewModel(BMCheckedArticles));
        }
        public void OpenReferenceManager(object input)
        {
            services.WindowService.OpenWindow(new ReferenceManagerViewModel(ViewType.DataView, input as Article));
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
                services.DialogService.OpenDialog(new DialogYesNoViewModel("Are you sure you want to remove this section from pending?", "Finish Section", DialogType.Warning)))
            {
                try
                {
                    services.IsWorking(true);

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

                    services.IsWorking(false);

                    // 3. Clear articles collection
                    this.Sections.Remove(SelectedSection);
                    this.SelectedSection = this.Sections.First();
                    this.Articles.Clear();
                    this.IsSectionSelected = false;
                }
                catch (Exception e)
                {
                    new BugTracker().Track("DataView", "Finish", e.Message, e.StackTrace);
                    services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
                }
                finally
                {
                    services.IsWorking(false);
                }
            }
        }
        #endregion

        // Private helpers
        private async Task PopulateArticles()
        {
            List<string> filterAuthorsFromString = String.IsNullOrEmpty(FilterAuthors) ? new List<string>() : FilterAuthors.Split(new string[] { ", " }, StringSplitOptions.None).ToList();
            List<string> filterKeywordsFromString = String.IsNullOrEmpty(FilterKeywords) ? new List<string>() : FilterKeywords.Split(new string[] { ", " }, StringSplitOptions.None).ToList();

            FilterExstension.SetGlobalPairing(isLoose: false);
            Filter filter = new Filter();
            filter
                .FilterTitle(_filterTitle, WordBreakMode)
                .FilterAuthors(filterAuthorsFromString, SelectedAuthorPairing)
                .FilterKeywords(filterKeywordsFromString, SelectedKeywordPairing)
                .FilterYear(FilterYear)
                .FilterPersonalComment(FilterPersonalComment)
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
                foreach (Article article in new ArticleRepo().LoadArticles(Users[UserIndex], filter.GetFilterString()))
                {
                    articles.Add(article);
                }
            });

            // 3. Populate article collection
            foreach (Article article in articles)
                this.Articles.Add(article);

            CanSortColumns = this.Articles.Count != 0;

            OnPropertyChanged("CanExportP");
        }
        private async Task PopulateArticlesSimple()
        {
            FilterExstension.SetGlobalPairing(isLoose: true);
            Filter filter = new Filter();
            if (!String.IsNullOrEmpty(SimpleSearch))
            {
                filter
                .FilterTitle(SimpleSearch, true)
                .FilterAuthors(SimpleSearch.Split(' ').ToList(), "AND")
                .FilterKeywords(SimpleSearch.Split(' ').ToList(), "AND");
            }

            filter
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
                foreach (Article article in new ArticleRepo().LoadArticles(Users[UserIndex], filter.GetFilterString()))
                {
                    articles.Add(article);
                }
            });

            // 3. Populate article collection
            foreach (Article article in articles)
                this.Articles.Add(article);

            CanSortColumns = this.Articles.Count != 0;

            OnPropertyChanged("CanExportP");
        }
        private async Task PopulateSections()
        {
            this.Sections.Clear();

            try
            {
                List<string> sections = new List<string>();

                services.IsWorking(true);

                await Task.Run(() =>
                {
                    string info = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "sections.json"));

                    JsonConvert.DeserializeObject<List<string>>(info).ForEach((cur) =>
                    {
                        sections.Add(cur);
                    });
                });

                services.IsWorking(false);

                this.Sections.Add("None");
                sections.ForEach((cur) =>
                {
                    this.Sections.Add(cur);
                });
                this.SelectedSection = this.Sections.First();
            }
            catch (Exception e)
            {
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong", "Error", DialogType.Error));
                new BugTracker().Track("Data View", "Reading sections from json", e.Message, e.StackTrace);
            }
            finally
            {
                services.IsWorking(false);
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

        // Pagination
        private async void MoveToPage(int page)
        {
            try
            {
                CurrentPage = page;

                // 1. Check if next page is avaliable
                if (CurrentPage > TotalPages)
                    return;

                services.IsWorking(true);

                // 3. Populate article collection
                if (_isSearchSimple)
                    await PopulateArticlesSimple();
                else
                    await PopulateArticles();

                GenerateButtons();

                services.IsWorking(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Move to page", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
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
    }
}
