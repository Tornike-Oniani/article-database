using ArticleDatabase.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Base;

namespace ViewModels.Main
{
    public class ReferencesViewModel : BaseViewModel
    {
        public User User { get; set; }

        public ReferencesViewModel(User user)
        {
            this.User = user;
        }
    }
}
