using ArticleDatabase.Commands;
using ArticleDatabase.Dialogs.DialogService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ArticleDatabase.Dialogs.DialogYesNo
{
    class DialogYesNoViewModel : DialogViewModelBase
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
            this.CloseDialogWithResult(input as Window, true);
        }

        public void No(object input)
        {
            this.CloseDialogWithResult(input as Window, false);
        }
    }
}
