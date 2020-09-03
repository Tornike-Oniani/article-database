using ArticleDatabase.Commands;
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
    /// Interaction logic for EditDialog.xaml
    /// </summary>
    public partial class EditDialog : Window
    {

        public RelayCommand CancelCommand { get; set; }
        public static bool Open;

        public EditDialog()
        {
            InitializeComponent();
            CancelCommand = new RelayCommand(Cancel);

            this.Loaded += (s, e) =>
            {
                Open = true;
                this.MinHeight = this.ActualHeight;
            };

            this.Closed += (s, e) => Open = false;
        }

        public void Cancel(object input = null)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
