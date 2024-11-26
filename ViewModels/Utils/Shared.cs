using Lib.DataAccessLayer.Models;
using Lib.ViewModels.Services.Browser;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;
using NotificationService;
using System;
using System.ComponentModel;

namespace MainLib.ViewModels.Utils
{
    public sealed class Shared : INotifyPropertyChanged
    {
        // Singleton implementaion
        private Shared() { }
        private static Shared _instance;
        public static Shared GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Shared();
                _instance.SelectedTheme = Properties.Settings.Default.Theme;
            }
            return _instance;
        }

        private string _selectedTheme;

        private Action<bool, string> _isWorkingAction;
        private Action<bool> _isShowingDialogAction;
        private INotificationManager _notificationManager;

        public IDialogService DialogService { get; private set; }
        public IWindowService WindowService { get; private set; }
        public IBrowserService BrowserService { get; private set; }
        public IThemeService ThemeService { get; private set; }
        public User User { get; private set; }
        public string LastExportFolderPath { get; private set; }
        public string LastSyncFolderPath { get; private set; }
        public string SelectedTheme
        {
            get { return _selectedTheme; }
            set { _selectedTheme = value; OnPropertyChanged("SelectedTheme"); }
        }


        public void SetServices(IDialogService dialogService, IWindowService windowService, IBrowserService browserService, IThemeService themeService)
        {
            this.DialogService = dialogService;
            this.WindowService = windowService;
            this.BrowserService = browserService;
            this.ThemeService = themeService;

            this._notificationManager = new NotificationManager();
        }
        public void SetWorkingStatusAction(Action<bool, string> isWorkingAction)
        {
            this._isWorkingAction = isWorkingAction;
        }
        public void SetLoggedInUser(User user)
        {
            this.User = user;
        }
        public void SetShowDialogAction(Action<bool> isShowingDialogAction)
        {
            this._isShowingDialogAction = isShowingDialogAction;
        }

        public void SaveExportPath(string path)
        {
            this.LastExportFolderPath = path;
        }
        public void SaveSyncPath(string path)
        {
            this.LastSyncFolderPath = path;
        }
        public void IsWorking(bool isWorking, string label = "Working...")
        {
            this._isWorkingAction.Invoke(isWorking, label);
        }
        public void IsShowingDialog(bool isShowingDialog)
        {
            this._isShowingDialogAction.Invoke(isShowingDialog);
        }
        public void ShowNotification(string message, string title, NotificationType type, string areaName, TimeSpan? expirationTime = null)
        {
            // If expiration time was provided set it to default 5 seconds
            TimeSpan? et = expirationTime == null ? new TimeSpan(0, 0, 5) : expirationTime;

            _notificationManager.Show(new NotificationContent { Message = message, Title = title, Type = type }, areaName: areaName, expirationTime: expirationTime);
        }
        public bool ShowDialogWithOverlay(DialogViewModelBase vm)
        {
            this.IsShowingDialog(true);
            bool result = this.DialogService.OpenDialog(vm);
            this.IsShowingDialog(false);
            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
