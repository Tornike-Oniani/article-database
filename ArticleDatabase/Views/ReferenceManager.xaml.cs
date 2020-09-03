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
    /// Interaction logic for ReferenceManager.xaml
    /// </summary>
    public partial class ReferenceManager : Window
    {
        public ReferenceManager()
        {
            InitializeComponent();

            this.Height = MainWindow.CurrentMain.Height * 80 / 100;
            this.Width = MainWindow.CurrentMain.Width * 40 / 100;

            this.Loaded += (object sender, RoutedEventArgs e) =>
            {
                searchBox.Focus();
            };
        }

        private void txbReferenceName_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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
