using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ViewModels.Commands;
using ViewModels.Services;

namespace Views
{
    public class WindowBase : Window, INotifyPropertyChanged, IWindow
    {
        #region Private members
        /// <summary>
        /// The margin around the window to allow a drop shadow
        /// </summary>
        private int mOuterMarginSize = 10;
        #endregion

        #region Public properties
        /// <summary>
        /// The smallest width the window can go to
        /// </summary>
        public double WindowMinimumWidth { get; set; } = 700;

        /// <summary>
        /// The smallest height the window can go to
        /// </summary>
        public double WindowMinimumHeight { get; set; } = 650;

        /// <summary>
        /// The size of the resize border around the window
        /// </summary>
        public int ResizeBorder { get; set; } = 6;

        /// <summary>
        /// The size of a resize border around the window, taking into account the outer margin
        /// </summary>
        public Thickness ResizeBorderThickness { get { return new Thickness(ResizeBorder + OuterMarginSize); } }

        /// <summary>
        /// The margin around the window to allow a drop shadow
        /// If window is maximized we don't want any outer margin so we set it to 0
        /// </summary>
        public int OuterMarginSize
        {
            get
            {
                return this.WindowState == WindowState.Maximized ? 0 : mOuterMarginSize;
            }
            set
            {
                mOuterMarginSize = value;
            }
        }

        /// <summary>
        /// The margin around the window to allow a drop shadow
        /// </summary>
        public Thickness OuterMarginSizeThickness { get { return new Thickness(OuterMarginSize); } }

        /// <summary>
        /// The height of the tite bar / caption of the window
        /// </summary>
        public int TitleHeight { get; set; } = 40;

        /// <summary>
        /// The height of the tite bar / caption of the window
        /// </summary>
        public GridLength TitleHeightGridLength { get { return new GridLength(TitleHeight + ResizeBorder); } }
        #endregion

        #region Commands
        // Command to minimze the window
        public ICommand MinimizeCommand { get; set; }

        // Command to maximize the window
        public ICommand MaximizeCommand { get; set; }

        // Command to close the window
        public ICommand CloseCommand { get; set; }

        // Command to show the system menu of the window
        public ICommand MenuCommand { get; set; }
        #endregion

        // Constructor
        public WindowBase()
        {
            // Create commands
            MinimizeCommand = new RelayCommand(Minimize);
            // WidnowState.Maximized is 2 and normal is 2 we want to switch between Normal and maximized each time this button is pressed
            // ^= (XOR) sets the WindowState to 0 if it is already maximized and if not it sets to Maximized
            MaximizeCommand = new RelayCommand(Maximize);
            CloseCommand = new RelayCommand(CloseAction);
            MenuCommand = new RelayCommand(ShowMenu);

            // Listen for the window resizing
            this.StateChanged += (sender, e) =>
            {
                // Fire off events for all properties that are affected by window resize
                OnPropertyChanged(nameof(ResizeBorderThickness));
                OnPropertyChanged(nameof(OuterMarginSize));
                OnPropertyChanged(nameof(OuterMarginSizeThickness));
            };

            // Fix window resize issue
            var resizer = new WindowResizer(this);
        }

        // Public methods / command actions
        public void Minimize(object input = null)
        {
            this.WindowState = WindowState.Minimized;
        }
        public void Maximize(object input = null)
        {
            this.WindowState ^= WindowState.Maximized;
        }
        public void CloseAction(object input = null)
        {
            this.Close();
        }
        public void ShowMenu(object input = null)
        {
            SystemCommands.ShowSystemMenu(this, GetMousePosition());
        }

        // Private helpers
        private Point GetMousePosition()
        {
            // Position of the mouse relative to the window
            var position = Mouse.GetPosition(this);

            // Add the window position so its a "ToScreen"
            return new Point(position.X + this.Left, position.Y + this.Top);
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
