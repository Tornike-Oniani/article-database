using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Browser;
using Lib.ViewModels.Services.Windows;
using MainLib.ViewModels.Popups;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Utils;
using Lib.DataAccessLayer.Info;

namespace MainLib.ViewModels.Main
{
    public class DataEntryViewModel : BaseViewModel, IDataErrorInfo
    {
        #region Validation
        // blank attribute waste from IDataErrorInfo not used in WPF but we have to have it in the interface
        public string Error { get { return null; } }

        // Dictionary for errors used to display them in tooltips (Title empty -> "Title can not be empty")
        public Dictionary<string, string> ErrorCollection { get; private set; } = new Dictionary<string, string>();

        // Bool used to Disable/Enable add and save buttons
        private bool _canAdd;
        public bool CanAdd
        {
            get { return _canAdd; }
            set { _canAdd = value; OnPropertyChanged("CanAdd"); }
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
                    // If title is empty set error string
                    case "SelectedFile":
                        if (string.IsNullOrEmpty(SelectedFile))
                            error = "File must be selected.";
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
                CanAdd = check;

                return error;
            }
        }
        #endregion

        // Private memebers
        private string _author;
        private string _keyword;
        private string _selectedFile;
        private Action<bool> _workingStatus;
        private IDialogService _dialogService;
        private IWindowService _windowService;
        private IBrowserService _browserService;

        // TEST
        private Random rnd = new Random();

        // Public properties
        public User User { get; set; }
        public Article Article { get; set; }
        public string Author
        {
            get { return _author; }
            set { _author = value; OnPropertyChanged("Author"); }
        }
        public string Keyword
        {
            get { return _keyword; }
            set { _keyword = value; OnPropertyChanged("Keyword"); }
        }
        public string SelectedFile
        {
            get { return _selectedFile; }
            set { _selectedFile = value; OnPropertyChanged("SelectedFile"); }
        }
        public List<Bookmark> Bookmarks { get; set; }
        public List<Reference> References { get; set; }

        // Commands
        public RelayCommand SelectFileCommand { get; set; }
        public RelayCommand SaveArticleCommand { get; set; }
        public RelayCommand ClearArticleAttributesCommand { get; set; }
        public RelayCommand OpenBookmarkManagerCommand { get; set; }
        public RelayCommand OpenReferenceManagerCommand { get; set; }

        // TEST
        public ObservableCollection<string> ItemsCollection { get; set; }

        // Constructor
        public DataEntryViewModel(User user, Action<bool> workingStatus, IDialogService dialogService, IWindowService windowService, IBrowserService browserService)
        {
            // 1. Initialize article and User
            this.Article = new Article();
            this.User = user;
            this._workingStatus = workingStatus;
            this.Bookmarks = new List<Bookmark>();
            this.References = new List<Reference>();
            this._dialogService = dialogService;
            this._windowService = windowService;
            this._browserService = browserService;

            // 2. Initialize commands
            SelectFileCommand = new RelayCommand(SelectFile);
            SaveArticleCommand = new RelayCommand(SaveArticle, CanSaveArticle);
            ClearArticleAttributesCommand = new RelayCommand(ClearArticleAttributes);
            OpenBookmarkManagerCommand = new RelayCommand(OpenBookmarkManager);
            OpenReferenceManagerCommand = new RelayCommand(OpenReferenceManager);

            // TEST
            GenerateRandomArticlesCommand = new RelayCommand(GenerateRandomArticles);
            ItemsCollection = new ObservableCollection<string>();
        }

        // Command actions
        public void SelectFile(object input = null)
        {
            try
            {
                string result = _browserService.OpenFileDialog(".pdf", "PDF files (*.pdf)|*.pdf");

                // Get the selected file
                SelectedFile = result;
            }
            catch(Exception e)
            {
                new BugTracker().Track("Data Entry", "Select file", e.Message);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
        }
        public async void SaveArticle(object input = null)
        {
            try
            {
                _workingStatus(true);

                await Task.Run(() =>
                {
                    ArticleRepo articleRepo = new ArticleRepo();

                    // Regex to switch multiple spaces into one (Restricts user to enter more than one space in Title textboxes)
                    RegexOptions options = RegexOptions.None;
                    Regex regex = new Regex("[ ]{2,}", options);

                    // 1. Format title
                    Article.Title = Article.Title.Trim();
                    Article.Title = regex.Replace(Article.Title, " ");

                    // 2. Add article to database
                    articleRepo.SaveArticle(Article, User);

                    // 3. Copy selected file to root folder with the new ID-based name
                    File.Copy(SelectedFile, Path.Combine(Environment.CurrentDirectory, "Files\\") + Article.FileName + ".pdf");

                    // 4.1 Retrieve id (in reality we retrieve whole article) of newly added article
                    Article currently_added_article = articleRepo.GetArticleWithTitle(Article.Title);

                    // 4.2 Add bookmarks
                    foreach (Bookmark bookmark in Bookmarks)
                        new BookmarkRepo().AddArticleToBookmark(bookmark, currently_added_article);

                    // 4.3 Add references
                    foreach (Reference reference in References)
                        new ReferenceRepo().AddArticleToReference(reference, currently_added_article);

                    // 4.4 Tracking
                    ArticleInfo info = new ArticleInfo(User, Article.Title, Bookmarks, References);
                    Tracker tracker = new Tracker(User);
                    tracker.TrackCreate<ArticleInfo>(info);
                    File.Copy(SelectedFile, tracker.GetFilesPath() + "\\" + Article.FileName + ".pdf");

                    // 6. Move the selected file into "Done" subfolder
                    string done_path = Path.GetDirectoryName(SelectedFile) + "\\Done\\";
                    Directory.CreateDirectory(Path.GetDirectoryName(SelectedFile) + "\\Done");
                    File.Move(SelectedFile, done_path + System.IO.Path.GetFileName(SelectedFile));
                });

                // 5. Clear article attributes
                ClearArticleAttributesCommand.Execute(null);
                Bookmarks.Clear();
                References.Clear();

                _workingStatus(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data Entry", "Add article", e.Message);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                _workingStatus(false);
            }
        }
        public void ClearArticleAttributes(object input = null)
        {
            try
            {
                Article.Clear();
                SelectedFile = null;
                Bookmarks.Clear();
                References.Clear();
            }
            catch(Exception e)
            {
                new BugTracker().Track("Data Entry", "Clear Article Attributes", e.Message);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
        }
        public void OpenBookmarkManager(object input = null)
        {
            _windowService.OpenWindow(new BookmarkManagerViewModel(
                User,
                ViewType.DataEntry,
                _dialogService,
                bookmarks: Bookmarks
                ));
        }
        public void OpenReferenceManager(object input = null)
        {
            _windowService.OpenWindow(new ReferenceManagerViewModel(
                ViewType.DataEntry,
                _dialogService,
                references: References
                ));
        }
        public bool CanSaveArticle(object input = null)
        {
            return CanAdd && Article.CanAdd;
        }

        #region Test
        public RelayCommand GenerateRandomArticlesCommand { get; set; }

        // TEST
        public async void GenerateRandomArticles(object input = null)
        {
            List<Article> random_articles = new List<Article>();
            Article random_article;
            string prevTitle = "";

            _workingStatus(true);

            await Task.Run(() =>
            {
                for (int i = 0; i < 15; i++)
                {
                    random_article = new Article();

                    random_article.Title = RandomTitle(prevTitle);

                    for (int k = 1; k < rnd.Next(3, 7); k++)
                    {
                        random_article.AuthorsCollection.Add(RandomAuthor());
                    }

                    for (int k = 1; k < rnd.Next(3, 7); k++)
                    {
                        random_article.KeywordsCollection.Add(RandomKeyword());
                    }

                    random_article.Year = RandomYear();

                    random_article.PersonalComment = RandomComment();

                    random_article.SIC = RandomSIC();

                    random_articles.Add(random_article);

                    prevTitle = random_article.Title;
                }

                foreach (Article article in random_articles)
                    new ArticleRepo().SaveArticle(article, User);

            });

            _workingStatus(false);

            Console.WriteLine("Done");
        }

        // TEST
        private string RandomTitle(string prevTitle)
        {

            int word_length = rnd.Next(15, 30);
            string title = "";
            string finTitle;

            for (int i = 0; i < word_length; i++)
            {
                title += GenerateRandomWord(5, 10) + " ";
            }

            finTitle = title.First().ToString().ToUpper() + title.Substring(1);

            while (finTitle == prevTitle)
            {
                word_length = rnd.Next(15, 30);
                title = "";

                for (int i = 0; i < word_length; i++)
                {
                    title += GenerateRandomWord(5, 10) + " ";
                }

                finTitle = title.First().ToString().ToUpper() + title.Substring(1);
            }

            return finTitle;
        }
        private int RandomSIC()
        {
            if (rnd.Next(0, 100) < 70)
                return 0;

            return 1;
        }
        private string RandomComment()
        {
            int word_length = rnd.Next(3, 5);
            string comment = "";

            for (int i = 0; i < word_length; i++)
            {
                comment += GenerateRandomWord(5, 10) + " ";
            }

            return comment;
        }
        private int RandomYear()
        {
            return rnd.Next(1900, DateTime.Now.Year);
        }
        private string RandomKeyword()
        {
            return GenerateRandomWord(5, 8);
        }
        private string RandomAuthor()
        {
            string[] middle = new string[] { "A.", "R.", "J.", "K.", "L.", "O.", "S." };

            string Author = "";
            string word;

            word = GenerateRandomWord(4, 8);
            word = word.First().ToString().ToUpper() + word.Substring(1) + " ";

            Author += word;

            if (rnd.Next(0, 100) < 75)
                Author += middle[rnd.Next(0, middle.Length - 1)] + " ";

            word = GenerateRandomWord(4, 8);
            word = word.First().ToString().ToUpper() + word.Substring(1);

            Author += word;

            return Author;
        }
        private string GenerateRandomWord(int min, int max)
        {
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" };
            string[] vowels = { "a", "e", "i", "o", "u" };


            string word = "";

            int requestedLength = rnd.Next(min, max);

            // Generate the word in consonant / vowel pairs
            while (word.Length < requestedLength)
            {
                if (requestedLength != 1)
                {
                    // Add the consonant
                    string consonant = GetRandomLetter(rnd, consonants);

                    if (consonant == "q" && word.Length + 3 <= requestedLength) // check +3 because we'd add 3 characters in this case, the "qu" and the vowel.  Change 3 to 2 to allow words that end in "qu"
                    {
                        word += "qu";
                    }
                    else
                    {
                        while (consonant == "q")
                        {
                            // Replace an orphaned "q"
                            consonant = GetRandomLetter(rnd, consonants);
                        }

                        if (word.Length + 1 <= requestedLength)
                        {
                            // Only add a consonant if there's enough room remaining
                            word += consonant;
                        }
                    }
                }

                if (word.Length + 1 <= requestedLength)
                {
                    // Only add a vowel if there's enough room remaining
                    word += GetRandomLetter(rnd, vowels);
                }
            }

            return word;
        }
        private string GetRandomLetter(Random rnd, string[] letters)
        {
            return letters[rnd.Next(0, letters.Length - 1)];
        }
        #endregion

    }
}
