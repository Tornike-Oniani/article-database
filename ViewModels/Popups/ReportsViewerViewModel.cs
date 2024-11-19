using Lib.DataAccessLayer.Models;
using Lib.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Popups
{
    public class ReportsViewerViewModel : BaseViewModel
    {
        public List<Article> Articles { get; set; }

        public ReportsViewerViewModel(List<Article> articles)
        {
            this.Title = "Reports Viewer";
            this.Articles = articles;
        }
    }
}
