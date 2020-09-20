using Lib.ViewModels.Services.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ViewModels.Services;

namespace Lib.ViewModels.Base
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public IWindow Window { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
