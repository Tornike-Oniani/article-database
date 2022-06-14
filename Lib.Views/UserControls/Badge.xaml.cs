using MaterialDesignThemes.Wpf;
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

namespace Lib.Views.UserControls
{
    /// <summary>
    /// Interaction logic for Badge.xaml
    /// </summary>
    public partial class Badge : UserControl
    {
        private static readonly new DependencyProperty BackgroundProperty =
    DependencyProperty.Register("Background", typeof(Brush), typeof(Badge), new PropertyMetadata(null));

        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        private static readonly new DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(Badge), new PropertyMetadata(null));

        public new Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        private static readonly DependencyProperty IconProperty =
           DependencyProperty.Register("Icon", typeof(PackIconKind), typeof(Badge), new PropertyMetadata(null));

        public PackIconKind Icon
        {
            get { return (PackIconKind)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public Badge()
        {
            InitializeComponent();
        }
    }
}
