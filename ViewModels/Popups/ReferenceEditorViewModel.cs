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
using Lib.ViewModels.Services.Dialogs;

namespace MainLib.ViewModels.Popups
{
    public class ReferenceEditorViewModel : BaseViewModel
    {
        // Private members
        private ReferenceListViewModel _parent;
        private string _name;

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
            try
            {
                ReferenceRepo referenceRepo = new ReferenceRepo();

                // 0. Get old reference name
                _name = referenceRepo.GetReferenceNameWithId(Reference.ID);

                bool hasMainArticle = !string.IsNullOrWhiteSpace(MainArticleTitle);

                if (hasMainArticle)
                    Reference.ArticleID = (new ArticleRepo()).CheckArticleWithTitle(MainArticleTitle);

                // 1. Update reference in database
                Reference.Name = Reference.Name.Trim();
                if (hasMainArticle)
                    referenceRepo.UpdateReference(Reference, true);
                else
                    referenceRepo.UpdateReference(Reference, false);

                // 1.1 Track reference update
                ReferenceInfo info = new ReferenceInfo(Reference.Name);
                new Tracker(new User() { Username = "Nikoloz", Admin = 1 }).TrackUpdate<ReferenceInfo>(info, _name);
            }
            catch(Exception e)
            {
                // Reference with that name already exists (ask merge dialog)
                if (e.Message.Contains("UNIQUE"))
                {
                    if (
                        Shared.GetInstance().ShowDialogWithOverlay(new DialogYesNoViewModel("Reference with that name already exists, do you want to merge?", "Merge reference", DialogType.Question))
                       )
                    {
                        ReferenceRepo repo = new ReferenceRepo();
                        Reference referenceFrom = repo.GetReference(_name);
                        Reference referenceInto = repo.GetReference(Reference.Name);
                        List<Article> articlesFrom = repo.LoadArticlesForReference(referenceFrom);
                        List<Article> articlesInto = repo.LoadArticlesForReference(referenceInto);

                        // 1. Get unique articles between 2 references
                        List<Article> uniques = articlesFrom.Where(art => !articlesInto.Exists(el => el.Title == art.Title)).ToList();

                        // 2. Add those articles into merged reference
                        repo.AddListOfArticlesToReference(referenceInto, uniques);

                        // 3. Delete old reference
                        repo.DeleteReference(referenceFrom);
                    }
                }
                // Generic exception
                else
                {
                    new BugTracker().Track("Refernce Editor", "Save Reference", e.Message, e.StackTrace);
                    Shared.GetInstance().DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
                }
            }
            finally
            {
                _parent.PopulateReferences();

                // Close window
                (input as ICommand).Execute(null);
            }
        }
        public bool CanSaveReference(object input = null)
        {
            return !string.IsNullOrWhiteSpace(Reference.Name) && 
                   (string.IsNullOrWhiteSpace(MainArticleTitle) || (new ArticleRepo()).CheckArticleWithTitle(MainArticleTitle) != 0);
        }
    }
}
