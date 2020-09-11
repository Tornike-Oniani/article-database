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
using System.Windows.Shapes;

namespace ArticleDatabase.Views
{
    /// <summary>
    /// Interaction logic for SearchDialog.xaml
    /// </summary>
    public partial class SearchDialog : Window
    {
        public static bool Open;

        public SearchDialog()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                Open = true;
                this.MinHeight = this.ActualHeight;
                Keyboard.Focus(this);
            };

            this.Closed += (s, e) => Open = false;
        }
    }
}
