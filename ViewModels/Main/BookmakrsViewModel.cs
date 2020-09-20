using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.DataAccessLayer.Models;
using Lib.ViewModels.Base;

namespace MainLib.ViewModels.Main
{
    public class BookmarksViewModel : BaseViewModel
    {
        public User User { get; set; }

        public BookmarksViewModel(User user)
        {
            User = user;
        }
    }
}
