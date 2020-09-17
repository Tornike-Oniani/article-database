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

namespace Views.Popups
{
    /// <summary>
    /// Interaction logic for ReferenceManager.xaml
    /// </summary>
    public partial class ReferenceManager : UserControl
    {
        public ReferenceManager()
        {
            InitializeComponent();
        }

        private void txbReferenceName_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;

            if (textbox.Visibility == Visibility.Visible)
            {
                textbox.Focus();
            }
        }
    }
}
