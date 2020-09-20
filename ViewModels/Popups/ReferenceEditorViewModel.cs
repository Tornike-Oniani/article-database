using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Pages;

namespace MainLib.ViewModels.Popups
{
    public class ReferenceEditorViewModel : BaseViewModel
    {
        // Private members
        private ReferenceListViewModel _parent;

        // Public properties
        public Reference Reference { get; set; }
        public string MainArticleTitle { get; set; }

        // Commands
        public RelayCommand SaveReferenceCommand { get; set; }

        // Constructor
        public ReferenceEditorViewModel(Reference reference, ReferenceListViewModel parent)
        {
            this.Title = "Save as...";

            this.Reference = new Reference();
            this.Reference.CopyByValue(reference);
            this._parent = parent;
            if (Reference.ArticleID != 0 && Reference.ArticleID != null)
                this.MainArticleTitle = (new ArticleRepo()).GetArticleWithId(Reference.ArticleID).Title;

            SaveReferenceCommand = new RelayCommand(SaveReference, CanSaveReference);
        }

        // Command actions
        public void SaveReference(object input)
        {
            Reference.ArticleID = (new ArticleRepo()).CheckArticleWithTitle(MainArticleTitle);

            (new ReferenceRepo()).UpdateReference(Reference);

            _parent.PopulateReferences();

            // Close window
            (input as ICommand).Execute(null);
        }
        public bool CanSaveReference(object input = null)
        {
            return !String.IsNullOrWhiteSpace(Reference.Name) && (new ArticleRepo()).CheckArticleWithTitle(MainArticleTitle) != 0;
        }
    }
}
