using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lib.Views.UserControls
{
    /// <summary>
    /// Interaction logic for PendingButton.xaml
    /// </summary>
    public partial class PendingButton : UserControl, INotifyPropertyChanged
    {
        // Text to be displayed on initial start button
        private static readonly DependencyProperty StartTextProperty =
            DependencyProperty.Register("StartText", typeof(string), typeof(PendingButton), new PropertyMetadata(null));

        public string StartText
        {
            get { return (string)GetValue(StartTextProperty); }
            set { SetValue(StartTextProperty, value); }
        }

        // Text to be displayed during pending status
        private static readonly DependencyProperty PendingTextProperty =
            DependencyProperty.Register("PendingText", typeof(string), typeof(PendingButton), new PropertyMetadata(null));

        public string PendingText
        {
            get { return (string)GetValue(PendingTextProperty); }
            set { SetValue(PendingTextProperty, value); }
        }

        // Text to be displayed on action button
        private static readonly DependencyProperty ActionTextProperty =
            DependencyProperty.Register("ActionText", typeof(string), typeof(PendingButton), new PropertyMetadata(null));

        public string ActionText
        {
            get { return (string)GetValue(ActionTextProperty); }
            set { SetValue(ActionTextProperty, value); }
        }

        // Is pending (we use this to add column to datagrid, since we use list to map columns)
        private static readonly DependencyProperty IsPendingProperty =
            DependencyProperty.Register("IsPending", typeof(bool), typeof(PendingButton), new PropertyMetadata(null));

        public bool IsPending
        {
            get { return (bool)GetValue(IsPendingProperty); }
            set { SetValue(IsPendingProperty, value); OnPropertyChanged("IsPending"); }
        }

        // Is pending (we use this to add column to datagrid, since we use list to map columns)
        private static readonly DependencyProperty CanRunActionProperty =
            DependencyProperty.Register("CanRunAction", typeof(bool), typeof(PendingButton), new PropertyMetadata(null));

        public bool CanRunAction
        {
            get { return (bool)GetValue(CanRunActionProperty); }
            set { SetValue(CanRunActionProperty, value); }
        }

        // Command to be run on firs toggle buttons click (which starts pending)
        private static readonly DependencyProperty InitialCommandProperty =
            DependencyProperty.Register("InitialCommand", typeof(ICommand), typeof(PendingButton), new PropertyMetadata(null));

        public ICommand InitialCommand
        {
            get { return (ICommand)GetValue(InitialCommandProperty); }
            set { SetValue(InitialCommandProperty, value); }
        }

        // Command to be run on action button click
        private static readonly DependencyProperty ActionCommandProperty =
            DependencyProperty.Register("ActionCommand", typeof(ICommand), typeof(PendingButton), new PropertyMetadata(null));

        public ICommand ActionCommand
        {
            get { return (ICommand)GetValue(ActionCommandProperty); }
            set { SetValue(ActionCommandProperty, value); }
        }

        // Command to be run on cancel button click
        private static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(PendingButton), new PropertyMetadata(null));

        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        // Constructor
        public PendingButton()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
