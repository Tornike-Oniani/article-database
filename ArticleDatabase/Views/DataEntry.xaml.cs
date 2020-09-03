using ArticleDatabase.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace ArticleDatabase.Views
{
    /// <summary>
    /// Interaction logic for DataEntry.xaml
    /// </summary>
    public partial class DataEntry : UserControl
    {
        public DataEntry()
        {
            InitializeComponent();
            this.Loaded += DataEntry_Loaded;
        }

        private void DataEntry_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. If main window is too small make it bigger and center it on screen
            if (MainWindow.CurrentMain.Width > 800)
                MainWindow.CurrentMain.Width = 800;

            // 2. Focus the title textbox
            txbTitle.Focus();
        }

        // Refocus title text box after add/clear
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            txbTitle.Focus();
        }

        // Accept only numbers in text box
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (int.TryParse(e.Text, out _))
                e.Handled = false;
            else
                e.Handled = true;
        }

        // Check if article already exists
        private void txbTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            string title = ((TextBox)sender).Text;
            string file;

            if (SqliteDataAccess.Exists(title, out file))
            {
                if (MessageBox.Show("An article with the given title already exists in Database, do you want to see the file?", "Duplicate", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // User clicked yes
                    // Set the file path with filename and FolderPath static attribute
                    string full_path = Path.Combine(Environment.CurrentDirectory, "Files\\") + file + ".pdf";
                    try
                    {
                        // Open given file with default program
                        Process.Start(full_path);
                        return;
                    }
                    catch
                    {
                        // This is when user doubleclicks on article that has null file
                        MessageBox.Show("File was not found.");
                    }
                }
                else
                {
                    // User clicked no
                    return;
                }
            }
        }
    }
}
