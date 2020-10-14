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
using MainLib.ViewModels.Utils;
using Lib.DataAccessLayer.Info;

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
            ReferenceRepo referenceRepo = new ReferenceRepo();

            // 0. Get old reference name
            string name = referenceRepo.GetReferenceNameWithId(Reference.ID);

            bool hasMainArticle = !string.IsNullOrWhiteSpace(MainArticleTitle);

            if (hasMainArticle)
                Reference.ArticleID = (new ArticleRepo()).CheckArticleWithTitle(MainArticleTitle);

            // 1. Update reference in database
            if (hasMainArticle)
                referenceRepo.UpdateReference(Reference, true);
            else
                referenceRepo.UpdateReference(Reference, false);

            // 1.1 Track reference update
            ReferenceInfo info = new ReferenceInfo(Reference.Name);
            new Tracker(new User() { Username = "Nikoloz" }).TrackUpdate<ReferenceInfo>(info, name);

            _parent.PopulateReferences();

            // Close window
            (input as ICommand).Execute(null);
        }
        public bool CanSaveReference(object input = null)
        {
            return !string.IsNullOrWhiteSpace(Reference.Name) && 
                   (string.IsNullOrWhiteSpace(MainArticleTitle) || (new ArticleRepo()).CheckArticleWithTitle(MainArticleTitle) != 0);
        }
    }
}
