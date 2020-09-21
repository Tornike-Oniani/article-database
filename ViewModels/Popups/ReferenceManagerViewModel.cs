﻿using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using ViewModels.UIStructs;

namespace MainLib.ViewModels.Popups
{
    public class ReferenceManagerViewModel : BaseViewModel
    {
        private Visibility _newReferenceVisibility;
        private Visibility _createVisibility;
        private Article _article;
        private string _name;
        private double _initialHeight;
        private IDialogService _dialogService;

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
        public RelayCommand CheckChangedCommand { get; set; }

        // Constructor
        public ReferenceManagerViewModel(Article article, IDialogService dialogService)
        {
            this.Title = "Save to...";
            this._dialogService = dialogService;

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
            CheckChangedCommand = new RelayCommand(CheckChanged);

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
            // 1. Add new bookmark to database
            string trimmedName = Name.Trim();
            bool duplicate_check = (new ReferenceRepo()).AddReference(trimmedName);

            // 2. Refresh Bookmarks collection
            ReferenceBoxes.Clear();
            PopulateReferenceBoxes();
            Name = "";

            // 3. Hide detailed view
            //Win.Height = _initialHeight;
            CreateVisibility = Visibility.Collapsed;
            NewReferenceVisibility = Visibility.Visible;

            // 4. Check bookmark boxes
            CheckReferenceBoxes();

            if (!duplicate_check)
                _dialogService.OpenDialog(new DialogOkViewModel("This reference already exists.", "Warning", DialogType.Warning));
        }
        public bool CanCreate(object input = null)
        {
            return !String.IsNullOrWhiteSpace(Name);
        }
        public void CheckChanged(object input)
        {
            ReferenceBox current_reference_box = input as ReferenceBox;

            // 1. If user checked add article to reference
            if (current_reference_box.IsChecked)
            {
                (new ReferenceRepo()).AddArticleToReference(current_reference_box.Reference, _article);
            }
            // 2. If user unchecked remove article from bookmark
            else
            {
                (new ReferenceRepo()).RemoveArticleFromReference(current_reference_box.Reference, _article);
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
            foreach (ReferenceBox referenceBox in ReferenceBoxes)
                referenceBox.HasArticle(_article);
        }
    }
}
