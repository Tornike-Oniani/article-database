using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Popups;
using MainLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace MainLib.ViewModels.Pages
{
    public class ReferenceListViewModel : BaseViewModel
    {
        private User _user;
        private Shared services;

        public CollectionViewSource _referencesCollection { get; set; }
        public ObservableCollection<Reference> References { get; set; }
        public ICollectionView ReferencesCollection { get { return _referencesCollection.View; } }
        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }

        // Commands
        public RelayCommand OpenReferenceCommand { get; set; }
        public ICommand CreateNewReferenceCommand { get; set; }
        public RelayCommand EditReferenceCommand { get; set; }
        public RelayCommand DeleteReferenceCommand { get; set; }

        // Constructor
        public ReferenceListViewModel()
        {
            this.services = Shared.GetInstance();
            this.User = services.User;
            References = new ObservableCollection<Reference>();
            _referencesCollection = new CollectionViewSource();
            _referencesCollection.Source = References;
            PopulateReferences();

            // Initialize commands
            this.CreateNewReferenceCommand = new RelayCommand(CreateNewReference);
            EditReferenceCommand = new RelayCommand(EditReference);
            DeleteReferenceCommand = new RelayCommand(DeleteReference);
        }

        // Command actions
        public void CreateNewReference(object input = null)
        {
            this.services.ShowWindowWithOverlay(new AddNewReferenceViewModel(this));
        }
        public void EditReference(object input)
        {
            this.services.ShowWindowWithOverlay(new ReferenceEditorViewModel(input as Reference, this));
        }
        public void DeleteReference(object input)
        {
            try
            {
                // 1. Retrieve sent reference
                Reference selected_reference = input as Reference;

                // 2. Ask user if they are sure
                if (services.ShowDialogWithOverlay(new DialogYesNoViewModel("Delete selected reference?", "Check", DialogType.Question)))
                {
                    // 3. Delete reference record from database
                    new ReferenceRepo().DeleteReference(selected_reference);

                    // 3.1 Track reference delete
                    new Tracker(new User() { Username = "Nikoloz", Admin = 1 }).TrackDelete("Reference", selected_reference.Name);

                    // 4. Refresh bookmark collections
                    PopulateReferences();
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Reference List", "Delete reference", e.Message, e.StackTrace);
                services.ShowDialogWithOverlay(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
        }

        // Public methods
        public async void PopulateReferences(bool global = false)
        {
            try
            {
                services.IsWorking(true);

                // 1. Clear bookmarks
                References.Clear();

                // 2. Populate references
                await Populate();

                services.IsWorking(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Reference List", "Populate references", e.Message, e.StackTrace);
                services.ShowDialogWithOverlay(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
            }
        }
        public async Task Populate()
        {
            List<Reference> references = new List<Reference>();

            await Task.Run(() =>
            {
                foreach (Reference reference in new ReferenceRepo().LoadReferences())
                {
                    // Populate articles colletion for each bookmark
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
