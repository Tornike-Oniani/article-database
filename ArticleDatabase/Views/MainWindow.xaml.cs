using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.ViewModels;
using ArticleDatabase.Views;
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

namespace ArticleDatabase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase
    {
        public static MainWindow CurrentMain;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                CurrentMain = this;
            };
            this.Closing += (s, e) =>
            {
                Application.Current.Shutdown();
            };
        }

        // Keyobard focus so that keybindings will work
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataView.CurrentView != null)
                Keyboard.Focus(DataView.CurrentView);
        }
    }
}
