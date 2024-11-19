using Lib.DataAccessLayer.Models;
using Lib.ViewModels.Commands;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace MainLib.Views.Popups
{
    /// <summary>
    /// Interaction logic for ReportsViewer.xaml
    /// </summary>
    public partial class ReportsViewer : UserControl
    {
        private FixedDocument doc;
        private double a4Width = 793.92;
        private double a4Height = 1122.24;
        private List<Article> articlesToBePrinted;
        private PageContent content;
        private FixedPage page;
        private List<Article> currentPageArticles;
        private ListView listView;
        private Grid wrapper;

        public  ICommand PrintCommand { get; set; }

        public ReportsViewer()
        {
            InitializeComponent();

            this.PrintCommand = new RelayCommand(Print);

            this.Loaded += (s, e) =>
            {
                this.articlesToBePrinted = ResultsList.ItemsSource as List<Article>;
            };
        }

        public void Print(object input)
        {
            this.doc = new FixedDocument();
            CreateNewPage();

            double occupiedSpace = 0;

            for (int i = 0; i < articlesToBePrinted.Count; i++)
            {
                var item = ResultsList.Items[i];
                ListViewItem container = ResultsList.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;

                if (container != null && container.IsLoaded) 
                {
                    double height = container.ActualHeight;
                    occupiedSpace += height;
                    // If current page can contain the item then add it
                    if (occupiedSpace <= a4Height)
                    {
                        currentPageArticles.Add(articlesToBePrinted[i]);
                        // We have to account for margin between items as well
                        occupiedSpace += 12;
                    }
                    // Otherwise create a new page with separate listview and add the item there
                    else
                    {
                        occupiedSpace = container.ActualHeight;
                        CreateNewPage();
                        this.currentPageArticles.Add(articlesToBePrinted[i]);
                        // We have to account for margin between items as well
                        occupiedSpace += 12;
                    }
                }
            }


            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                IDocumentPaginatorSource idpSource = doc;

                printDialog.PrintDocument(idpSource.DocumentPaginator, "Sample printing");
            }

            //TextBlock textBlock = new TextBlock
            //{
            //    Text = "Page 1",
            //    FontSize = 16
            //};

            //page.Children.Add(textBlock);
            //content.Child = page;
            //doc.Pages.Add(content);

            //content = new PageContent();
            //page = new FixedPage();
            //textBlock = new TextBlock
            //{
            //    Text = "Page 2",
            //    FontSize = 16
            //};

            //page.Children.Add(textBlock);
            //content.Child = page;
            //doc.Pages.Add(content);

            //foreach (var item in ResultsList.Items)
            //{
            //    ListViewItem container = ResultsList.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;

            //    if (container != null && container.IsLoaded) 
            //    {
            //        double height = container.ActualHeight;
            //        Console.WriteLine("Item: {0}", height);
            //    }
            //}

            //FlowDocument doc = input as FlowDocument;

            //PrintDialog printDialog = new PrintDialog();

            //if (printDialog.ShowDialog() == true)
            //{
            //    PrintQueue printQueue = printDialog.PrintQueue;
            //    IDocumentPaginatorSource idpSource = doc;

            //    printDialog.PrintDocument(idpSource.DocumentPaginator, "Sample printing");
            //}
        }

        public void CreateNewPage()
        {
            this.content = new PageContent();
            this.page = new FixedPage();
            this.page.Height = a4Height;
            this.page.Width = a4Width;

            this.currentPageArticles = new List<Article>();

            this.listView = new ListView();
            this.listView.Style = Resources["PrintingStyle"] as Style;
            this.listView.ItemsSource = currentPageArticles;
            this.wrapper = new Grid { Width = a4Width };

            this.wrapper.Children.Add(listView);
            this.page.Children.Add(wrapper);
            this.content.Child = page;
            this.doc.Pages.Add(content);
        }
    }
}
