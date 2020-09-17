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
using ViewModels.Main;
using ViewModels.Pages;
using Views.Pages;
using Views.Services.Dialogs;
using Views.Services.Windows;

namespace Views.Main
{
    /// <summary>
    /// Interaction logic for Bookmarks.xaml
    /// </summary>
    public partial class Bookmarks : UserControl
    {
        public Bookmarks()
        {
            InitializeComponent();

            this.Loaded += (object sender, RoutedEventArgs e) =>
            {
                if (this.DataContext is BookmarksViewModel)
                {

                    BookmarkListViewModel view_model = new BookmarkListViewModel(((BookmarksViewModel)this.DataContext).User, new DialogService(), new WindowService());
                    Page _mainPage = new BookmarkList(view_model);
                    _mainPage.DataContext = view_model;
                    _mainFrame.Navigate(_mainPage);
                }
                else if (this.DataContext is ReferencesViewModel)
                {
                    ReferenceListViewModel view_model = new ReferenceListViewModel(((ReferencesViewModel)this.DataContext).User, new DialogService(), new WindowService());
                    Page _mainPage = new ReferenceList(view_model);
                    _mainPage.DataContext = view_model;
                    _mainFrame.Navigate(_mainPage);
                }
            };
        }
    }
}
