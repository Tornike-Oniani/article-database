using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ViewModels.Commands;

namespace ViewModels.UIStructs
{
    public class ReferenceBox : BaseModel
    {
        // Private members
        private bool _isChecked;

        // Public properties
        public Reference Reference { get; set; }
        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; OnPropertyChanged("IsChecked"); }
        }

        // Constructor
        public ReferenceBox(Reference reference)
        {
            this.Reference = reference;
        }

        // Public methods
        public void HasArticle(Article article)
        {
            this.IsChecked = (new ReferenceRepo()).CheckArticleInReference(Reference, article);
        }
    }
}
