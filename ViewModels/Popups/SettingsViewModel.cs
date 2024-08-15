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

        #region Property fields
        private int _fontSize;
        private int _headerFontSize;
        private int _textFontSize;
        private int _smallTextFontSize;
        private int _buttonFontSize;
        private int _abstractBoxFontSize;
        private int _inputTextFontSize;
        private string _syncName;
        private string _allowedCharacters;
        private string _selectedTheme;
        #endregion

        #region Private attributes
        private readonly IThemeService _themeService;
        #endregion

        #region Public properties
        public int FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; OnPropertyChanged("FontSize"); }
        }
        public int HeaderFontSize
        {
            get { return _headerFontSize; }
            set { _headerFontSize = value; OnPropertyChanged("HeaderFontSize"); }
        }
        public int TextFontSize
        {
            get { return _textFontSize; }
            set { _textFontSize = value; OnPropertyChanged("TextFontSize"); }
        }
        public int SmallTextFontSize
        {
            get { return _smallTextFontSize; }
            set { _smallTextFontSize = value; OnPropertyChanged("SmallTextFontSize"); }
        }
        public int ButtonFontSize
        {
            get { return _buttonFontSize; }
            set { _buttonFontSize = value; OnPropertyChanged("BigHeaderFontSize"); }
        }       
        public int AbstractBoxFontSize
        {
            get { return _abstractBoxFontSize; }
            set { _abstractBoxFontSize = value; OnPropertyChanged("AbstractBoxFontSize"); }
        }
        public int InputTextFontSize
        {
            get { return _inputTextFontSize; }
            set { _inputTextFontSize = value; OnPropertyChanged("InputTextFontSize"); }
        }
        public string SyncName
        {
            get { return _syncName; }
            set { _syncName = value; OnPropertyChanged("SyncName"); }
        }
        public string AllowedCharacters
        {
            get { return _allowedCharacters; }
            set { _allowedCharacters = value; OnPropertyChanged("AllowedCharacters"); }
        }
        public string SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                _selectedTheme = value;
                OnPropertyChanged("SelectedTheme");
            }
        }
        public User User { get; set; }
        public ObservableCollection<string> Themes { get; set; }
        #endregion

        #region Commands
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        #endregion

        #region Constructor
        public SettingsViewModel()
        {
            this.User = Shared.GetInstance().User;
            this.Title = "Settings...";
            this.FontSize = Properties.Settings.Default.FontSize;
            this.HeaderFontSize = Properties.Settings.Default.HeaderFontSize;
            this.TextFontSize = Properties.Settings.Default.TextFontSize;
            this.SmallTextFontSize = Properties.Settings.Default.SmallTextFontSize;
            this.ButtonFontSize = Properties.Settings.Default.ButtonFontSize;
            this.AbstractBoxFontSize = Properties.Settings.Default.AbstractBoxFontSize;
            this.InputTextFontSize = Properties.Settings.Default.InputTextFontSize;
            this.SyncName = Properties.Settings.Default.SyncName;
            this.AllowedCharacters = Properties.Settings.Default.AllowedCharacters;
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
        #endregion

        #region Command actions
        public void Save(object input = null)
        {
            Properties.Settings.Default.FontSize = FontSize;
            Properties.Settings.Default.HeaderFontSize = HeaderFontSize;
            Properties.Settings.Default.TextFontSize = TextFontSize;
            Properties.Settings.Default.SmallTextFontSize = SmallTextFontSize;
            Properties.Settings.Default.ButtonFontSize = ButtonFontSize;
            Properties.Settings.Default.AbstractBoxFontSize = AbstractBoxFontSize;
            Properties.Settings.Default.InputTextFontSize = InputTextFontSize;
            Properties.Settings.Default.SyncName = SyncName;
            Properties.Settings.Default.AllowedCharacters = AllowedCharacters;
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
        #endregion
    }
}
