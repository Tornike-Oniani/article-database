using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleDatabase.ViewModels.Pages
{
    public class BookmarkViewViewModel : BaseViewModel
    {
        /**
         * Private members
         */
        private User _user;
        private bool _modifyRights;
        private List<string> _columns;

        /**
         * Public properties:
         *  - Bookmark
         *  - Selected article
         *  [Selected article in data grid]
         */
        public Bookmark Bookmark { get; set; }
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


        /**
         * Commands:
         *  - Open file
         *  [Opens selected file in .pdf browser]
         *  - Export bookmark
         *  [Exports all files in bookmark into selected folder]
         *  - Remove article
         *  [Removes article from bookmark]
         *  - Copy
         *  [Copy's article name to clipboar]
         */
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand ExportBookmarkCommand { get; set; }
        public RelayCommand ExportCommand { get; set; }
        public RelayCommand RemoveArticleCommand { get; set; }
        public RelayCommand CopyCommand { get; set; }

        // Constructor
        public BookmarkViewViewModel(Bookmark bookmark, User user, bool modifyRights = true)
        {
            Columns = new List<string>();
            this.Bookmark = bookmark;
            this._user = user;
            this.ModifyRights = modifyRights;

            // Initialize commands
            OpenFileCommand = new RelayCommand(OpenFile);
            ExportBookmarkCommand = new RelayCommand(ExportBookmark, CanExportBookmark);
            ExportCommand = new RelayCommand(Export, CanExport);
            RemoveArticleCommand = new RelayCommand(RemoveArticle, CanOnSelectedArticle);
            CopyCommand = new RelayCommand(Copy, CanOnSelectedArticle);
        }

        /**
         * Command actions
         */
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
                MessageBox.Show("File was not found");
            }
        }
        public void Export(object input = null)
        {
            // Destination will be the path chosen from dialog box (Where files should be exported)
            string destination = null;

            // 1. Open forms folder dialog box
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    // Set the destination to dialog box's result
                    destination = fbd.SelectedPath;
                }
            }

            // If path was chosen from the dialog box
            if (destination != null)
            {
                // 2. Get the list of articles which were checked for export
                List<Article> checked_articles = Bookmark.Articles.Where(article => article.Checked == true).ToList();

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

                foreach (Article article in Bookmark.Articles)
                    article.Checked = false;

                MessageBox.Show("Done", "Message");
            }
        }
        public bool CanExport(object input = null)
        {
            List<Article> checked_articles = Bookmark.Articles.Where(article => article.Checked == true).ToList();
            if (checked_articles.Count > 0)
                return true;

            return false;
        }
        public void ExportBookmark(object input = null)
        {
            // Destination will be the path chosen from dialog box (Where files should be exported)
            string destination = null;

            // 1. Open forms folder dialog box
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    // Set the destination to dialog box's result
                    destination = fbd.SelectedPath;
                }
            }

            // If path was chosen from the dialog box
            if (destination != null)
            {
                // Export each article in bookmark's article collection
                foreach (Article article in Bookmark.Articles)
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

                MessageBox.Show("Done", "Message");
            }
        }
        public bool CanExportBookmark(object input = null)
        {
            return Bookmark.Articles.Count > 0;
        }
        public void RemoveArticle(object input = null)
        {
            // 1. Remove article from bookmark in database
            SqliteDataAccess.RemoveArticleFromBookmark(Bookmark, SelectedArticle);

            // 2. Refresh articles collection
            Bookmark.PopulateArticles(_user);
        }
        public void Copy(object input = null)
        {
            Clipboard.SetText(SelectedArticle.Title);
        }
        public bool CanOnSelectedArticle(object input = null)
        {
            return SelectedArticle != null;
        }
    }
}
