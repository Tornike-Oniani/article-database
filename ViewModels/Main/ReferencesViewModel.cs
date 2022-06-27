using Lib.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ViewModels.Base;
using MainLib.ViewModels.Utils;

namespace MainLib.ViewModels.Main
{
    public class ReferencesViewModel : BaseViewModel
    {
        public User User { get; set; }

        public ReferencesViewModel()
        {
            this.User = Shared.GetInstance().User;
        }
    }
}
