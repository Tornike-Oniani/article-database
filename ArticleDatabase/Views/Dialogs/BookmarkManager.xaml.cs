using ArticleDatabase.Commands;
using ArticleDatabase.ViewModels;
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
using System.Windows.Shapes;

namespace ArticleDatabase.Views
{
    /// <summary>
    /// Interaction logic for BookmarkManager.xaml
    /// </summary>
    public partial class BookmarkManager : Window
    {
        //private BookmarkManagerViewModel _viewModel;

        public BookmarkManager()
        {
            InitializeComponent();

            this.Height = MainWindow.CurrentMain.Height * 80 / 100;
            this.Width = MainWindow.CurrentMain.Width * 40 / 100;
            //_viewModel = new BookmarkManagerViewModel(this);
            //this.DataContext = _viewModel;

            this.Loaded += (object sender, RoutedEventArgs e) =>
            {
                searchBox.Focus();
            };
        }

        private void txbBookmarkName_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;

            if (textbox.Visibility == Visibility.Visible)
            {
                textbox.Focus();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
