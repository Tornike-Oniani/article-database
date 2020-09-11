using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleDatabase.ViewModels.Dialogs
{
    public class SearchDialogViewModel : BaseViewModel
    {
        public DataViewViewModel Parent { get; set; }

        public SearchDialogViewModel(DataViewViewModel parent)
        {
            this.Parent = parent;
        }
    }
}
