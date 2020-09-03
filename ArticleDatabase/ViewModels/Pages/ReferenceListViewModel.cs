using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ArticleDatabase.ViewModels.Pages
{
    public class ReferenceListViewModel : BaseViewModel
    {
        private User _user;
        private CollectionViewSource _referencesCollection;
        private string _filterText;

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                _referencesCollection.View.Refresh();
                OnPropertyChanged("FilterText");
            }
        }


        public ObservableCollection<Reference> References { get; set; } 
        public ICollectionView ReferencesCollection { get { return _referencesCollection.View; } }
        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }


        /**
         * Commands:
         *  - Open bookmark
         *  [The action of this command is in code behind of BookmarkList view because we use NavigationService which is not accessible from view model]
         *  - Delete bookmark
         */
        public RelayCommand OpenReferenceCommand { get; internal set; }
        public RelayCommand DeleteReferenceCommand { get; set; }

        // Constructor
        public ReferenceListViewModel(User user)
        {
            this.User = user;
            References = new ObservableCollection<Reference>();
            _referencesCollection = new CollectionViewSource();
            _referencesCollection.Source = References;
            _referencesCollection.Filter += _referencesCollection_Filter;
            PopulateReferences();

            // Initialize commands
            DeleteReferenceCommand = new RelayCommand(DeleteReference);
        }

        private void _referencesCollection_Filter(object sender, FilterEventArgs e)
        {
            // Edge case: filter text is empty
            if (String.IsNullOrWhiteSpace(FilterText))
            {
                e.Accepted = true;
                return;
            }

            Reference current = e.Item as Reference;
            if (current.Name.ToUpper().Contains(FilterText.ToUpper()))
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }

        /**
         * Command actions
         */
        public void DeleteReference(object input)
        {
            // 1. Retrieve sent bookmark
            Reference selected_reference = input as Reference;

            // 2. Ask user if they are sure
            if (MessageBox.Show("Delete following bookmark?\n" + selected_reference.Name, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                // 3. Delete bookmark record from database
                SqliteDataAccess.DeleteReference(selected_reference);

                // 4. Refresh bookmark collections
                PopulateReferences();
            }
        }

        /**
         * Public methods
         */
        public void PopulateReferences(bool global = false)
        {
            // 1. Clear bookmarks
            References.Clear();

            // 2. Populate references
            foreach (Reference reference in SqliteDataAccess.LoadReferences())
            {
                reference.PopulateArticles();
                reference.SetMainArticle();
                // Populate articles colletion for each bookmark
                References.Add(reference);
            }
        }
    }
}
