using Lib.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lib.DataAccessLayer.Utils
{
    // We are using REGEXP to filter results from SQLITE, but REGEXP sometimes doesn't work correctly and also deosn't support certain regex syntax, so we use this class to filter the returned resutls again with desired regex
    public class ExtraFilter
    {
        #region Private members
        //(?i)(?<!\w)(?:\w?{text}|{text}\w?)(?!\w)
        //(?i)(?<!\w){text}(?!\w)
        private string regex = @"(?i)(?<!\w)(?:\w?{text}|{text}\w?)(?!\w)";
        #endregion

        #region Public methods
        public List<Article> FilterArticlesWithTerms(List<Article> articles, List<string> termWords, List<string> termPhrases)
        {
            if ( termWords.Count == 0 && termPhrases.Count == 0)
            {
                return articles;
            }

            return articles
                .Where(article => 
                (termWords.All(word => Regex.IsMatch(article.Title, Regexify(word)) ||
                                       Regex.IsMatch(article.AbstractBody, $@"(?i)(?<!\w){word}(?!\w)"))) 
                &&
                (termPhrases.All(phrase => article.Title.Contains(phrase) ||
                                            article.AbstractBody.Contains(phrase)))
                ).ToList();
        }
        #endregion

        #region Private helpers
        private string Regexify(string text)
        {
            return regex.Replace("{text}", Regex.Escape(text));
        }
        #endregion
    }
}
