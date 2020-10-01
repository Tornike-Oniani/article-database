using Lib.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ViewModels.Base;

namespace MainLib.ViewModels.Main
{
    public class ReferencesViewModel : BaseViewModel
    {
        public User User { get; set; }
        public Action<bool> WorkStatus { get; set; }

        public ReferencesViewModel(User user, Action<bool> workStatus)
        {
            this.User = user;
            this.WorkStatus = workStatus;
        }
    }
}
