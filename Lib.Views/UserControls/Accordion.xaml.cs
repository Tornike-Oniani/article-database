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

namespace Lib.Views.UserControls
{
    /// <summary>
    /// Interaction logic for Accordion.xaml
    /// </summary>
    public partial class Accordion : UserControl, INotifyPropertyChanged
    {
        private static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(Accordion), new PropertyMetadata(null));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty DropdownContentProperty = 
            DependencyProperty.Register("DropdownContent", typeof(Grid), typeof(Accordion));

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

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            AnimateAccordion();
        }

        private void AnimateAccordion()
        {
            double targetHeight = 0;

            if (IsOpened)
            {
                if (accordion__dropdown.Child is FrameworkElement content)
                {
                    content.Measure(new Size(accordion__dropdown.ActualWidth, double.PositiveInfinity));
                    targetHeight = content.DesiredSize.Height;
                }
            }

            var animation = new DoubleAnimation()
            {
                From = accordion__dropdown.ActualHeight,
                To = targetHeight,
                Duration = TimeSpan.FromMilliseconds(100),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            accordion__dropdown.BeginAnimation(HeightProperty, animation);
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
