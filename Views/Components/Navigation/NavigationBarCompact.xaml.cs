using Lib.ViewModels.Commands;
using MainLib.Views.Main;
using MainLib.Views.Utils;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainLib.Views.Components.Navigation
{
    /// <summary>
    /// Interaction logic for NavigationBarCompact.xaml
    /// </summary>
    public partial class NavigationBarCompact : UserControl, INotifyPropertyChanged
    {
        private bool _isExpanded;

        private double startingWidth = 0;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; OnPropertyChanged("IsExpanded"); }
        }

        public ICommand ExpandCommand { get; set; }

        public NavigationBarCompact()
        {
            this.ExpandCommand = new RelayCommand(Expand);

            InitializeComponent();

            this.Loaded += NavigationBarCompact_Loaded;
        }

        private void NavigationBarCompact_Loaded(object sender, RoutedEventArgs e)
        {
            this.startingWidth = Sidebar.ActualWidth;
        }

        public void Expand(object input = null)
        {
            double targetWidth = this.IsExpanded ? this.startingWidth : 176;

            DoubleAnimation widthAnimation = new DoubleAnimation()
            {
                From = Sidebar.ActualWidth,
                To = targetWidth,
                Duration = TimeSpan.FromMilliseconds(100),
                EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseIn }
            };

            Sidebar.BeginAnimation(WidthProperty, widthAnimation);

            this.IsExpanded = !IsExpanded;
        }

        // Keyobard focus so that keybindings will work
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataView.CurrentView != null)
                Keyboard.Focus(DataView.CurrentView);
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
