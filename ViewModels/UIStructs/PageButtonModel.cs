using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.UIStructs
{
    /// <summary>
    /// We will create this class instances in ObsevableCollection and then
    /// bind it to items panel itemsource to generate buttons for datagrid
    /// pagination
    /// </summary>
    public class PageButtonModel
    {
        public int Page { get; private set; }
        public bool IsActive { get; private set; }

        // Constructor
        public PageButtonModel(int page, bool isActive)
        {
            this.Page = page;
            this.IsActive = isActive;
        }
    }
}
