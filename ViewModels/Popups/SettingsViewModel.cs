using Lib.DataAccessLayer.Models;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MainLib.ViewModels.Popups
{
    public class SettingsViewModel : BaseViewModel
    {

        private int _fontSize;
        private string _syncName;
        private string _selectedTheme;
        private IThemeService _themeService;

        public int FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; OnPropertyChanged("FontSize"); }
        }
        public string SyncName
        {
            get { return _syncName; }
            set { _syncName = value; OnPropertyChanged("SyncName"); }
        }
        public User User { get; set; }
        public string SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                _selectedTheme = value;
                OnPropertyChanged("SelectedTheme");
            }
        }
        public ObservableCollection<string> Themes { get; set; }

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public SettingsViewModel()
        {
            this.User = Shared.GetInstance().User;
            this.Title = "Settings...";
            this.FontSize = Properties.Settings.Default.FontSize;
            this.SyncName = Properties.Settings.Default.SyncName;
            this._themeService = Shared.GetInstance().ThemeService;
            this.Themes = new ObservableCollection<string>()
            {
                "Standard",
                "Dim Monitor"
            };
            this.SelectedTheme = Properties.Settings.Default.Theme;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        public void Save(object input = null)
        {
            Properties.Settings.Default.FontSize = FontSize;
            Properties.Settings.Default.SyncName = SyncName;
            Properties.Settings.Default.Theme = _selectedTheme;
            Properties.Settings.Default.Save();
            _themeService.ChangeTheme(_selectedTheme);
            Shared.GetInstance().SelectedTheme = _selectedTheme;
            this.Window.Close();
        }
        public void Cancel(object input = null)
        {
            this.Window.Close();
        }
    }
}
