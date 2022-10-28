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

namespace MainLib.ViewModels.Popups
{
    public class SettingsViewModel : BaseViewModel
    {

        private int _fontSize;
        private string _syncName;

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

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public SettingsViewModel()
        {
            this.User = Shared.GetInstance().User;
            this.Title = "Settings...";
            this.FontSize = Properties.Settings.Default.FontSize;
            this.SyncName = Properties.Settings.Default.SyncName;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        public void Save(object input = null)
        {
            Properties.Settings.Default.FontSize = FontSize;
            Properties.Settings.Default.SyncName = SyncName;
            Properties.Settings.Default.Save();
            this.Window.Close();
        }
        public void Cancel(object input = null)
        {
            this.Window.Close();
        }
    }
}
