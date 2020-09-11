using ArticleDatabase.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ArticleDatabase.ViewModels.Base
{
    public class BaseWindow : INotifyPropertyChanged
    {
        #region Private members
        /// <summary>
        /// The margin around the window to allow a drop shadow
        /// </summary>
        private int mOuterMarginSize = 10;

        private Window _window;
        #endregion

        #region Public properties
        public string Title { get; set; }

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
        /// The padding of the inner content of the main window
        /// </summary>
        public Thickness InnerContentPadding { get { return new Thickness(ResizeBorder); } }

        /// <summary>
        /// The margin around the window to allow a drop shadow
        /// If window is maximized we don't want any outer margin so we set it to 0
        /// </summary>
        public int OuterMarginSize
        {
            get
            {
                return _window.WindowState == WindowState.Maximized ? 0 : mOuterMarginSize;
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

        /**
         * We use this in BookmarkManager to change height of the window.
         * When I used Height binding method it messed up center startup location.
         */
        public Window Win { get { return _window; } }

        // Command to minimze the window
        public ICommand MinimizeCommand { get; set; }

        // Command to maximize the window
        public ICommand MaximizeCommand { get; set; }

        // Command to close the window
        public ICommand CloseCommand { get; set; }

        // Command to show the system menu of the window
        public ICommand MenuCommand { get; set; }

        // Constructor
        public BaseWindow()
        {
            // Create commands
            MinimizeCommand = new RelayCommand(Minimize);
            // WidnowState.Maximized is 2 and normal is 2 we want to switch between Normal and maximized each time this button is pressed
            // ^= (XOR) sets the WindowState to 0 if it is already maximized and if not it sets to Maximized
            MaximizeCommand = new RelayCommand(Maximize);
            CloseCommand = new RelayCommand(Close);
            MenuCommand = new RelayCommand(ShowMenu);
        }

        public BaseWindow(Window window)
        {
            _window = window;

            // Create commands
            MinimizeCommand = new RelayCommand(Minimize);
            // WidnowState.Maximized is 2 and normal is 2 we want to switch between Normal and maximized each time this button is pressed
            // ^= (XOR) sets the WindowState to 0 if it is already maximized and if not it sets to Maximized
            MaximizeCommand = new RelayCommand(Maximize);
            CloseCommand = new RelayCommand(Close);
            MenuCommand = new RelayCommand(ShowMenu);

            // Listen for the window resizing
            _window.StateChanged += (sender, e) =>
            {
                // Fire off events for all properties that are affected by window resize
                OnPropertyChanged(nameof(ResizeBorderThickness));
                OnPropertyChanged(nameof(OuterMarginSize));
                OnPropertyChanged(nameof(OuterMarginSizeThickness));
            };

            // Fix window resize issue
            var resizer = new WindowResizer(_window);
        }

        public void SetWindow(Window window)
        {
            _window = window;

            // Listen for the window resizing
            _window.StateChanged += (sender, e) =>
            {
                // Fire off events for all properties that are affected by window resize
                OnPropertyChanged(nameof(ResizeBorderThickness));
                OnPropertyChanged(nameof(OuterMarginSize));
                OnPropertyChanged(nameof(OuterMarginSizeThickness));
            };

            // Fix window resize issue
            var resizer = new WindowResizer(_window);
        }

        public void Minimize(object input = null)
        {
            _window.WindowState = WindowState.Minimized;
        }

        public void Maximize(object input = null)
        {
            _window.WindowState ^= WindowState.Maximized;
        }

        public void Close(object input = null)
        {
            _window.Close();
        }

        public void ShowMenu(object input = null)
        {
            SystemCommands.ShowSystemMenu(_window, GetMousePosition());
        }

        private Point GetMousePosition()
        {
            // Position of the mouse relative to the window
            var position = Mouse.GetPosition(_window);

            // Add the window position so its a "ToScreen"
            return new Point(position.X + _window.Left, position.Y + _window.Top);
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
