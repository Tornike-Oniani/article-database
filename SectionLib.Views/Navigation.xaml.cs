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

namespace SectionLib.Views
{
    /// <summary>
    /// Interaction logic for Navigation.xaml
    /// </summary>
    public partial class Navigation : UserControl
    {
        public Navigation()
        {
            InitializeComponent();
        }

        // When we click Data view focus the keyboard on that so that keybindings will work
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            //if (DataView.CurrentView != null)
            //    Keyboard.Focus(DataView.CurrentView);
        }
    }
}
