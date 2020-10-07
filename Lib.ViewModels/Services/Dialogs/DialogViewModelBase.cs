using Lib.ViewModels.Services.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.ViewModels.Services.Dialogs
{
    public class DialogViewModelBase : INotifyPropertyChanged
    {
        private DialogType _type;

        public DialogType Type
        {
            get { return _type; }
            set { _type = value; OnPropertyChanged("Type"); }
        }
        public string Title { get; set; }
        public string Text { get; set; }
        public bool UserDialogResult { get; set; }

        public DialogViewModelBase(string text, string title, DialogType type)
        {
            this.Title = title;
            this.Text = text;
            this.Type = type;
        }

        public void SetDialogResult(bool result, IWindow win)
        {
            this.UserDialogResult = result;
            if (win != null)
                win.DialogResult = result;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
