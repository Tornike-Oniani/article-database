using Lib.ViewModels.Services.Dialogs;
using Lib.Views.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace MainLib.Views.Popups
{
    /// <summary>
    /// Interaction logic for SearchDialog.xaml
    /// </summary>
    public partial class SearchDialog : UserControl
    {
        public SearchDialog()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                Keyboard.Focus(this);
                txbSearch.Focus();
            };
        }

        private void txbSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txbTitle = sender as TextBox;

            // Regex to switch multiple spaces into one (Restricts user to enter more than one space in Title textboxes)
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            Regex unusualCharacters = new Regex("^[A-Za-z0-9 .,'()+/_?:\"\\&*%$#@<>{}!=;-]+$");

            txbTitle.Text = txbTitle.Text.Trim();
            txbTitle.Text = regex.Replace(txbTitle.Text, " ");

            // Check for unusual characters
            if (!string.IsNullOrEmpty(txbTitle.Text) & !unusualCharacters.IsMatch(txbTitle.Text))
            {
                new DialogService().OpenDialog(new DialogOkViewModel("This input contains unusual characters, please retype it manually. (Don't copy & paste!)", "Error", DialogType.Error));
                txbTitle.Text = null;
                return;
            }
        }
    }
}
