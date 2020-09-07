using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleDatabase.Dialogs.DialogService
{
    public class DialogViewModelBase
    {
        public DialogType Type { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public bool UserDialogResult { get; set; }

        public DialogViewModelBase(string text, string title, DialogType type)
        {
            this.Title = title;
            this.Text = text;
            this.Type = type;
        }

        public void CloseDialogWithResult(Window dialog, bool result)
        {
            this.UserDialogResult = result;
            if (dialog != null)
                dialog.DialogResult = true;
        }
    }
}
