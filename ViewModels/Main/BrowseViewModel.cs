using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.DataAccessLayer.Utils;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
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
using System.Windows.Input;

namespace MainLib.ViewModels.Main
{
    public class BrowseViewModel : BaseViewModel
    {
        #region Private members
        private string[] dudWords = new string[] { "in", "of", "at", "to", "into", "on", "onto", "a", "the", "and" };
        private List<Article> articles = new List<Article>();
        private Shared services;
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
        // Sorting
        public string SortString
        {
            get { return _sortString; }
            set { _sortString = value; OnPropertyChanged("SortString"); }
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
        #endregion

        #region Constructors
        public BrowseViewModel()
        {
            // Generic
            this.services = Shared.GetInstance();
            this.User = Shared.GetInstance().User;
            this.Articles = new ObservableCollection<Article>();

            // Pagination
            this.ItemsPerPage = 25;
            this.CurrentPage = 1;

            // Sorting
            this.SortString = "Title ASC";

            // Commands
            this.SearchCommand = new RelayCommand(Search, CanSearch);
            this.NextPageCommand = new RelayCommand(NextPage, CanNextPage);
            this.PreviousPageCommand = new RelayCommand(PreviousPage, CanPreviousPage);
            this.SortCommand = new RelayCommand(Sort);
            this.ClearCommand = new RelayCommand(Clear);
            this.OpenFileCommand = new RelayCommand(OpenFile);
            this.DownloadFileCommand = new RelayCommand(DownloadFile);
            this.ChangeItemsPerPageCommand = new RelayCommand(ChangeItemsPerPage);
        }
        #endregion

        #region Command actions
        public async void Search(object input = null)
        {
            // Extract words and phrases from terms (they are distinguished by if they are inside brackets or not)
            List<string> termWords = new List<string>();
            List<string> termPhrases = new List<string>();
            if (!String.IsNullOrWhiteSpace(Terms))
            {
                termWords = Regex.Matches(Terms, @"\[(.*?)\]").Cast<Match>().Select(m => m.Value.Substring(1, m.Value.Length - 2)).ToList();
                termPhrases = Regex.Replace(Terms, @"\[(.*?)\]", "").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Except(dudWords).ToList();
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
                this.articles = new ArticleRepo().LoadArticles(User, filter);
            });
            services.IsWorking(false);

            PopulateArticles();
            this.CurrentPage = 1;
            this.ShowResults = true;
            OnPropertyChanged("TotalPages");
            OnPropertyChanged("ShowNoResultsLabel");
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
            this.Articles.Clear();
            this.Terms = String.Empty;
            this.Authors = String.Empty;
            this.Year = String.Empty;
            this.TermsWordsHighlight.Clear();
            this.TermsPhrasesHighlight.Clear();
            this.CurrentPage = 1;
            this.articles.Clear();
            this.Articles.Clear();
            this.ShowResults = false;
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
                this.services.DialogService.OpenDialog(new DialogOkViewModel("File was not found", "Error", DialogType.Error));
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
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
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
            this.Articles.Clear();
            int offset = (this.CurrentPage - 1) * this.ItemsPerPage;
            for (int i = offset; i < this.CurrentPage * this.ItemsPerPage; i++)
            {
                if (i > this.articles.Count - 1)
                {
                    break;
                }
                this.Articles.Add(this.articles[i]);
            }
        }
        private void SortArticles()
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
            PopulateArticles();
        }
        private string FormatText(string input)
        {
            if (String.IsNullOrWhiteSpace(input))
            {
                return "";
            }

            return TextFormat.RemoveUnprintableCharacters(TextFormat.RemoveSpareWhiteSpace(input));
        }
        #endregion
    }
}
