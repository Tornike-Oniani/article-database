using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using System;
using System.Windows.Input;
using MainLib.ViewModels.Utils;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Services.Dialogs;
using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Info;
using MainLib.ViewModels.Pages;

namespace MainLib.ViewModels.Popups
{
    public class AddNewReferenceViewModel : BaseViewModel
    {
        // Private helpers
        private readonly ReferenceListViewModel _parent;
        private Shared _services;

        // Binded properties
        public string ReferenceName { get; set; } = String.Empty;
        public string MainArticleTitle { get; set; } = String.Empty;

        // Commands
        public ICommand CreateCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        // Constructor
        public AddNewReferenceViewModel(ReferenceListViewModel parent)
        {
            this._parent = parent;
            this._services = Shared.GetInstance();
            this.Title = "New Reference...";
            this.CreateCommand = new RelayCommand(Create);
            this.CancelCommand = new RelayCommand(Cancel);
        }

        // Command actions
        public void Create(object input = null)
        {
            // Format input
            ReferenceName = ReferenceName.Trim();
            MainArticleTitle = MainArticleTitle.Trim();

            // 1. Check if main article exists
            if (!new ArticleRepo().Exists(MainArticleTitle, out _))
            {
                _services.DialogService.OpenDialog(new DialogOkViewModel("Article with that title doesn't exists, recheck main article name.", "Error", DialogType.Error));
                return;
            }

            ReferenceRepo repo = new ReferenceRepo();
            // 2. Create new Reference and check if it already exists
            if (!repo.AddReference(ReferenceName))
            {
                _services.DialogService.OpenDialog(new DialogOkViewModel("Reference already exists", "Error", DialogType.Error));
                return;
            }

            // 3. Get created reference id
            Reference addedReference = repo.GetReference(ReferenceName);

            // 4. Get main article id
            addedReference.ArticleID = new ArticleRepo().CheckArticleWithTitle(MainArticleTitle);

            // 5. Updated created reference with main article id
            repo.UpdateReference(addedReference, true);

            // Track reference creation
            ReferenceInfo info = new ReferenceInfo(ReferenceName);
            new Tracker(new User() { Username = "Nikoloz", Admin = 1 }).TrackCreate<ReferenceInfo>(info);
            info = new ReferenceInfo(addedReference.Name);
            new Tracker(new User() { Username = "Nikoloz", Admin = 1 }).TrackUpdate<ReferenceInfo>(info, addedReference.Name);

            // Refresh references list and close dialog
            _parent.PopulateReferences();
            ((ICommand)input).Execute(null);

        }
        public void Cancel(object input = null)
        {
            ((ICommand)input).Execute(null);
        }
    }
}
