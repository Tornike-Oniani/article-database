using Lib.DataAccessLayer.Info;
using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Main;
using MainLib.ViewModels.Popups;
using MainLib.ViewModels.Utils;
using NotificationService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace MainLib.ViewModels.Pages
{
    public class ReferenceViewViewModel : BaseViewModel
    {
        // Property fields
        private ViewTemplate _selectedDataViewType;
        private bool _isViewCompact;
        private User _user;
        private List<string> _columns;
        private string _selectedSortProperty;
        private string _selectedSortDirection;
        private bool _isExportEnabled;

        // Public properties
        public ViewTemplate SelectedViewType
        {
            get { return _selectedDataViewType; }
            set { _selectedDataViewType = value; OnPropertyChanged("SelectedViewType"); }
        }
        public bool IsViewCompact
        {
            get { return _isViewCompact; }
            set { _isViewCompact = value; OnPropertyChanged("IsViewCompact"); }
        }
        public Reference Reference { get; set; }
        public ObservableCollection<Article> Articles { get; set; }
        public ArticleDataManager ArticleDataManager { get; set; }
        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }
        public List<string> Columns
        {
            get { return _columns; }
            set { _columns = value; OnPropertyChanged("Columns"); }
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
        public Shared Services { get; set; }

        public CollectionViewSource _articlesCollection { get; set; }
        public ICollectionView ArticlesCollection { get { return _articlesCollection.View; } }

        // Commands
        public ICommand SwitchDataViewCommand { get; set; }
        public ICommand LoadArticlesCommand { get; set; }
        public ICommand OpenMainArticleCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand ExportReferenceCommand { get; set; }
        public ICommand EnableExportCommand { get; set; }
        public ICommand UpdateExportStatusCommand { get; set; }
        public ICommand SortFromRibbonCommand { get; set; }
        public ICommand SortFromDataGridCommand { get; set; }

        // Constructor
        public ReferenceViewViewModel(Reference reference)
        {
            this.Services = Shared.GetInstance();
            this.Columns = new List<string>();
            this.Reference = reference;
            this.User = Services.User;
            this.Articles = new ObservableCollection<Article>();
            this.IsViewCompact = true;
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

            SelectedSortProperty = SortableProperties[0];
            SelectedSortDirection = SortDirections[0];

            // Initialize commands
            this.SwitchDataViewCommand = new RelayCommand(SwitchDataView);
            this.LoadArticlesCommand = new RelayCommand(LoadArticles);
            this.OpenMainArticleCommand = new RelayCommand(OpenMainArticle);
            this.ExportCommand = new RelayCommand(Export, CanExport);
            this.ExportReferenceCommand = new RelayCommand(ExportReference, CanExportReference);
            this.EnableExportCommand = new RelayCommand(EnableExport);
            this.UpdateExportStatusCommand = new RelayCommand(UpdateExportStatus);
            this.SortFromRibbonCommand = new RelayCommand(SortFromRibbon);
            this.SortFromDataGridCommand = new RelayCommand(SortFromDataGrid);

            this.ArticleDataManager = new ArticleDataManager(LoadArticlesCommand);
            this.SelectedViewType = new CompactViewTemplate();
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
            await PopulateReferenceArticles();
        }
        public async Task PopulateReferenceArticles()
        {
            try
            {
                Services.IsWorking(true);

                Articles.Clear();

                List<Article> articles = new List<Article>();

                await Task.Run(() =>
                {
                    foreach (Article article in (new ReferenceRepo()).LoadArticlesForReference(Reference))
                        articles.Add(article);
                });

                foreach (Article article in articles)
                    Articles.Add(article);

                Services.IsWorking(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Reference View", "Populate references", e.Message, e.StackTrace);
                Services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                Services.IsWorking(false);
            }
        }
        public void OpenMainArticle(object input = null)
        {
            // 1. If main article is not set return
            if (Reference.Article == null || Reference.ArticleID == null)
                return;

            // 2. Get full path of the file
            string full_path = Environment.CurrentDirectory + "\\Files\\" + Reference.Article.FileName + ".pdf";

            // 3. Open file with default program
            try
            {
                Process.Start(full_path);
            }

            // 4. Catch if file doesn't exist physically
            catch
            {
                Services.DialogService.OpenDialog(new DialogOkViewModel("File was not found", "Error", DialogType.Error));
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

                    //services.DialogService.OpenDialog(new DialogOkViewModel("Done", "Message", DialogType.Success));
                    Services.ShowNotification("Export", $"Exported {exportedArticlesCount} files successfully.", NotificationType.Success, "ReferenceNotificationArea", new TimeSpan(0, 0, 3));
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Reference View", "Export", e.Message, e.StackTrace);
                Services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                Services.IsWorking(false);
            }
        }    
        public async void ExportReference(object input = null)
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

                    Services.DialogService.OpenDialog(new DialogOkViewModel("Done", "Result", DialogType.Success));
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Bookmark View", "Export Bookmark", e.Message, e.StackTrace);
                Services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
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
        // Command validators
        public bool CanExport(object input = null)
        {
            List<Article> checked_articles = Articles.Where(article => article.Checked == true).ToList();
            if (checked_articles.Count > 0)
                return true;

            return false;
        }
        public bool CanExportReference(object input = null)
        {
            return Articles.Count > 0;
        }
    }
}
