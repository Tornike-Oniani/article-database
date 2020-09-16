using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ViewModels.Commands;

namespace ViewModels.Services.Dialogs
{
    public class DialogYesNoViewModel : DialogViewModelBase
    {

        public ICommand YesCommand { get; set; }
        public ICommand NoCommand { get; set; }

        public DialogYesNoViewModel(string text, string title, DialogType type) : base(text, title, type)
        {
            YesCommand = new RelayCommand(Yes);
            NoCommand = new RelayCommand(No);
        }

        public void Yes(object input)
        {
            this.SetDialogResult(true, input as IWindow);
        }

        public void No(object input)
        {
            this.SetDialogResult(false, input as IWindow);
        }
    }
}
