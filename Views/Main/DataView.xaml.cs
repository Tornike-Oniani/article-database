using Lib.ViewModels.Services.Dialogs;
using Lib.Views.Services.Dialogs;
using MainLib.ViewModels.Main;
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

namespace MainLib.Views.Main
{
    /// <summary>
    /// Interaction logic for DataView.xaml
    /// </summary>
    public partial class DataView : UserControl
    {
        public static DataView CurrentView;

        public DataView()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                // 1. Focus keyobard on this view (We need this so that Usercontrol.Inputbindings work)
                Keyboard.Focus(this);

                // 2. Get the view to static
                CurrentView = this;

                // 3. Open search dialog
                //DataViewViewModel vm = (DataViewViewModel)this.DataContext;
                //vm.OpenSearchDialogCommand.Execute(null);
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
