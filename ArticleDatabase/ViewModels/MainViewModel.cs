using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ArticleDatabase.ViewModels
{
    public class MainViewModel : BaseViewModel
    {

        #region Private Member

        /// <summary>
        /// The window this model controls
        /// </summary>
        private Window mWindow;

        /// <summary>
        /// The margin around the window to allow a drop shadow
        /// </summary>
        private int mOuterMarginSize = 10;

        #endregion

        #region Public Properties

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
                return mWindow.WindowState == WindowState.Maximized ? 0 : mOuterMarginSize;
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

        /// <summary>
        /// Command to minimze the window
        /// </summary>
        public ICommand MinimizeCommand { get; set; }

        /// <summary>
        /// Command to maximize the window
        /// </summary>
        public ICommand MaximizeCommand { get; set; }

        /// <summary>
        /// Command to close the window
        /// </summary>
        public ICommand CloseCommand { get; set; }

        /// <summary>
        /// Command to show the system menu of the window
        /// </summary>
        public ICommand MenuCommand { get; set; }

        #endregion

        #region Private Helpers

        private Point GetMousePosition()
        {
            // Position of the mouse relative to the window
            var position = Mouse.GetPosition(mWindow);

            // Add the window position so its a "ToScreen"
            return new Point(position.X + mWindow.Left, position.Y + mWindow.Top);
        }

        #endregion

        private BaseViewModel _selectedViewModel;
        private User _user;

        public BaseViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set { _selectedViewModel = value; OnPropertyChanged("SelectedViewModel"); }
        }

        public ICommand UpdateViewCommand { get; set; }

        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }


        public MainViewModel(User user, Window window)
        {
            UpdateViewCommand = new UpdateViewCommand(this, user);
            this.SelectedViewModel = new HomeViewModel(user);
            this.Title = user.Username;
            // Set admin/user status
            user.Admin = (new UserRepo()).IsAdmin(user);
            this.User = user;

            mWindow = window;

            // Listen for the window resizing
            mWindow.StateChanged += (sender, e) =>
            {
                // Fire off events for all properties that are affected by window resize
                OnPropertyChanged(nameof(ResizeBorderThickness));
                OnPropertyChanged(nameof(OuterMarginSize));
                OnPropertyChanged(nameof(OuterMarginSizeThickness));
            };

            // Create commands
            MinimizeCommand = new RelayCommand(Minimize);
            // WidnowState.Maximized is 2 and normal is 2 we want to switch between Normal and maximized each time this button is pressed
            // ^= (XOR) sets the WindowState to 0 if it is already maximized and if not it sets to Maximized
            MaximizeCommand = new RelayCommand(Maximize);
            CloseCommand = new RelayCommand(Close);
            MenuCommand = new RelayCommand(ShowMenu);

            // Fix window resize issue
            var resizer = new WindowResizer(mWindow);
        }


        public void Minimize(object input = null)
        {
            mWindow.WindowState = WindowState.Minimized;
        }

        public void Maximize(object input = null)
        {
            mWindow.WindowState ^= WindowState.Maximized;
        }

        public void Close(object input = null)
        {
            mWindow.Close();
        }

        public void ShowMenu(object input = null)
        {
            SystemCommands.ShowSystemMenu(mWindow, GetMousePosition());
        }
    }
}
