using MainLib.ViewModels.Main;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MainLib.Views.Main.Components.Browse
{
    /// <summary>
    /// Interaction logic for BrowseResults.xaml
    /// </summary>
    public partial class BrowseResults : UserControl
    {
        private ScrollViewer _resultsListScrollViewer;

        public BrowseResults()
        {
            InitializeComponent();
            this.Loaded += Browse_Loaded;
        }

        private void Browse_Loaded(object sender, RoutedEventArgs e)
        {
            this._resultsListScrollViewer = (ScrollViewer)ResultsList.Template.FindName("PART_ScrollViewer", ResultsList);

            if (this._resultsListScrollViewer == null)
            {
                return;
            }

            BrowseViewModel dataContext = this.DataContext as BrowseViewModel;
            dataContext.ScrollToTopRequested += DataContext_ScrollToTopRequested;
        }

        private void DataContext_ScrollToTopRequested(object sender, EventArgs e)
        {
            _resultsListScrollViewer.ScrollToTop();
        }
    }
}
