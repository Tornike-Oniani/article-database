using Lib.DataAccessLayer.Models;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Classes;
using NotificationService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainLib.ViewModels.Utils
{
    public class PrintHelper : INotifyPropertyChanged
    {
        #region Private members
        private readonly Shared services = Shared.GetInstance();
        private readonly PdfCreator pdfCreator = new PdfCreator();
        #endregion

        #region Property fields
        private bool _isSelectingArticlesForPrinting;
        #endregion

        #region Public properties
        public List<Article> ArticlesToBePrinted { get; set; } = new List<Article>();
        public bool IsSelectingArticlesForPrinting
        {
            get { return _isSelectingArticlesForPrinting; }
            set { _isSelectingArticlesForPrinting = value; OnPropertyChanged("IsSelectingArticlesForPrinting"); }
        }
        public bool CanPrintSelectedArticles
        {
            get
            {
                return this.ArticlesToBePrinted.Count > 0;
            }
        }
        #endregion

        #region Commands
        public ICommand SelectArticlesToPrintCommand { get; set; }
        public ICommand CancelSelectingArticlesToPrintCommand { get; set; }
        public ICommand MarkArticleForPrintCommand { get; set; }
        public ICommand PrintSelectedArticlesCommand { get; set; }
        #endregion

        #region Constructors
        public PrintHelper()
        {
            this.SelectArticlesToPrintCommand = new RelayCommand(SelectArticlesToPrint);
            this.PrintSelectedArticlesCommand = new RelayCommand(PrintSelectedArticles);
            this.MarkArticleForPrintCommand = new RelayCommand(MarkArticleForPrint);
            this.CancelSelectingArticlesToPrintCommand = new RelayCommand(CancelSelectingArticlesToPrint);
        }
        #endregion

        #region Command actions
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
            if (this.ArticlesToBePrinted.Exists(a => a.ID == article.ID))
            {
                Article articleToRemove = this.ArticlesToBePrinted.SingleOrDefault(a => a.ID == article.ID);
                this.ArticlesToBePrinted.Remove(articleToRemove);
            }
            else
            {
                this.ArticlesToBePrinted.Add(article);
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

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
