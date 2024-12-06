using MainLib.Views.Main;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MainLib.Views.Components.Navigation
{
    /// <summary>
    /// Interaction logic for NavigationBar.xaml
    /// </summary>
    public partial class NavigationBar : UserControl
    {
        public NavigationBar()
        {
            InitializeComponent();
        }

        // Keyobard focus so that keybindings will work
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataView.CurrentView != null)
                Keyboard.Focus(DataView.CurrentView);
        }
    }
}
