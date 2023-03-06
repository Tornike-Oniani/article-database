using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Services.Dialogs;
using Lib.Views.Services.Dialogs;
using MainLib.ViewModels.Main;
using MainLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MainLib.Views.Main
{
    /// <summary>
    /// Interaction logic for DataEntry.xaml
    /// </summary>
    public partial class DataEntry : UserControl
    {
        public DataEntry()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                txbTitle.Focus();
            };

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
            TextBox txbTitle = sender as TextBox;

            // Regex to switch multiple spaces into one (Restricts user to enter more than one space in Title textboxes)
            //RegexOptions options = RegexOptions.None;
            //Regex regex = new Regex("[ ]{2,}", options);
            //Regex unusualCharacters = new Regex("^[A-Za-z0-9 .,'()+/_?:\"\\&*%$#@<>{}!=;-]+$");

            DataEntryViewModel vm = this.DataContext as DataEntryViewModel;
            txbTitle.Text = TextFormat.RemoveSpareWhiteSpace(txbTitle.Text);
            vm.TitleUnusualCharacters = new List<string>(TextFormat.GetUnusualCharacters(txbTitle.Text));

            //txbTitle.Text = txbTitle.Text.Trim();
            //txbTitle.Text = regex.Replace(txbTitle.Text, " ");
            //txbTitle.Text.Replace("\n", " ").Replace("\r", " ").Replace("–", "-").Replace("“", "\"").Replace("”", "\"").Replace("„", "\"").Replace("‟", "\"");

            // Check for unusual characters
            //if (!string.IsNullOrEmpty(txbTitle.Text))
            //{
            //    new DialogService().OpenDialog(new DialogOkViewModel("This input contains unusual characters, please retype it manually. (Don't copy & paste!)", "Error", DialogType.Error));
            //    txbTitle.Text = null;
            //    return;
            //}

            string title = txbTitle.Text;
            string file;

            if (new ArticleRepo().Exists(title, out file))
            {

                if (new DialogService().OpenDialog(new DialogYesNoViewModel("An article with the given title already exists in Database, do you want to see the file?", "Duplicate", DialogType.Warning)))
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
                        new DialogService().OpenDialog(new DialogOkViewModel("File was not found", "Error", DialogType.Error));
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
