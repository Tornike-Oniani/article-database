using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Utils;
using NotificationService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace MainLib.ViewModels.Pages
{
    public class BookmarkViewViewModel : BaseViewModel
    {
        // Property fields
        private bool _modifyRights;
        private List<string> _columns;
        private bool _isViewCompact;
        private string _selectedSortProperty;
        private string _selectedSortDirection;
        private bool _isExportEnabled;
        private ViewTemplate _selectedViewType;

        // Public properties
        public Bookmark Bookmark { get; set; }
        public ObservableCollection<Article> Articles { get; set; }
        public ArticleDataManager ArticleDataManager { get; set; }
        public List<string> Columns
        {
            get { return _columns; }
            set { _columns = value; OnPropertyChanged("Columns"); }
        }
        public bool ModifyRights
        {
            get { return _modifyRights; }
            set { _modifyRights = value; OnPropertyChanged("ModifyRights"); }
        }
        public bool CanExportP
        {
            get { return Articles.Where(article => article.Checked == true).ToList().Count != 0; }
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
        public bool IsExportEnabled
        {
            get { return _isExportEnabled; }
            set { _isExportEnabled = value; OnPropertyChanged("IsExportEnabled"); }
        }
        public string SelectedSort { get { return SelectedSortProperty + " " + SelectedSortDirection; } }
        public ViewTemplate SelectedViewType
        {
            get { return _selectedViewType; }
            set { _selectedViewType = value; OnPropertyChanged("SelectedViewType"); }
        }
        public bool IsViewCompact
        {
            get { return _isViewCompact; }
            set { _isViewCompact = value; OnPropertyChanged("IsViewCompact"); }
        }
        public Shared Services { get; set; }
        public User User { get; set; }

        public CollectionViewSource _articlesCollection { get; set; }
        public ICollectionView ArticlesCollection { get { return _articlesCollection.View; } }

        // Commands
        public ICommand SwitchDataViewCommand { get; set; }
        public ICommand LoadArticlesCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand ExportBookmarkCommand { get; set; }
        public ICommand EnableExportCommand { get; set; }
        public ICommand UpdateExportStatusCommand { get; set; }
        public ICommand SortFromRibbonCommand { get; set; }
        public ICommand SortFromDataGridCommand { get; set; }
        public ICommand RemoveSortCommand { get; set; }

        // Constructor
        public BookmarkViewViewModel(Bookmark bookmark, bool modifyRights = true)
        {
            this.Services = Shared.GetInstance();
            this.Bookmark = bookmark;
            this.User = Services.User;
            this.Articles = new ObservableCollection<Article>();
            this.ModifyRights = modifyRights;
            this.IsExportEnabled = false;

            _articlesCollection = new CollectionViewSource();
            _articlesCollection.Source = Articles;

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

            //SelectedSortProperty = SortableProperties[0];
            //SelectedSortDirection = SortDirections[0];

            // Initialize commands
            this.LoadArticlesCommand = new RelayCommand(LoadArticles);
            this.ExportCommand = new RelayCommand(Export, CanExport);
            this.ExportBookmarkCommand = new RelayCommand(ExportBookmark, CanExportBookmark);
            this.EnableExportCommand = new RelayCommand(EnableExport);
            this.UpdateExportStatusCommand = new RelayCommand(UpdateExportStatus);
            this.SwitchDataViewCommand = new RelayCommand(SwitchDataView);
            this.SortFromRibbonCommand = new RelayCommand(SortFromRibbon);
            this.SortFromDataGridCommand = new RelayCommand(SortFromDataGrid);
            this.RemoveSortCommand = new RelayCommand(RemoveSort);

            this.ArticleDataManager = new ArticleDataManager(LoadArticlesCommand);
            this.SelectedViewType = new CompactViewTemplate();
            this.IsViewCompact = true;
        }

        // Command actions
        public void SwitchDataView(object input)
        {
            string view = input as string;
            if (view.Contains("Compact"))
            {
                SelectedViewType = new CompactViewTemplate();
                IsViewCompact = true;
            }
            else
            {
                SelectedViewType = new FullViewTemplate();
                IsViewCompact = false;
            }

        }
        public async void LoadArticles(object input = null)
        {
            await PopulateBookmarkArticles();
        }
        public async Task PopulateBookmarkArticles()
        {
            try
            {
                Services.IsWorking(true);

                Articles.Clear();

                List<Article> articles = new List<Article>();

                await Task.Run(() =>
                {
                    foreach (Article article in new BookmarkRepo().LoadArticlesForBookmark(User, this.Bookmark))
                        articles.Add(article);
                });

                foreach (Article article in articles)
                    Articles.Add(article);

                Services.IsWorking(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Bookmark View", "Populate bookmark articles", e.Message, e.StackTrace);
                Services.ShowDialogWithOverlay(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                Services.IsWorking(false);
            }
        }
        public void EnableExport(object input)
        {
            foreach (Article article in Articles)
                article.Checked = false;
            IsExportEnabled = !IsExportEnabled;
        }
        public async void Export(object input = null)
        {
            try
            {
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

                    int exportedArticlesCount = 0;

                    // 3. Uncheck articles
                    foreach (Article article in Articles)
                    {
                        article.Checked = false;
                        exportedArticlesCount++;
                    }

                    Services.IsWorking(false);

                    UpdateExportStatus();

                    Services.ShowNotification("Export", $"Exported {exportedArticlesCount} files successfully.", NotificationType.Success, "BookmarkNotificationArea", new TimeSpan(0, 0, 3));
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Bookmark View", "Export", e.Message, e.StackTrace);
                Services.ShowDialogWithOverlay(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                Services.IsWorking(false);
            }
        }
        public async void ExportBookmark(object input = null)
        {
            try
            {
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
                        // Export each article in bookmark's article collection
                        foreach (Article article in Articles)
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

                    Services.IsWorking(false);

                    Services.ShowDialogWithOverlay(new DialogOkViewModel("Done", "Result", DialogType.Success));
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Bookmark View", "Export Bookmark", e.Message, e.StackTrace);
                Services.ShowDialogWithOverlay(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                Services.IsWorking(false);
            }

        }
        public void UpdateExportStatus(object input = null)
        {
            OnPropertyChanged("CanExportP");
        }
        public void SortFromRibbon(object input = null)
        {
            if (Articles.Count == 0)
                return;

            ArticlesCollection.SortDescriptions.Clear();
            ArticlesCollection.SortDescriptions.Add(new SortDescription(SelectedSortProperty, SelectedSortDirection == "ASC" ? ListSortDirection.Ascending : ListSortDirection.Descending));

            OnPropertyChanged("SelectedSort");
        }
        public void SortFromDataGrid(object input)
        {
            string header = input.ToString();
            header = header == "Comment" ? "PersonalComment" : header;

            // Ignore columns
            if (header == "Authors" || header == "Keywords" || header == "Export" || header == "Bookmark")
                return;

            if (Articles.Count == 0)
                return;

            if (SelectedSort.Contains(header))
            {
                if (SelectedSort.Contains("ASC"))
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
            OnPropertyChanged("SelectedSort");
            SortFromRibbonCommand.Execute(null);
        }
        public bool CanExport(object input = null)
        {
            List<Article> checked_articles = Articles.Where(article => article.Checked == true).ToList();
            if (checked_articles.Count > 0)
                return true;

            return false;
        }
        public bool CanExportBookmark(object input = null)
        {
            return Articles.Count > 0;
        }
        public void RemoveSort(object input = null)
        {
            ArticlesCollection.SortDescriptions.Clear();
        }
    }
}
