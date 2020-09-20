using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Main;
using Lib.ViewModels.Services.Dialogs;

namespace MainLib.ViewModels.Popups
{
    public class ArticleEditorViewModel : BaseViewModel
    {
        private string _author;
        private string _keyword;
        private string _selectedFile;
        private IDialogService _dialogService;

        public DataViewViewModel Parent { get; set; }
        public Article Article { get; set; }
        public string Author
        {
            get { return _author; }
            set { _author = value; OnPropertyChanged("Author"); }
        }
        public string Keyword
        {
            get { return _keyword; }
            set { _keyword = value; OnPropertyChanged("Keyword"); }
        }
        public string SelectedFile
        {
            get { return _selectedFile; }
            set { _selectedFile = value; OnPropertyChanged("SelectedFile"); }
        }

        public RelayCommand SelectFileCommand { get; set; }
        public RelayCommand UpdateArticleCommand { get; set; }

        public ArticleEditorViewModel(DataViewViewModel parent, IDialogService dialogService)
        {
            this.Title = "Edit Article";
            this._dialogService = dialogService;

            // 1. Set parent view model
            Parent = parent;

            // 2. Create article instance
            Article = new Article();

            // 3. Copy article properties from parent's selected article
            Article.CopyByValue(Parent.SelectedArticle, true, false);

            // 4. Initialize commands
            SelectFileCommand = new RelayCommand(SelectFile);
            UpdateArticleCommand = new RelayCommand(UpdateArticle);
        }

        public void SelectFile(object input = null)
        {
            // 1. Create new dialog window
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // 2. Set filter for file extension and default file extension 
            dlg.DefaultExt = ".pdf";
            dlg.Filter = "PDF files (*.pdf)|*.pdf";


            // 3. Display the dialog window
            Nullable<bool> result = dlg.ShowDialog();


            // 4. Get the selected file
            if (result == true)
            {
                SelectedFile = dlg.FileName;
            }
        }
        public void UpdateArticle(object input)
        {
            try
            {
                // 1. Update article record in database
                (new ArticleRepo()).UpdateArticle(Article, Parent.User);

                // 2. If new file was selected overwrite it to older one
                if (SelectedFile != null)
                {
                    File.Copy(SelectedFile, Path.Combine(Environment.CurrentDirectory, "Files\\") + Article.FileName + ".pdf", true);
                }

                // 3. Copy new article properties to parent's selected article (so that the values will be updated on data grid)
                Parent.SelectedArticle.CopyByValue(Article, false, true);
            }

            catch (Exception error)
            {
                // Message = "constraint failed\r\nUNIQUE constraint failed: tblArticle.Title"
                if (error.Message.Contains("UNIQUE") && error.Message.Contains("Title"))
                {
                    _dialogService.OpenDialog(new DialogOkViewModel("Article with that name already exists", "Duplicate", DialogType.Warning));

                }
                else
                {
                    _dialogService.OpenDialog(new DialogOkViewModel(error.Message, "General Exception", DialogType.Error));
                }


            }

            // Close window
            (input as ICommand).Execute(null);
        }
    }
}
