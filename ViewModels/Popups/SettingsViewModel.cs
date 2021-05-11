using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainLib.ViewModels.Popups
{
    public class SettingsViewModel : BaseViewModel
    {

        private int _fontSize;

        public int FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; OnPropertyChanged("FontSize"); }
        }

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public SettingsViewModel()
        {
            this.Title = "Settings...";
            this.FontSize = Properties.Settings.Default.FontSize;
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        public void Save(object input = null)
        {
            Properties.Settings.Default.FontSize = FontSize;
            Properties.Settings.Default.Save();
            this.Window.Close();
        }
        public void Cancel(object input = null)
        {
            this.Window.Close();
        }
    }
}
