using Lib.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Main
{
    public partial class DataViewViewModel
    {
        private bool _searchOptionsIsChecked;
        private string _filterTitle;
        private string _filterAuthor;
        private string _filterKeyword;
        private string _filterYear;
        private string _filterPersonalComment;
        private bool _wordBreakMode;
        private string _selectedAuthorPairing;
        private string _selectedKeywordPairing;
        private string _idFilter;


        public bool SearchOptionsIsChecked
        {
            get { return _searchOptionsIsChecked; }
            set { _searchOptionsIsChecked = value; OnPropertyChanged("SearchOptionsIsChecked"); }
        }
        public string FilterTitle
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_filterTitle))
                    return null;

                return _filterTitle;
            }
            set
            {
                _filterTitle = value;
                //OnPropertyChanged("FilterTitle"); 
            }
        }
        public string FilterAuthor
        {
            get { return _filterAuthor; }
            set
            {
                _filterAuthor = value; OnPropertyChanged("FilterAuthor");
            }
        }
        public string FilterKeyword
        {
            get { return _filterKeyword; }
            set
            {
                _filterKeyword = value; OnPropertyChanged("FilterKeyword");
            }
        }
        public string FilterYear
        {
            get { return _filterYear; }
            set { _filterYear = value; OnPropertyChanged("FilterYear"); }
        }
        public string FilterPersonalComment
        {
            get { return _filterPersonalComment; }
            set { _filterPersonalComment = value; OnPropertyChanged("FilterPersonalComment"); }
        }
        public List<string> AuthorPairings { get; set; }
        public List<string> KeywordPairings { get; set; }
        public ObservableCollection<string> FilterAuthors { get; set; }
        public ObservableCollection<string> FilterKeywords { get; set; }
        public string SelectedAuthorPairing
        {
            get { return _selectedAuthorPairing; }
            set { _selectedAuthorPairing = value; OnPropertyChanged("SelectedAuthorPairing"); }
        }
        public string SelectedKeywordPairing
        {
            get { return _selectedKeywordPairing; }
            set { _selectedKeywordPairing = value; OnPropertyChanged("SelectedKeywordPairing"); }
        }
        public string IdFilter
        {
            get { return _idFilter; }
            set { _idFilter = value; OnPropertyChanged("IdFilter"); }
        }

        // Temporary authors and keywords highlighter
        public string AuthorHighlight
        {
            get
            {
                if (FilterAuthors.Count > 0) { return String.Join(" ", FilterAuthors); }

                return "";
            }
        }
        public string KeywordHighlight
        {
            get
            {
                if (FilterKeywords.Count > 0) { return String.Join(" ", FilterKeywords); }

                return "";
            }
        }
        public bool WordBreakMode
        {
            get { return _wordBreakMode; }
            set { _wordBreakMode = value; }
        }

        public RelayCommand ClearCommand { get; set; }

        public DataViewViewModel()
        {   
            // 1. Initialize collections and fields
            FilterAuthors = new ObservableCollection<string>();
            FilterKeywords = new ObservableCollection<string>();

            this.AuthorPairings = new List<string>() { "AND", "OR" };
            this.KeywordPairings = new List<string>() { "AND", "OR" };
            if (String.IsNullOrWhiteSpace(SelectedAuthorPairing))
                this.SelectedAuthorPairing = AuthorPairings[0];
            if (String.IsNullOrWhiteSpace(SelectedKeywordPairing))
                this.SelectedKeywordPairing = KeywordPairings[0];

            this.ClearCommand = new RelayCommand(Clear, CanClear);
        }

        public void Clear(object input = null)
        {
            FilterTitle = null;
            FilterAuthors.Clear();
            FilterKeywords.Clear();
            FilterYear = null;
            FilterPersonalComment = null;
            IdFilter = null;
            Articles.Clear();
            OnPropertyChanged("FilterTitle");
        }
        public bool CanClear(object input = null)
        {
            if (
                string.IsNullOrEmpty(FilterTitle) &&
                FilterAuthors.Count == 0 &&
                FilterKeywords.Count == 0 &&
                string.IsNullOrEmpty(FilterYear) &&
                string.IsNullOrEmpty(FilterPersonalComment) &&
                string.IsNullOrEmpty(IdFilter)
                )
                return false;

            return true;
        }
    }
}
