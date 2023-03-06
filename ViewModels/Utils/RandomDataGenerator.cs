using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainLib.ViewModels.Utils
{
    internal class RandomDataGenerator
    {
        private Random rnd = new Random();
        private User user;
        private readonly Shared services;

        public ObservableCollection<string> ItemsCollection { get; set; }

        public ICommand GenerateRandomArticlesCommand { get; set; }

        public RandomDataGenerator()
        {
            this.services = Shared.GetInstance();
            this.user = services.User;
            ItemsCollection = new ObservableCollection<string>();
            this.GenerateRandomArticlesCommand = new RelayCommand(GenerateRandomArticles);
        }

        // TEST
        public async void GenerateRandomArticles(object input = null)
        {
            List<Article> random_articles = new List<Article>();
            Article random_article;
            string prevTitle = "";

            services.IsWorking(true);

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
                    new ArticleRepo().SaveArticle(article, user);

            });

            services.IsWorking(false);

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
    }
}
