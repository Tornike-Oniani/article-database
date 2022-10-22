using Lib.ViewModels.Services.Dialogs;
using Lib.Views.Services.Dialogs;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
