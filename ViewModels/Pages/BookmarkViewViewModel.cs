using Lib.DataAccessLayer.Info;
using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Popups;
using MainLib.ViewModels.Utils;
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

namespace MainLib.ViewModels.Pages
{
    public class BookmarkViewViewModel : BaseViewModel
    {
        /**
         * Private members
         */
        private User _user;
        private bool _modifyRights;
        private List<string> _columns;
        private Shared services;

        /**
         * Public properties:
         *  - Bookmark
         *  - Selected article
         *  [Selected article in data grid]
         */
        public Bookmark Bookmark { get; set; }
        public ObservableCollection<Article> Articles { get; set; }
        public Article SelectedArticle { get; set; }
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
        public Shared Services { get; set; }
        public User User { get; set; }

        /**
         * Commands:
         *  - Open file
         *  [Opens selected file in .pdf browser]
         *  - Export bookmark
         *  [Exports all files in bookmark into selected folder]
         *  - Remove article
         *  [Removes article from bookmark]
         *  - Copy
         *  [Copy's article name to clipboard]
         */
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand ExportCommand { get; set; }
        public RelayCommand ExportBookmarkCommand { get; set; }
        public RelayCommand RemoveArticleCommand { get; set; }
        public RelayCommand CopyCommand { get; set; }
        public ICommand EnableExportCommand { get; set; }
        public ICommand UpdateExportStatusCommand { get; set; }
        public RelayCommand OpenAddPersonalCommand { get; set; }
        public RelayCommand OpenEditCommand { get; set; }
        public ICommand OpenAbstractEditorCommand { get; set; }

        // Constructor
        public BookmarkViewViewModel(Bookmark bookmark, bool modifyRights = true)
        {
            this.services = Shared.GetInstance();
            this.Services = Shared.GetInstance();
            this.Columns = new List<string>();
            this.Bookmark = bookmark;
            this._user = services.User;
            this.User = services.User;
            this.Articles = new ObservableCollection<Article>();
            this.ModifyRights = modifyRights;

            // Initialize commands
            OpenFileCommand = new RelayCommand(OpenFile);
            ExportCommand = new RelayCommand(Export, CanExport);
            ExportBookmarkCommand = new RelayCommand(ExportBookmark, CanExportBookmark);
            RemoveArticleCommand = new RelayCommand(RemoveArticle, CanOnSelectedArticle);
            CopyCommand = new RelayCommand(Copy, CanOnSelectedArticle);
            EnableExportCommand = new RelayCommand(EnableExport);
            UpdateExportStatusCommand = new RelayCommand(UpdateExportStatus);
            OpenAddPersonalCommand = new RelayCommand(OpenAddPersonal, IsArticleSelected);
            OpenEditCommand = new RelayCommand(OpenEditDialog, IsArticleSelected);
            OpenAbstractEditorCommand = new RelayCommand(OpenAbstractEditor);
        }

        /**
         * Command actions
         */
        public async Task PopulateBookmarkArticles()
        {
            try
            {
                services.IsWorking(true);

                Articles.Clear();

                List<Article> articles = new List<Article>();

                await Task.Run(() =>
                {
                    foreach (Article article in new BookmarkRepo().LoadArticlesForBookmark(_user, this.Bookmark))
                        articles.Add(article);
                });

                foreach (Article article in articles)
                    Articles.Add(article);

                services.IsWorking(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Bookmark View", "Populate bookmark articles", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
            }
        }
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
                services.DialogService.OpenDialog(new DialogOkViewModel("File was not found", "Error", DialogType.Error));
            }
        }
        public void EnableExport(object input)
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

                    services.IsWorking(false);

                    UpdateExportStatus();

                    //services.DialogService.OpenDialog(new DialogOkViewModel("Done", "Message", DialogType.Success));
                    services.ShowNotification("Export", $"Exported {exportedArticlesCount} files successfully.", NotificationType.Success, "BookmarkNotificationArea", new TimeSpan(0, 0, 3));
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Bookmark View", "Export", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
            }
        }
        public async void ExportBookmark(object input = null)
        {
            try
            {
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

                    services.IsWorking(false);

                    services.DialogService.OpenDialog(new DialogOkViewModel("Done", "Result", DialogType.Success));
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Bookmark View", "Export Bookmark", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
            }

        }
        public async void RemoveArticle(object input = null)
        {
            try
            {
                // 1. Remove article from bookmark in database
                new BookmarkRepo().RemoveArticleFromBookmark(Bookmark, SelectedArticle);

                // 1.1 Track removing article from bookmark
                Couple info = new Couple("Bookmark", "Remove", SelectedArticle.Title, Bookmark.Name);
                new Tracker(_user).TrackCoupling<Couple>(info);

                // 2. Refresh articles collection
                await PopulateBookmarkArticles();
            }
            catch (Exception e)
            {
                new BugTracker().Track("Bookmark View", "Remove article", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
        }
        public void Copy(object input = null)
        {
            Clipboard.SetText(SelectedArticle.Title);
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
        public bool CanOnSelectedArticle(object input = null)
        {
            return SelectedArticle != null;
        }
        public void UpdateExportStatus(object input = null)
        {
            OnPropertyChanged("CanExportP");
        }
        public void OpenEditDialog(object input = null)
        {
            services.WindowService.OpenWindow(new MainLib.ViewModels.Popups.ArticleEditorViewModel(SelectedArticle));
        }
        public void OpenAbstractEditor(object input)
        {
            services.WindowService.OpenWindow(new AbstractEditorViewModel(SelectedArticle), passWindow: true);
        }
        public void OpenAddPersonal(object input)
        {
            services.WindowService.OpenWindow(new MainLib.ViewModels.Popups.AddPersonalDialogViewModel(SelectedArticle));
        }
        public bool IsArticleSelected(object input = null)
        {
            if (SelectedArticle != null)
                return true;

            return false;
        }
    }
}
