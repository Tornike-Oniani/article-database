using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Lib.Views.UserControls
{
    /// <summary>
    /// Interaction logic for Accordion.xaml
    /// </summary>
    public partial class Accordion : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty DropdownContentProperty = DependencyProperty.Register("DropdownContent", typeof(Grid), typeof(Accordion));

        public Grid DropdownContent
        {
            get { return (Grid)GetValue(DropdownContentProperty); }
            set { SetValue(DropdownContentProperty, value); }
        }

        private static readonly DependencyProperty CommandProperty =
    DependencyProperty.Register("Command", typeof(ICommand), typeof(Accordion), new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        private static readonly DependencyProperty IsOpenedProperty =
DependencyProperty.Register("IsOpened", typeof(bool), typeof(Accordion), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsOpened
        {
            get { return (bool)GetValue(IsOpenedProperty); }
            set { SetValue(IsOpenedProperty, value); OnPropertyChanged("IsOpened"); }
        }

        public Accordion()
        {
            InitializeComponent();
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
