using MainLib.ViewModels.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainLib.Views.Components
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
            BrowseViewModel dataContext = this.DataContext as BrowseViewModel;
            dataContext.ScrollToTopRequested += DataContext_ScrollToTopRequested;
        }

        private void DataContext_ScrollToTopRequested(object sender, EventArgs e)
        {
            _resultsListScrollViewer.ScrollToTop();
        }
    }
}
