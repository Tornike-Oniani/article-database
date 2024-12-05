using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.DataAccessLayer.Utils;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Classes;
using MainLib.ViewModels.Popups;
using MainLib.ViewModels.Utils;
using NotificationService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MainLib.ViewModels.Main
{
    public class BrowseViewModel : BaseViewModel
    {
        #region Private members
        private readonly string[] dudWords = new string[] { "in", "of", "at", "to", "into", "on", "onto", "a", "the", "and" };
        private List<Article> articles = new List<Article>();
        private readonly Shared services;
        private readonly PdfCreator pdfCreator;
        #endregion

        #region Property fields
        private string _term;
        private string _authors;
        private string _year;
        private bool _showAdditionalFilters;
        private List<string> _termsWordsHighlight;
        private List<string> _termsPhrasesHighlight;
        private bool _showResults;
        private int _currentPage;
        private string _sortString;
        private bool _isBatchExporting;
        private bool _isSelectingArticlesForPrinting;
        #endregion

        #region Public properties
        // Filters
        public string Terms
		{
			get { return _term; }
			set { _term = FormatText(value); OnPropertyChanged("Terms"); }
		}
		public string Authors
		{
			get { return _authors; }
			set { _authors = value; OnPropertyChanged("Authors"); }
		}
		public string Year
		{
			get { return _year; }
			set { _year = value; OnPropertyChanged("Year"); }
		}
        // Highlights
        public List<string> TermsWordsHighlight
        {
            get { return _termsWordsHighlight; }
            set { _termsWordsHighlight = value; OnPropertyChanged("TermsWordsHighlight"); }
        }
        public List<string> TermsPhrasesHighlight
        {
            get { return _termsPhrasesHighlight; }
            set { _termsPhrasesHighlight = value; OnPropertyChanged("TermsPhrasesHighlight"); }
        }
        public string AuthorHighlight
        {
            get
            {
                if (String.IsNullOrEmpty(Authors)) { return ""; }
                return Authors;
            }
        }
        // Generic
        public ObservableCollection<Article> Articles { get; set; }
        public User User { get; set; }
        // Visibility
        public bool ShowAdditionalFilters
        {
            get { return _showAdditionalFilters; }
            set { _showAdditionalFilters = value; OnPropertyChanged("ShowAdditionalFilters"); }
        }
        public bool ShowResults
        {
            get { return _showResults; }
            set { _showResults = value; OnPropertyChanged("ShowResults"); }
        }
        public bool ShowNoResultsLabel
        {
            get
            {
                return ShowResults && Articles.Count == 0;
            }
        }
        // Pagination
        public int ItemsPerPage { get; set; }
        public int CurrentPage
        {
            get { return _currentPage; }
            set 
            { 
                _currentPage = value; 
                OnPropertyChanged("CurrentPage"); 
                OnPropertyChanged("CanGoToNextPage"); 
                OnPropertyChanged("CanGoToPreviousPage"); 
            }
        }
        public int TotalPages 
        { 
            get
            {
                if ((articles.Count % ItemsPerPage) == 0)
                {
                    return articles.Count / ItemsPerPage;
                }
                else if (articles.Count == 0)
                {
                    return 1;
                }
                else
                {
                    return (articles.Count / ItemsPerPage) + 1;
                }
            }
        }
        public bool CanGoToNextPage 
        { 
            get { return this.CurrentPage < this.TotalPages; } 
        }
        public bool CanGoToPreviousPage 
        { 
            get { return this.CurrentPage > 1; } 
        }
        public int ResultsCount { get { return this.articles.Count; } }
        // Sorting
        public string SortString
        {
            get { return _sortString; }
            set { _sortString = value; OnPropertyChanged("SortString"); }
        }
        // Search history
        public SearchHistoryManager SearchHistoryManager { get; set; }
        public ObservableCollection<SearchEntry> RecentSearches { get; set; }
        public ObservableCollection<SearchEntry> FavoriteSearches { get; set; }
        // Export
        public List<Article> ArticlesToBeExported { get; set; }
        public List<Article> ArticlesToBePrinted { get; set; }
        public bool IsBatchExporting
        {
            get { return _isBatchExporting; }
            set { _isBatchExporting = value; OnPropertyChanged("IsBatchExporting"); }
        }
        public bool CanBatchExport
        {
            get
            {
                return ArticlesToBeExported.Count > 0;
            }
        }
        public bool CanPrintSelectedArticles
        {
            get
            {
                return ArticlesToBePrinted.Count > 0;
            }
        }

        public bool IsSelectingArticlesForPrinting
        {
            get { return _isSelectingArticlesForPrinting; }
            set { _isSelectingArticlesForPrinting = value; OnPropertyChanged("IsSelectingArticlesForPrinting"); }
        }

        #endregion

        #region Commands
        public ICommand SearchCommand { get; set; }
        public ICommand NextPageCommand { get; set; }
        public ICommand PreviousPageCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand SortCommand { get; set; }
        public ICommand OpenFileCommand { get; set; }
        public ICommand DownloadFileCommand { get; set; }
        public ICommand ChangeItemsPerPageCommand { get; set; }
        public ICommand ApplyRecentSearchCommand { get; set; }
        public ICommand ToggleFavoriteSearchCommand { get; set; }
        public ICommand DeleteRecentSearchCommand { get; set; }
        public ICommand ClearRecentSearchesCommand { get; set; }
        public ICommand StartBatchExportCommand { get; set; }
        public ICommand BatchExportCommand { get; set; }
        public ICommand CancelBatchExportCommand { get; set; }
        public ICommand MarkArticleForBatchExportCommand { get; set; }
        public ICommand PrintCurrentPageCommand { get; set; }
        public ICommand PrintAllResultsCommand { get; set; }
        public ICommand SelectArticlesToPrintCommand { get; set; }
        public ICommand CancelSelectingArticlesToPrintCommand { get; set; }
        public ICommand MarkArticleForPrintCommand { get; set; }
        public ICommand PrintSelectedArticlesCommand { get; set; }
        #endregion

        #region Events
        public event EventHandler ScrollToTopRequested;
        protected virtual void OnScrollTopRequested()
        {
            this.ScrollToTopRequested?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Constructors
        public BrowseViewModel()
        {
            // Generic
            this.services = Shared.GetInstance();
            this.User = Shared.GetInstance().User;
            this.Articles = new ObservableCollection<Article>();

            // Highlight
            this.TermsWordsHighlight = new List<string>();
            this.TermsPhrasesHighlight = new List<string>();

            // Pagination
            this.ItemsPerPage = 25;
            this.CurrentPage = 1;

            // Sorting
            this.SortString = "Title ASC";

            // Search history
            this.SearchHistoryManager = new SearchHistoryManager();
            this.RecentSearches = new ObservableCollection<SearchEntry>(SearchHistoryManager.SearchHistory.RecentSearches);
            this.FavoriteSearches = new ObservableCollection<SearchEntry>(SearchHistoryManager.SearchHistory.FavoriteSearches);

            // Export
            this.ArticlesToBeExported = new List<Article>();

            // Printing
            this.pdfCreator = new PdfCreator();
            this.ArticlesToBePrinted = new List<Article>();

            // Commands
            this.SearchCommand = new RelayCommand(Search, CanSearch);
            this.NextPageCommand = new RelayCommand(NextPage, CanNextPage);
            this.PreviousPageCommand = new RelayCommand(PreviousPage, CanPreviousPage);
            this.SortCommand = new RelayCommand(Sort);
            this.ClearCommand = new RelayCommand(Clear);
            this.OpenFileCommand = new RelayCommand(OpenFile);
            this.DownloadFileCommand = new RelayCommand(DownloadFile);
            this.ChangeItemsPerPageCommand = new RelayCommand(ChangeItemsPerPage);
            this.ApplyRecentSearchCommand = new RelayCommand(ApplyRecentSearch);
            this.ToggleFavoriteSearchCommand = new RelayCommand(ToggleFavoriteSearch);
            this.DeleteRecentSearchCommand = new RelayCommand(DeleteRecentSearch);
            this.ClearRecentSearchesCommand = new RelayCommand(ClearRecentSearches);
            this.StartBatchExportCommand = new RelayCommand(StartBatchExport);
            this.BatchExportCommand = new RelayCommand(BatchExport);
            this.CancelBatchExportCommand = new RelayCommand(CancelBatchExport);
            this.MarkArticleForBatchExportCommand = new RelayCommand(MarkArticleForBatchExport);
            this.PrintCurrentPageCommand = new RelayCommand(PrintCurrentPage);
            this.PrintAllResultsCommand = new RelayCommand(PrintAllResults);
            this.SelectArticlesToPrintCommand = new RelayCommand(SelectArticlesToPrint);
            this.PrintSelectedArticlesCommand = new RelayCommand(PrintSelectedArticles);
            this.MarkArticleForPrintCommand = new RelayCommand(MarkArticleForPrint);
            this.CancelSelectingArticlesToPrintCommand = new RelayCommand(CancelSelectingArticlesToPrint);
        }
        #endregion

        #region Command actions
        public async void Search(object input)
        {
            // Extract words and phrases from terms (they are distinguished by if they are inside brackets or not)
            List<string> termWords = new List<string>();
            List<string> termPhrases = new List<string>();
            if (!String.IsNullOrWhiteSpace(Terms))
            {
                //termWords = Regex.Matches(Terms, @"\[(.*?)\]").Cast<Match>().Select(m => m.Value.Substring(1, m.Value.Length - 2)).ToList();
                //termPhrases = Regex.Replace(Terms, @"\[(.*?)\]", "").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Except(dudWords, StringComparer.OrdinalIgnoreCase).ToList();
                termWords = Regex.Replace(Terms, @"\[(.*?)\]", "").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Except(dudWords, StringComparer.OrdinalIgnoreCase).ToList();
                termPhrases = Regex.Matches(Terms, @"\[(.*?)\]").Cast<Match>().Select(m => m.Value.Substring(1, m.Value.Length - 2)).ToList();
            }

            // Set up words to be highlighted
            this.TermsWordsHighlight = termWords;
            this.TermsPhrasesHighlight = termPhrases;

            // Authors
            List<string> filterAuthorsFromString = String.IsNullOrEmpty(Authors) ? new List<string>() : Authors.Split(new string[] { ", " }, StringSplitOptions.None).ToList();

            // Build a filter
            FilterExstension.SetGlobalPairing(isLoose: true);
            Filter filter = new Filter();
            filter
                .FilterTitle(termWords.ToArray(), termPhrases.ToArray())
                .FilterAbstract(termWords.ToArray(), termPhrases.ToArray())
                .FilterAuthors(filterAuthorsFromString, "AND")
                .FilterKeywords(termWords, "AND")
                .FilterYear(Year);

            // Fetch articles from database
            services.IsWorking(true);
            await Task.Run(() =>
            {
                ExtraFilter extraFilter = new ExtraFilter();
                this.articles = new ArticleRepo().LoadArticles(User, filter);
                this.articles = extraFilter.FilterArticlesWithTerms(articles, termWords, termPhrases);
            });
            services.IsWorking(false);

            PopulateArticles();
            this.CurrentPage = 1;
            this.ShowResults = true;
            OnPropertyChanged("TotalPages");
            OnPropertyChanged("ShowNoResultsLabel");
            SortArticles();

            // If search contains authors or years make the additional filters visible (usually required when applying recent search)
            if (!String.IsNullOrEmpty(Authors) || !String.IsNullOrEmpty(Year))
            {
                this.ShowAdditionalFilters = true;
            }

            // Add search to history
            if ((!String.IsNullOrEmpty(this.Terms) || !String.IsNullOrEmpty(this.Authors) || !String.IsNullOrEmpty(this.Year)) && (string)input != "RecentSearch")
            {
                SearchEntry newSearchEntry = new SearchEntry() { Terms = this.Terms, Authors = this.Authors, Year = this.Year };
                SearchHistoryManager.AddSearchToHistory(newSearchEntry);
                if (!RecentSearches.Contains(newSearchEntry, new SearchEntryComparer()))
                {
                    RecentSearches.Insert(0, newSearchEntry);
                }
            }
        }
        public void NextPage(object input = null)
        {
            this.CurrentPage += 1;
            PopulateArticles();
        }
        public void PreviousPage(object input = null)
        {
            this.CurrentPage -= 1;
            PopulateArticles();
        }
        public void Sort(object input)
        {
            this.SortString = input.ToString();
            SortArticles();
        }
        public void Clear(object input = null)
        {
            this.CancelBatchExport();
            this.CancelSelectingArticlesToPrint();
            this.Articles.Clear();
            this.Terms = String.Empty;
            this.Authors = String.Empty;
            this.Year = String.Empty;
            this.TermsWordsHighlight.Clear();
            this.TermsPhrasesHighlight.Clear();
            this.CurrentPage = 1;
            this.ShowResults = false;
            this.SortString = "Title ASC";
            this.articles.Clear();
        }
        public void OpenFile(object input)
        {
            // 1. If no item was selected return
            if (input == null)
            {
                return;
            }

            // 2. Get full path of the file
            string full_path = "";
            full_path = Environment.CurrentDirectory + "\\Files\\" + ((Article)input).FileName + ".pdf";

            // 3. Open file with default program
            try
            {
                Process.Start(full_path);
            }
            // 4. Catch if file doesn't exist physically
            catch
            {
                this.services.ShowDialogWithOverlay(new DialogOkViewModel("File was not found", "Error", DialogType.Error));
            }
        }
        public async void DownloadFile(object input)
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
                        Article articleToExport = (Article)input;

                        if (!string.IsNullOrEmpty(articleToExport.FileName))
                        {
                            // If title is too long just get the substring to name the .pdf file
                            if (articleToExport.Title.Length > 40)
                            {
                                string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars());
                                Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                                string validName = r.Replace(articleToExport.Title.Substring(0, 40), "");
                                fileRootPath = Path.Combine(Environment.CurrentDirectory, "Files\\") + articleToExport.FileName + ".pdf";
                                if (!File.Exists(fileRootPath))
                                {
                                    fileNotFound = true;
                                }
                                File.Copy(fileRootPath, destination + "\\" + validName + "(" + articleToExport.FileName + ")" + ".pdf", true);
                            }
                            else
                            {
                                string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars());
                                Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                                string validName = r.Replace(articleToExport.Title, "");
                                fileRootPath = Path.Combine(Environment.CurrentDirectory, "Files\\") + articleToExport.FileName + ".pdf";
                                if (!File.Exists(fileRootPath))
                                {
                                    fileNotFound = true;
                                }
                                File.Copy(fileRootPath, destination + "\\" + validName + "(" + articleToExport.FileName + ")" + ".pdf", true);
                            }
                        }
                    });

                    services.IsWorking(false);

                    //services.DialogService.OpenDialog(new DialogOkViewModel("Done", "Message", DialogType.Success));
                    if (fileNotFound)
                    {
                        services.ShowNotification($"File couldn't be found, validate the database.", "Download", NotificationType.Error, "DataViewNotificationArea", new TimeSpan(0, 0, 4));
                        return;
                    }

                    services.ShowNotification($"File downloaded successfully.", "Download", NotificationType.Success, "DataViewNotificationArea", new TimeSpan(0, 0, 2));
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Export", e.Message, e.StackTrace);
                services.ShowDialogWithOverlay(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
            }
        }
        public void ChangeItemsPerPage(object input)
        {
            this.ItemsPerPage = int.Parse(input.ToString());
            this.CurrentPage = 1;
            OnPropertyChanged("TotalPages");
            PopulateArticles();
        }
        public void ApplyRecentSearch(object input)
        {
            SearchEntry searchEntry = input as SearchEntry;
            this.Terms = searchEntry.Terms;
            this.Authors = searchEntry.Authors;
            this.Year = searchEntry.Year;
            this.SearchCommand.Execute("RecentSearch");
        }
        public void ToggleFavoriteSearch(object input)
        {
            SearchEntry searchEntry = input as SearchEntry;
            // We have to reverse here because the ToggleButton first switches the IsFavorite property and then this command is run
            if (searchEntry.IsFavorite)
            {
                this.SearchHistoryManager.AddSearchToFavorites(searchEntry);
                this.FavoriteSearches.Insert(0, searchEntry);
            }
            else
            {
                this.SearchHistoryManager.RemoveSearchFromFavorites(searchEntry);
                SearchEntry searchEntryToRemove = this.FavoriteSearches.Single(item => new SearchEntryComparer().Equals(item, searchEntry));
                this.FavoriteSearches.Remove(searchEntryToRemove);
                // If we remove favorite from favorites list we have to make sure the IsFavorite property is also set to false to recent search item equivalent
                SearchEntry searchEntryToUnfavorite = this.RecentSearches.SingleOrDefault(item => new SearchEntryComparer().Equals(item, searchEntry));
                if (searchEntryToUnfavorite != null && searchEntryToUnfavorite.IsFavorite)
                {
                    searchEntryToUnfavorite.IsFavorite = false;
                }
            }

        }
        public void DeleteRecentSearch(object input)
        {
            SearchEntry searchEntry = input as SearchEntry;
            this.RecentSearches.Remove(searchEntry);
            this.SearchHistoryManager.DeleteSearchFromHstory(searchEntry);
        }
        public void ClearRecentSearches(object input = null)
        {
            this.RecentSearches.Clear();
            this.SearchHistoryManager.ClearSearchHistory();
        }
        public void StartBatchExport(object input = null)
        {
            this.IsBatchExporting = true;
        }
        public async void BatchExport(object input = null)
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
                        foreach (Article article in ArticlesToBeExported)
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

                    services.IsWorking(false);

                    if (fileNotFound)
                    {
                        services.ShowNotification($"Some files couldn't be found, validate the database.", "Download", NotificationType.Error, "NotificationArea", new TimeSpan(0, 0, 4));
                        return;
                    }

                    services.ShowNotification($"File(s) downloaded successfully.", "Download", NotificationType.Success, "NotificationArea", new TimeSpan(0, 0, 2));
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Export", e.Message, e.StackTrace);
                services.ShowDialogWithOverlay(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
                CancelBatchExport();
            }
        }
        public void CancelBatchExport(object input = null)
        {
            this.ArticlesToBeExported.Clear();
            this.IsBatchExporting = false;
            OnPropertyChanged("ArticlesToBeExported");
            OnPropertyChanged("CanBatchExport");
        }
        public void MarkArticleForBatchExport(object input)
        {
            Article article = input as Article;
            if (ArticlesToBeExported.Exists(a => a.ID == article.ID))
            {
                Article articleToRemove = ArticlesToBeExported.SingleOrDefault(a => a.ID == article.ID);
                ArticlesToBeExported.Remove(articleToRemove);
            }
            else
            {
                ArticlesToBeExported.Add(article);
            }
            OnPropertyChanged("ArticlesToBeExported");
            OnPropertyChanged("CanBatchExport");
        }
        public async void PrintCurrentPage(object input = null)
        {
            await this.pdfCreator.Print(this.Articles.ToList());
            this.services.ShowNotification("Print saved as Pdf", "Printing", NotificationType.Success, NotificationAreaTypes.Default, new TimeSpan(0, 0, 2));
        }
        public async void PrintAllResults(object input = null)
        {
            if (this.services.ShowDialogWithOverlay(new DialogYesNoViewModel($"Are you sure you want to print {this.articles.Count} articles?", "Print all articles", DialogType.Warning)))
            {
                await this.pdfCreator.Print(this.articles);
                this.services.ShowNotification("Print saved as Pdf", "Printing", NotificationType.Success, NotificationAreaTypes.Default, new TimeSpan(0, 0, 2));
            }
        }
        public void SelectArticlesToPrint(object input = null)
        {
            this.IsSelectingArticlesForPrinting = true;
        }
        public void CancelSelectingArticlesToPrint(object input = null)
        {
            this.ArticlesToBePrinted.Clear();
            this.IsSelectingArticlesForPrinting = false;
            OnPropertyChanged("ArticlesToBePrinted");
            OnPropertyChanged("CanPrintSelectedArticles");
        }
        public void MarkArticleForPrint(object input)
        {
            Article article = input as Article;
            if (ArticlesToBePrinted.Exists(a => a.ID == article.ID))
            {
                Article articleToRemove = ArticlesToBePrinted.SingleOrDefault(a => a.ID == article.ID);
                ArticlesToBePrinted.Remove(articleToRemove);
            }
            else
            {
                ArticlesToBePrinted.Add(article);
            }
            OnPropertyChanged("ArticlesToBePrinted");
            OnPropertyChanged("CanPrintSelectedArticles");
        }
        public async void PrintSelectedArticles(object input = null)
        {
            await this.pdfCreator.Print(this.ArticlesToBePrinted);
            CancelSelectingArticlesToPrint();
            this.services.ShowNotification("Print saved as Pdf", "Printing", NotificationType.Success, NotificationAreaTypes.Default, new TimeSpan(0, 0, 2));
        }
        #endregion

        #region Command validators
        public bool CanSearch(object input = null)
        {
            return true;
        }
        public bool CanNextPage(object input = null)
        {
            return this.CurrentPage < this.TotalPages;
        }
        public bool CanPreviousPage(object input = null)
        {
            return this.CurrentPage > 1;
        }
        #endregion

        #region Private helpers
        private void PopulateArticles()
        {
            List<Article> freshArticles = new List<Article>();
            int offset = (this.CurrentPage - 1) * this.ItemsPerPage;
            for (int i = offset; i < this.CurrentPage * this.ItemsPerPage; i++)
            {
                if (i > this.articles.Count - 1)
                {
                    break;
                }
                freshArticles.Add(this.articles[i]);
            }
            this.Articles.Clear();
            foreach (Article article in freshArticles)
            {
                this.Articles.Add(article);
            }
            //this.Articles = freshArticles;
            //OnPropertyChanged("Articles");
            OnPropertyChanged("ResultsCount");
            OnScrollTopRequested();
        }
        private async void SortArticles()
        {
            this.services.IsWorking(true);
            await Task.Run(() =>
            {
                if (SortString == "Title ASC")
                {
                    this.articles = this.articles.OrderBy(o => o.Title).ToList();
                }
                else if (SortString == "Title DESC")
                {
                    this.articles = this.articles.OrderByDescending(o => o.Title).ToList();
                }
                else if (SortString == "Year ASC")
                {
                    this.articles = this.articles.OrderBy(o => o.Year).ToList();
                }
                else if (SortString == "Year DESC")
                {
                    this.articles = this.articles.OrderByDescending(o => o.Year).ToList();
                }
                else if (SortString == "Date ASC")
                {
                    this.articles = this.articles.OrderBy(o => o.ID).ToList();
                }
                else if (SortString == "Date DESC")
                {
                    this.articles = this.articles.OrderByDescending(o => o.ID).ToList();
                }
                // We want to reset the page back to 1 when we change sort
                this.CurrentPage = 1;
            });
            PopulateArticles();
            this.services.IsWorking(false);
        }
        private static string FormatText(string input)
        {
            if (String.IsNullOrWhiteSpace(input))
            {
                return "";
            }

            return TextFormat.RemoveUnprintableCharacters(TextFormat.RemoveSpareWhiteSpace(TextFormat.RemoveLineBreaks(input)));
        }
        #endregion
    }
}
