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
        private Window _window;

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
