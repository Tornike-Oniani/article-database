using Lib.DataAccessLayer.Info;
using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Main;
using MainLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using ViewModels.UIStructs;

namespace MainLib.ViewModels.Popups
{
    public class ReferenceManagerViewModel : BaseViewModel
    {
        private ViewType _parent;
        private Visibility _newReferenceVisibility;
        private Visibility _createVisibility;
        private Article _article;
        private string _name;
        private List<Reference> _references;
        private Tracker _tracker;
        private Shared services;

        public Visibility NewReferenceVisibility
        {
            get { return _newReferenceVisibility; }
            set { _newReferenceVisibility = value; OnPropertyChanged("NewReferenceVisibility"); }
        }
        public Visibility CreateVisibility
        {
            get { return _createVisibility; }
            set { _createVisibility = value; OnPropertyChanged("CreateVisibility"); }
        }
        public ObservableCollection<ReferenceBox> ReferenceBoxes { get; set; }
        public CollectionViewSource _referenceBoxesCollection { get; set; }
        public ICollectionView ReferenceBoxesCollection
        {
            get { return _referenceBoxesCollection.View; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        public RelayCommand CreateNewReferenceCommand { get; set; }
        public RelayCommand CreateCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }
        public RelayCommand CheckChangedCommand { get; set; }

        // Constructor
        public ReferenceManagerViewModel(
            ViewType parent,
            Article article = null,
            List<Reference> references = null)
        {
            this.services = Shared.GetInstance();
            this._parent = parent;
            this.Title = "Save to...";
            this._references = references;
            this._tracker = new Tracker(new User());

            // 1. Initialize starting state
            this._article = article;
            NewReferenceVisibility = Visibility.Visible;
            CreateVisibility = Visibility.Collapsed;
            ReferenceBoxes = new ObservableCollection<ReferenceBox>();
            _referenceBoxesCollection = new CollectionViewSource();
            _referenceBoxesCollection.Source = ReferenceBoxes;
            PopulateReferenceBoxes();
            //this._initialHeight = MainWindow.CurrentMain.Height * 80 / 100;

            // 2. Set up actions
            CreateNewReferenceCommand = new RelayCommand(CreateNewReference);
            CreateCommand = new RelayCommand(Create, CanCreate);
            CancelCommand = new RelayCommand(Cancel);

            // Case 1: Reference manager was opened by DataEntry
            if (_parent == ViewType.DataEntry)
                CheckChangedCommand = new RelayCommand(CheckChangedDataEntry);
            // Case 2: Reference manager was opened by DataView
            else if (_parent == ViewType.DataView)
                CheckChangedCommand = new RelayCommand(CheckChangedDataView);

            // 3. Check reference boxes
            CheckReferenceBoxes();
        }

        public void CreateNewReference(object input = null)
        {
            //Win.Height = _initialHeight + 40;
            NewReferenceVisibility = Visibility.Collapsed;
            CreateVisibility = Visibility.Visible;
        }

        public void Create(object input = null)
        {
            try
            {
                // 1. Add new reference to database
                string trimmedName = Name.Trim();
                bool duplicate_check = (new ReferenceRepo()).AddReference(trimmedName);

                // 1.1 Track create reference
                ReferenceInfo info = new ReferenceInfo(trimmedName);
                new Tracker(new User() { Username = "Nikoloz", Admin = 1 }).TrackCreate<ReferenceInfo>(info);

                // 2. Refresh References collection
                ReferenceBoxes.Clear();
                PopulateReferenceBoxes();
                Name = "";

                // 3. Hide detailed view
                //Win.Height = _initialHeight;
                CreateVisibility = Visibility.Collapsed;
                NewReferenceVisibility = Visibility.Visible;

                // 4. Check reference boxes
                CheckReferenceBoxes();

                if (!duplicate_check)
                    services.ShowDialogWithOverlay(new DialogOkViewModel("This reference already exists.", "Warning", DialogType.Warning));
            }
            catch (Exception e)
            {
                new BugTracker().Track("Reference Manager", "Create reference", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
        }
        public void Cancel(object input = null)
        {
            CreateVisibility = Visibility.Collapsed;
            NewReferenceVisibility = Visibility.Visible;
        }
        public bool CanCreate(object input = null)
        {
            return !String.IsNullOrWhiteSpace(Name);
        }
        public void CheckChangedDataView(object input)
        {
            ReferenceBox current_reference_box = input as ReferenceBox;

            // 1. If user checked add article to reference
            if (current_reference_box.IsChecked)
            {
                try
                {
                    new ReferenceRepo().AddArticleToReference(current_reference_box.Reference, _article);

                    // Tack
                    Couple info = new Couple("Reference", "Add", _article.Title, current_reference_box.Reference.Name);
                    new Tracker(new User() { Username = "Nikoloz", Admin = 1 }).TrackCoupling<Couple>(info);
                }
                catch (Exception e)
                {
                    new BugTracker().Track("Reference Manager", "Add article to reference", e.Message, e.StackTrace);
                    services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
                }
            }
            // 2. If user unchecked remove article from bookmark
            else
            {
                try
                {
                    new ReferenceRepo().RemoveArticleFromReference(current_reference_box.Reference, _article);

                    // Track
                    Couple info = new Couple("Reference", "Remove", _article.Title, current_reference_box.Reference.Name);
                    new Tracker(new User() { Username = "Nikoloz", Admin = 1 }).TrackCoupling<Couple>(info);
                }
                catch (Exception e)
                {
                    new BugTracker().Track("Reference Manager", "Remove article from reference", e.Message, e.StackTrace);
                    services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
                }
            }
        }
        public void CheckChangedDataEntry(object input)
        {
            ReferenceBox current_reference_box = input as ReferenceBox;

            // If Bookmarks contains the input remove it
            if (_references.Exists(el => el.Name == current_reference_box.Reference.Name))
            {
                try
                {
                    int index = _references.FindIndex(el => el.Name == current_reference_box.Reference.Name);
                    _references.RemoveAt(index);
                }
                catch (Exception e)
                {
                    new BugTracker().Track("Reference Manager (Data Entry)", "Remove article from reference", e.Message, e.StackTrace);
                    services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
                }
            }
            // If Bookmarks doesn't contain the input add it
            else
            {
                _references.Add(current_reference_box.Reference);
            }
        }

        /**
         * Private helpers:
         *  - Populate bookmarks 
         *  [Transforms bookmark models into BookmarkBox which have additional attribute to deremine if the bookmark checkbox on UI hast to be checked]
         */
        private void PopulateReferenceBoxes()
        {
            foreach (Reference reference in (new ReferenceRepo()).LoadReferences())
                ReferenceBoxes.Add(new ReferenceBox(reference));
        }
        private void CheckReferenceBoxes()
        {
            if (_parent == ViewType.DataEntry)
            {

                if (_references.Count != 0)
                    foreach (ReferenceBox referenceBox in ReferenceBoxes)
                    {
                        if (_references.Exists(el => el.Name == referenceBox.Reference.Name))
                            referenceBox.IsChecked = true;
                        else
                            referenceBox.IsChecked = false;
                    }
            }
            else if (_parent == ViewType.DataView)
            {
                foreach (ReferenceBox referenceBox in ReferenceBoxes)
                    referenceBox.HasArticle(_article);
            }
        }
    }
}
