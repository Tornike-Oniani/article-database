using Lib.DataAccessLayer.Models;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainLib.ViewModels.Main
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Property fields
        private int _bigHeaderFontSize;
        private int _headerFontSize;
        private int _textFontSize;
        private int _smallTextFontSize;
        private int _articleHeaderFontSize;
        private int _articleTextFontSize;
        private string _syncName;
        private string _allowedCharacters;
        private string _selectedTheme;
        private string _selectedUIFont;
        private string _selectedArticleFont;
        #endregion

        #region Private attributes
        private readonly IThemeService _themeService;
        #endregion

        #region Public properties       
        public int BigHeaderFontSize
        {
            get { return _bigHeaderFontSize; }
            set { _bigHeaderFontSize = value; OnPropertyChanged("BigHeaderFontSize"); }
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
        public int ArticleHeaderFontSize
        {
            get { return _articleHeaderFontSize; }
            set { _articleHeaderFontSize = value; OnPropertyChanged("ArticleHeaderFontSize"); }
        }
        public int ArticleTextFontSize
        {
            get { return _articleTextFontSize; }
            set { _articleTextFontSize = value; OnPropertyChanged("ArticleTextFontSize"); }
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
        public List<string> UIFonts { get; set; }
        public List<string> ArticleFonts { get; set; }
        public string SelectedUIFont
        {
            get { return _selectedUIFont; }
            set { _selectedUIFont = value; OnPropertyChanged("SelectedUIFont"); }
        }
        public string SelectedArticleFont
        {
            get { return _selectedArticleFont; }
            set { _selectedArticleFont = value; OnPropertyChanged("SelectedArticleFont"); }
        }
        #endregion

        #region Commands
        public ICommand SaveCommand { get; set; }
        #endregion

        #region Constructor
        public SettingsViewModel()
        {
            this.User = Shared.GetInstance().User;
            this.Title = "Settings...";
            this.BigHeaderFontSize = Properties.Settings.Default.BigHeaderFontSize;
            this.HeaderFontSize = Properties.Settings.Default.HeaderFontSize;
            this.TextFontSize = Properties.Settings.Default.TextFontSize;
            this.SmallTextFontSize = Properties.Settings.Default.SmallTextFontSize;
            this.ArticleHeaderFontSize = Properties.Settings.Default.ArticleHeaderFontSize;
            this.ArticleTextFontSize = Properties.Settings.Default.ArticleTextFontSize;
            this.SyncName = Properties.Settings.Default.SyncName;
            this.AllowedCharacters = Properties.Settings.Default.AllowedCharacters;
            this._themeService = Shared.GetInstance().ThemeService;
            this.Themes = new ObservableCollection<string>()
            {
                "Minimalistic",
                "Contrast",
                "Dark"
            };
            this.SelectedTheme = Properties.Settings.Default.Theme;
            this.UIFonts = new List<string>()
            {
                "Inter",
                "Ebrima",
                "Gadugi",
                "Leelawadee UI",
                "Segoe UI",
                "Figtree",
                "YujiMai"
            };
            this.ArticleFonts = new List<string>()
            {
                "Helvetica",
                "Verdana",
                "Bookerly",
                "Lexend",
                "YujiMai"
            };
            this.SelectedUIFont = Properties.Settings.Default.UIFontFamily;
            this._selectedArticleFont = Properties.Settings.Default.ArticleFontFamily;

            this.SaveCommand = new RelayCommand(Save);
        }
        #endregion

        #region Command actions
        public void Save(object input = null)
        {
            Properties.Settings.Default.BigHeaderFontSize = this.BigHeaderFontSize;
            Properties.Settings.Default.HeaderFontSize = this.HeaderFontSize;
            Properties.Settings.Default.TextFontSize = this.TextFontSize;
            Properties.Settings.Default.SmallTextFontSize = this.SmallTextFontSize;
            Properties.Settings.Default.ArticleHeaderFontSize = this.ArticleHeaderFontSize;
            Properties.Settings.Default.ArticleTextFontSize = this.ArticleTextFontSize;
            Properties.Settings.Default.SyncName = this.SyncName;
            Properties.Settings.Default.AllowedCharacters = this.AllowedCharacters;
            Properties.Settings.Default.Theme = this.SelectedTheme;
            Properties.Settings.Default.UIFontFamily = this.SelectedUIFont;
            Properties.Settings.Default.ArticleFontFamily = this.SelectedArticleFont;
            Properties.Settings.Default.Save();
            _themeService.ChangeTheme(_selectedTheme);
            _themeService.ChangeFont(this.SelectedUIFont, FontTarget.UI);
            _themeService.ChangeFont(this.SelectedArticleFont, FontTarget.Article);
            Shared.GetInstance().SelectedTheme = _selectedTheme;
        }
        #endregion
    }
}
