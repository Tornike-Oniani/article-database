using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using System.Windows.Input;

namespace MainLib.ViewModels.Popups
{
    public class AbstractEditorViewModel : BaseViewModel
    {
        public string Body { get; set; }

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public AbstractEditorViewModel()
        {
            this.SaveCommand = new RelayCommand(Save);
            this.CancelCommand = new RelayCommand(Cancel);
        }

        public void Save(object input = null)
        {

        }
        public void Cancel(object input = null)
        {

        }
    }
}
