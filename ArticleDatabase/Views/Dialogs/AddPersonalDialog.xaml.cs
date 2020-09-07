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
    /// Interaction logic for AddPersonalDialog.xaml
    /// </summary>
    public partial class AddPersonalDialog : Window
    {

        public RelayCommand CancelCommand { get; set; }

        public AddPersonalDialog()
        {
            InitializeComponent();

            CancelCommand = new RelayCommand(Cancel);

            this.Loaded += (s, e) =>
            {
                this.MinHeight = this.ActualHeight;
            };
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
