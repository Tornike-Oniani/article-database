using MainLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.UIStructs
{
    public class ArticleForm : INotifyPropertyChanged, IDataErrorInfo
    {
        // Property fields
        private string _title;
        private string _year;
        private string _personalComment;
        private bool _hasNoAbstract;
        private string _abstract;
        private bool _sic;
        private bool _fileContainsOnlyAbstract;
        private string _filePath;
        private List<string> _unusualCharactersInTitle;
        private List<string> _unusualCharactersInAbstract;

        // Public properties
        public string Title
        {
            get { return _title; }
            set 
            { 
                // Format title
                _title = FormatText(value);
                // Get all unusual characters from title
                this.UnusualCharactersInTitle = new List<string>(TextFormat.GetUnusualCharacters(_title));
                OnPropertyChanged("Title"); 
            }
        }
        public ObservableCollection<string> Authors { get; set; }
        public ObservableCollection<string> Keywords { get; set; }
        public string Year
        {
            get { return _year; }
            set { _year = value; OnPropertyChanged("Year"); }
        }
        public string PersonalComment
        {
            get { return _personalComment; }
            set { _personalComment = value; OnPropertyChanged("PersonalComment"); }
        }
        public bool HasNoAbstract
        {
            get { return _hasNoAbstract; }
            set { _hasNoAbstract = value; OnPropertyChanged("HasNoAbstract"); }
        }
        public string Abstract
        {
            get { return _abstract; }
            set 
            { 
                // Format abstract
                _abstract = FormatText(value);
                // Get all unusual characters from abstract
                this.UnusualCharactersInAbstract = new List<string>(TextFormat.GetUnusualCharacters(_abstract));
                OnPropertyChanged("Abstract"); 
            }
        }
        public bool SIC
        {
            get { return _sic; }
            set { _sic = value; OnPropertyChanged("SIC"); }
        }
        public bool FileContainsOnlyAbstract
        {
            get { return _fileContainsOnlyAbstract; }
            set { _fileContainsOnlyAbstract = value; OnPropertyChanged("FileContainsOnlyAbstract"); }
        }
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; OnPropertyChanged("FilePath"); }
        }       
        public List<string> UnusualCharactersInTitle
        {
            get { return _unusualCharactersInTitle; }
            set { _unusualCharactersInTitle = value; OnPropertyChanged("UnusualCharactersInTitle"); }
        }
        public List<string> UnusualCharactersInAbstract
        {
            get { return _unusualCharactersInAbstract; }
            set { _unusualCharactersInAbstract = value; OnPropertyChanged("UnusualCharactersInAbstract"); }
        }

        // Constructor
        public ArticleForm()
        {
            this.Authors = new ObservableCollection<string>();
            this.Keywords = new ObservableCollection<string>();
            this.UnusualCharactersInTitle = new List<string>();
            this.UnusualCharactersInAbstract = new List<string>();
        }

        // Public methods
        public bool IsArticleValid()
        {
            return !String.IsNullOrWhiteSpace(Title) && (!String.IsNullOrWhiteSpace(Abstract) || HasNoAbstract) && HasNoErrors && TextFormat.IsValidText(Title) && TextFormat.IsValidText(Abstract);
        }
        public void ClearForm()
        {
            this.Title = null;
            this.Year = null;
            this.PersonalComment = null;
            this.Abstract = null;
            this.Authors.Clear();
            this.Keywords.Clear();
            this.SIC = false;
            this.FileContainsOnlyAbstract = false;
            this.FilePath = null;
            this.UnusualCharactersInTitle.Clear();
            this.UnusualCharactersInAbstract.Clear();
        }

        // Private helpers
        // Trims, changes every white space into singular space and removes unprintable characters
        private string FormatText(string input)
        {
            if (String.IsNullOrWhiteSpace(input))
            {
                return "";
            }

            return TextFormat.RemoveUnprintableCharacters(TextFormat.RemoveSpareWhiteSpace(TextFormat.RemoveLineBreaks(input)));
        }

        // IDataErrorInfo implementation
        // blank attribute waste from IDataErrorInfo not used in WPF but we have to have it in the interface
        public string Error { get { return null; } }

        // Dictionary for errors used to display them in tooltips (Title empty -> "Title can not be empty")
        public Dictionary<string, string> ErrorCollection { get; private set; } = new Dictionary<string, string>();

        // Bool used to Disable/Enable add and save buttons
        private bool _hasNoErrors;
        public bool HasNoErrors
        {
            get { return _hasNoErrors; }
            set { _hasNoErrors = value; OnPropertyChanged("HasNoErrors"); }
        }

        // Validation for textboxes
        string IDataErrorInfo.this[string PropertyName]
        {
            get
            {
                string error = null;
                bool check = true;

                switch (PropertyName)
                {
                    // If title is empty or contains unusual characters
                    case "Title":
                        if (String.IsNullOrWhiteSpace(Title))
                        {
                            error = "Title can not be empty";
                            break;
                        }
                        if (UnusualCharactersInTitle.Count > 0)
                        {
                            error = "Remove highlighted characters from title";
                            break;
                        }
                        break;
                    // If entered year is more than current date
                    case "Year":
                        if (String.IsNullOrEmpty(Year))
                        {
                            error = null;
                            break;
                        }
                        if (!int.TryParse(Year, out _))
                        {
                            error = "Year must be a number";
                            break;
                        }
                        if (int.Parse(Year) > DateTime.Now.Year)
                        {
                            error = Year + " is above current year - " + DateTime.Now.Year.ToString();
                            break;
                        }
                        break;
                    // If no file is selected
                    case "FilePath":
                        if (String.IsNullOrWhiteSpace(FilePath))
                            error = "File must be selected.";
                        break;
                    // If abstract is empty or contains unusual characters
                    case "Abstract":
                        if (String.IsNullOrWhiteSpace(Abstract))
                        {
                            error = "Abstract can not be empty";
                            break;
                        }
                        if (UnusualCharactersInAbstract.Count > 0)
                        {
                            error = "Remove highlighted characters from absract";
                            break;
                        }
                        break;

                }
                // if Dictionary already containts error for this property change it else add the error into dictionary
                // For example one textbox can have multiple validations (It can not be empty and minimum characters)
                // If error was set to "Title can not be empty" and now it is not empty but breaks the minimum character validation
                // Property was already in the dictionary and instead of adding we change its content to second validation error
                if (ErrorCollection.ContainsKey(PropertyName))
                    ErrorCollection[PropertyName] = error;
                else if (error != null)
                    ErrorCollection.Add(PropertyName, error);

                // Raise observable event and set CanAdd bool based on error
                OnPropertyChanged("ErrorCollection");
                foreach (KeyValuePair<string, string> entry in ErrorCollection)
                {
                    if (entry.Value != null)
                        check = false;
                }
                HasNoErrors = check;

                return error;
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
