using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ViewModels.Base;
using ViewModels.Commands;
using ViewModels.Services.Browser;

namespace ViewModels.Main
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

        #region Private memebers
        private string _author;
        private string _keyword;
        private string _selectedFile;
        private IBrowserService _browserService;

        // TEST
        private Random rnd = new Random();
        #endregion

        #region Public properties
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
        #endregion

        #region Commands
        public RelayCommand SelectFileCommand { get; set; }
        public RelayCommand SaveArticleCommand { get; set; }
        public RelayCommand ClearArticleAttributesCommand { get; set; }
        #endregion

        // TEST
        public ObservableCollection<string> ItemsCollection { get; set; }

        // Constructor
        public DataEntryViewModel(User user, IBrowserService browserService)
        {
            // 1. Initialize article and User
            Article = new Article();
            User = user;
            this._browserService = browserService;

            // 2. Initialize commands
            SelectFileCommand = new RelayCommand(SelectFile);
            SaveArticleCommand = new RelayCommand(SaveArticle, CanSaveArticle);
            ClearArticleAttributesCommand = new RelayCommand(ClearArticleAttributes);

            // TEST
            GenerateRandomArticlesCommand = new RelayCommand(GenerateRandomArticles);
            ItemsCollection = new ObservableCollection<string>();
        }

        #region Command actions
        public void SelectFile(object input = null)
        {
            /*
            // 1. Create new dialog window
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // 2. Set filter for file extension and default file extension 
            dlg.DefaultExt = ".pdf";
            dlg.Filter = "PDF files (*.pdf)|*.pdf";


            // 3. Display the dialog window
            Nullable<bool> result = dlg.ShowDialog();
            */

            string result = _browserService.OpenFileDialog(".pdf", "PDF files (*.pdf)|*.pdf");


            // 4. Get the selected file
            SelectedFile = result;
        }
        public void SaveArticle(object input = null)
        {
            // Regex to switch multiple spaces into one (Restricts user to enter more than one space in Title textboxes)
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);

            // 1. Format title
            Article.Title = Article.Title.Trim();
            Article.Title = regex.Replace(Article.Title, " ");

            // 2. Add article to database
            (new ArticleRepo()).SaveArticle(Article, User);

            // 3. Copy selected file to root folder with the new ID-based name
            File.Copy(SelectedFile, Path.Combine(Environment.CurrentDirectory, "Files\\") + Article.FileName + ".pdf");

            // 4. Move the selected file into "Done" subfolder
            string done_path = Path.GetDirectoryName(SelectedFile) + "\\Done\\";
            Directory.CreateDirectory(Path.GetDirectoryName(SelectedFile) + "\\Done");
            File.Move(SelectedFile, done_path + System.IO.Path.GetFileName(SelectedFile));

            // 5. Clear article attributes
            ClearArticleAttributesCommand.Execute(null);
        }
        public void ClearArticleAttributes(object input = null)
        {
            Article.Clear();
            SelectedFile = null;
        }

        public bool CanSaveArticle(object input = null)
        {
            return CanAdd && Article.CanAdd;
        }
        #endregion

        #region Test
        public RelayCommand GenerateRandomArticlesCommand { get; set; }

        // TEST
        public void GenerateRandomArticles(object input = null)
        {
            List<Article> random_articles = new List<Article>();
            Article random_article;
            string prevTitle = "";

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
                (new ArticleRepo()).SaveArticle(article, User);

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
