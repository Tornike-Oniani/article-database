using Lib.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MainLib.ViewModels.Main
{
    public partial class DataViewViewModel
    {
        // Public proeprty fields
        private string _simpleSearch;
        private bool _searchOptionsIsChecked;
        private string _filterTitle;
        private bool _wordBreakMode;
        private string _filterAuthors;
        private string _filterKeywords;
        private string _filterYear;
        private string _filterPersonalComment;
        private string _selectedAuthorPairing;
        private string _selectedKeywordPairing;
        private string _idFilter;
        private string _filterAbstract;
        private List<string> _titleSearchWords;
        private List<string> _titleSearchPhrases;
        private List<string> _abstractSearchWords;
        private List<string> _abstractSearchPhrases;

        string[] dudWords = new string[] { "in", "of", "at", "to", "into", "on", "onto", "a", "the", "and" };


        // Public properties
        public string SimpleSearch
        {
            get { return _simpleSearch; }
            set { _simpleSearch = value; OnPropertyChanged("SimpleSearch"); }
        }
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
                OnPropertyChanged("FilterTitle");
            }
        }
        public bool WordBreakMode
        {
            get { return _wordBreakMode; }
            set { _wordBreakMode = value; }
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
        public string FilterAuthors
        {
            get { return _filterAuthors; }
            set { _filterAuthors = value; OnPropertyChanged("FilterAuthors"); }
        }
        public string FilterKeywords
        {
            get { return _filterKeywords; }
            set { _filterKeywords = value; OnPropertyChanged("FilterKeywords"); }
        }
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
        public string FilterAbstract
        {
            get { return _filterAbstract; }
            set { _filterAbstract = value; OnPropertyChanged("FilterAbstract"); }
        }      
        public List<string> TitleSearchWords
        {
            get { return _titleSearchWords; }
            set { _titleSearchWords = value; OnPropertyChanged("TitleSearchWords"); }
        }
        public List<string> TitleSearchPhrases
        {
            get { return _titleSearchPhrases; }
            set { _titleSearchPhrases = value; OnPropertyChanged("TitleSearchPhrases"); }
        }
        public List<string> AbstractSearchWords
        {
            get { return _abstractSearchWords; }
            set { _abstractSearchWords = value; OnPropertyChanged("AbstractSearchWords"); }
        }
        public List<string> AbstractSearchPhrases
        {
            get { return _abstractSearchPhrases; }
            set { _abstractSearchPhrases = value; OnPropertyChanged("AbstractSearchPhrases"); }
        }

        // Temporary authors and keywords highlighter
        public string TitleHighlight { get { return this._filterTitle; } }
        public string AuthorHighlight
        {
            get
            {
                //if (FilterAuthors.Count > 0) { return String.Join(" ", FilterAuthors); }
                if (String.IsNullOrEmpty(FilterAuthors)) { return ""; }
                return FilterAuthors.Replace(",", "");
            }
        }
        public string KeywordHighlight
        {
            get
            {
                //if (FilterKeywords.Count > 0) { return String.Join(" ", FilterKeywords); }
                if (String.IsNullOrEmpty(FilterKeywords)) { return ""; }
                return FilterKeywords.Replace(",", "");
            }
        }

        // Commands
        public ICommand ClearCommand { get; set; }

        // Command actions
        public void Clear(object input = null)
        {
            FilterTitle = null;
            FilterAuthors = null;
            FilterKeywords = null;
            FilterYear = null;
            FilterPersonalComment = null;
            IdFilter = null;
            FilterAbstract= null;
            FilterAbstract = null;
            Articles.Clear();
            TitleSearchWords.Clear();
            TitleSearchPhrases.Clear();
            AbstractSearchWords.Clear();
            AbstractSearchPhrases.Clear();
            OnPropertyChanged("FilterTitle");
        }
        // Command action validators
        public bool CanClear(object input = null)
        {
            if (
                string.IsNullOrEmpty(FilterTitle) &&
                string.IsNullOrEmpty(FilterAuthors) &&
                string.IsNullOrEmpty(FilterKeywords) &&
                string.IsNullOrEmpty(FilterYear) &&
                string.IsNullOrEmpty(FilterPersonalComment) &&
                string.IsNullOrEmpty(IdFilter)
                )
                return false;

            return true;
        }

        // Private helpers
        private void InitializeSearchOptions()
        {
            // 1. Initialize collections and fields
            this.AuthorPairings = new List<string>() { "AND", "OR" };
            this.KeywordPairings = new List<string>() { "AND", "OR" };
            if (String.IsNullOrWhiteSpace(SelectedAuthorPairing))
                this.SelectedAuthorPairing = AuthorPairings[0];
            if (String.IsNullOrWhiteSpace(SelectedKeywordPairing))
                this.SelectedKeywordPairing = KeywordPairings[0];

            this.ClearCommand = new RelayCommand(Clear, CanClear);
        }
    }
}
