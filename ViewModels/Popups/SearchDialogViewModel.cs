using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ViewModels.Base;
using MainLib.ViewModels.Main;

namespace MainLib.ViewModels.Popups
{
    public class SearchDialogViewModel : BaseViewModel
    {
        public DataViewViewModel Parent { get; set; }

        public SearchDialogViewModel(DataViewViewModel parent)
        {
            this.Title = "Set filter...";

            this.Parent = parent;
        }
    }
}
