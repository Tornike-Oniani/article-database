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
using MainLib.ViewModels.Main;
using MainLib.ViewModels.Pages;
using MainLib.Views.Pages;
using Lib.Views.Services.Dialogs;
using Lib.Views.Services.Windows;

namespace MainLib.Views.Main
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
                    BookmarksViewModel vm = (BookmarksViewModel)this.DataContext;
                    BookmarkListViewModel view_model = new BookmarkListViewModel(vm.User, vm.WorkStatus, new DialogService(), new WindowService());
                    Page _mainPage = new BookmarkList(view_model);
                    _mainPage.DataContext = view_model;
                    _mainFrame.Navigate(_mainPage);
                }
                else if (this.DataContext is ReferencesViewModel)
                {
                    ReferencesViewModel vm = (ReferencesViewModel)this.DataContext;
                    ReferenceListViewModel view_model = new ReferenceListViewModel(vm.User, vm.WorkStatus, new DialogService(), new WindowService());
                    Page _mainPage = new ReferenceList(view_model);
                    _mainPage.DataContext = view_model;
                    _mainFrame.Navigate(_mainPage);
                }
            };
        }
    }
}
