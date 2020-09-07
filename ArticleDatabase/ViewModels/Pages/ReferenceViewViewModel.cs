using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleDatabase.ViewModels.Pages
{
    public class ReferenceViewViewModel : BaseViewModel
    {
        private User _user;

        /**
         * Public properties:
         *  - Bookmark
         *  - Selected article
         *  [Selected article in data grid]
         */
        public Reference Reference { get; set; }
        public Article SelectedArticle { get; set; }
        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
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
        public RelayCommand RemoveArticleCommand { get; set; }
        public RelayCommand CopyCommand { get; set; }

        // Constructor
        public ReferenceViewViewModel(Reference reference, User user)
        {
            this.Reference = reference;
            this.User = user;

            // Initialize commands
            OpenFileCommand = new RelayCommand(OpenFile);
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
        public void RemoveArticle(object input = null)
        {
            // 1. Remove article from bookmark in database
            (new ReferenceRepo()).RemoveArticleFromReference(Reference, SelectedArticle);

            // 2. Refresh articles collection
            Reference.PopulateArticles();
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
