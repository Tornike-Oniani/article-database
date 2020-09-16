using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Base;
using ViewModels.Main;

namespace ViewModels.Popups
{
    public class SearchDialogViewModel : BaseViewModel
    {
        public DataViewViewModel Parent { get; set; }

        public SearchDialogViewModel(DataViewViewModel parent)
        {
            this.Title = "Set filter";

            this.Parent = parent;
        }
    }
}
