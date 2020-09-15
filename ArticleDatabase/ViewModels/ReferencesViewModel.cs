using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleDatabase.ViewModels
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
