using ArticleDatabase.Commands;
using ArticleDatabase.Dialogs.DialogService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ArticleDatabase.Dialogs.DialogOk
{
    public class DialogOkViewModel : DialogViewModelBase
    {
        public ICommand OkCommand { get; set; }

        public DialogOkViewModel(string text, string title, DialogType type) : base(text, title, type)
        {
            OkCommand = new RelayCommand(Ok);
        }

        public void Ok(object input)
        {
            this.CloseDialogWithResult(input as Window, true);
        }
    }
}
