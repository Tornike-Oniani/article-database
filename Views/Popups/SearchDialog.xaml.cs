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
            };
        }
    }
}
