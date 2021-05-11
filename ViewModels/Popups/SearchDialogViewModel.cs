using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Main;

namespace MainLib.ViewModels.Popups
{
    public class SearchDialogViewModel : BaseViewModel
    {
        public DataViewViewModel Parent { get; set; }

        public ICommand SearchCommand { get; set; }

        public SearchDialogViewModel(DataViewViewModel parent)
        {
            this.Title = "Set filter...";
            this.Parent = parent;

            this.SearchCommand = new RelayCommand(Search);
        }

        public void Search(object input = null)
        {
            Parent.LoadArticlesCommand.Execute(null);
            this.Window.Close();
        }
    }
}
