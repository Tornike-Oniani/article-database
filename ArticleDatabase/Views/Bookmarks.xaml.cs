using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.ViewModels;
using ArticleDatabase.ViewModels.Pages;
using ArticleDatabase.Views.Pages;
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

namespace ArticleDatabase.Views
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
                BookmarkListViewModel view_model = new BookmarkListViewModel(((BookmarksViewModel)this.DataContext).User);
                Page _mainPage = new BookmarkList(view_model);
                _mainPage.DataContext = view_model;
                _mainFrame.Navigate(_mainPage);
            };
        }
    }
}
