using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Base;
using MainLib.ViewModels.Popups;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;

namespace MainLib.ViewModels.Pages
{
    public class ReferenceListViewModel : BaseViewModel
    {
        private User _user;
        private IWindowService _windowService;
        private IDialogService _dialogService;

        public CollectionViewSource _referencesCollection { get; set; }
        public ObservableCollection<Reference> References { get; set; }
        public ICollectionView ReferencesCollection { get { return _referencesCollection.View; } }
        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }
        public Action<bool> WorkStatus { get; set; }


        /**
         * Commands:
         *  - Open bookmark
         *  [The action of this command is in code behind of BookmarkList view because we use NavigationService which is not accessible from view model]
         *  - Delete bookmark
         */
        public RelayCommand OpenReferenceCommand { get; set; }
        public RelayCommand EditReferenceCommand { get; set; }
        public RelayCommand DeleteReferenceCommand { get; set; }

        // Constructor
        public ReferenceListViewModel(User user, Action<bool> workStatus, IDialogService dialogService, IWindowService windowService)
        {
            this.User = user;
            this.WorkStatus = workStatus;
            this._dialogService = dialogService;
            this._windowService = windowService;
            References = new ObservableCollection<Reference>();
            _referencesCollection = new CollectionViewSource();
            _referencesCollection.Source = References;
            PopulateReferences();

            // Initialize commands
            EditReferenceCommand = new RelayCommand(EditReference);
            DeleteReferenceCommand = new RelayCommand(DeleteReference);
        }

        /**
         * Command actions
         */
        public void EditReference(object input)
        {
            _windowService.OpenWindow(new ReferenceEditorViewModel(input as Reference, this));
        }
        public void DeleteReference(object input)
        {
            // 1. Retrieve sent bookmark
            Reference selected_reference = input as Reference;

            // 2. Ask user if they are sure

            if (_dialogService.OpenDialog(new DialogYesNoViewModel("Delete following reference?\n" + selected_reference.Name, "Check", DialogType.Question)))
            {
                // 3. Delete bookmark record from database
                (new ReferenceRepo()).DeleteReference(selected_reference);

                // 4. Refresh bookmark collections
                PopulateReferences();
            }
        }

        /**
         * Public methods
         */
        public async void PopulateReferences(bool global = false)
        {
            WorkStatus(true);

            // 1. Clear bookmarks
            References.Clear();

            // 2. Populate references
            await Populate();

            WorkStatus(false);
        }

        public async Task Populate()
        {
            List<Reference> references = new List<Reference>();

            await Task.Run(() =>
            {
                foreach (Reference reference in new ReferenceRepo().LoadReferences())
                {
                    // Populate articles colletion for each bookmark
                    //reference.PopulateArticles();
                    reference.GetArticleCount();
                    reference.SetMainArticle();
                    references.Add(reference);
                }
            });

            foreach (Reference reference in references)
                this.References.Add(reference);
        }
    }
}
