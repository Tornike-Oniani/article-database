﻿using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using ArticleDatabase.Dialogs.DialogOk;
using ArticleDatabase.Dialogs.DialogService;
using ArticleDatabase.UIStructs;
using ArticleDatabase.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ArticleDatabase.ViewModels
{
    public class ReferenceManagerViewModel : BaseViewModel
    {
        private Visibility _newReferenceVisibility;
        private Visibility _createVisibility;
        private Article _article;
        private string _name;
        private double _initialHeight;

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

        // Constructor
        public ReferenceManagerViewModel(Article article)
        {
            this.Title = "Save to...";

            // 1. Initialize starting state
            this._article = article;
            NewReferenceVisibility = Visibility.Visible;
            CreateVisibility = Visibility.Collapsed;
            ReferenceBoxes = new ObservableCollection<ReferenceBox>();
            _referenceBoxesCollection = new CollectionViewSource();
            _referenceBoxesCollection.Source = ReferenceBoxes;
            PopulateReferenceBoxes();
            this._initialHeight = MainWindow.CurrentMain.Height * 80 / 100;

            // 2. Set up actions
            CreateNewReferenceCommand = new RelayCommand(CreateNewReference);
            CreateCommand = new RelayCommand(Create, CanCreate);

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

            if (!duplicate_check)
                DialogService.OpenDialog(new DialogOkViewModel("This reference already exists.", "Warning", DialogType.Warning), MainWindow.CurrentMain);
        }

        public bool CanCreate(object input = null)
        {
            return !String.IsNullOrWhiteSpace(Name);
        }

        /**
         * Private helpers:
         *  - Populate bookmarks 
         *  [Transforms bookmark models into BookmarkBox which have additional attribute to deremine if the bookmark checkbox on UI hast to be checked]
         */
        private void PopulateReferenceBoxes()
        {
            foreach (Reference reference in (new ReferenceRepo()).LoadReferences())
                ReferenceBoxes.Add(new ReferenceBox(reference, _article));
        }
    }
}
