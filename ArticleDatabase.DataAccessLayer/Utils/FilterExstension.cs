using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lib.DataAccessLayer.Utils
{
    public static class FilterExstension
    {
        //(?i)(?<!\w)(?:\w?{word}|{word}\w?)(?!\w)
        //(?i)(?<!\w){word}(?!\w)
        private static string wordRegex = @"'(?i)(?<!\w)(?:\w?{word}|{word}\w?)(?!\w)'";

        // Usually pairing between different criterias is AND, for example
        // when we set both title and author, we want the pairing to be AND 
        // between them, but in simple search we want to change it to OR
        private static string GlobalPairing = " AND ";
        public static void SetGlobalPairing(bool isLoose)
        {
            if (isLoose)
                GlobalPairing = " OR ";
            else
                GlobalPairing = " AND ";

        }

        public static Filter FilterTitle(this Filter filter, string[] words, string[] phrases)
        {
            // If title is blank return
            if ((words.Length == 0 && phrases.Length == 0)) { return filter; }


            AppendModifed(filter);

            filter.FilterQuery.Append("(");

            // Append each word as wildcard
            for (int i = 0; i < words.Length; i++)
            {
                //filter.FilterQuery.Append("final.Title REGEXP " + $@"'(?i)\b{words[i]}\b'");
                //filter.FilterQuery.Append("final.Title REGEXP " + $@"'(?i)(?<!\w){words[i].Replace("(", "\\(").Replace(")", "\\)")}(?!\w)'");
                //(?i)\b[a-z]?{words[i].Replace("(", "\\(").Replace(")", "\\)")}[a-z]?\b
                //(?i)(?<!\w)(?:\w?{words[i].Replace("(", "\\(").Replace(")", "\\)")}|{words[i].Replace("(", "\\(").Replace(")", "\\)")}\w?)(?!\w)
                //(?i)(?<=^|[^a-z]|\b[a-z])({words[i].Replace("(", "\\(").Replace(")", "\\)")})(?=$|[^a-z]|[a-z]\b)
                string escapeWord = Regex.Escape(words[i]);
                escapeWord = escapeWord.Replace("'", "''");
                filter.FilterQuery.Append("final.Title REGEXP " + wordRegex.Replace("{word}", escapeWord));

                if (i < words.Length - 1 || phrases.Length > 0)
                {
                    filter.FilterQuery.Append(" AND ");
                }
            }

            // Append each phrase as wildcard
            for (int i = 0; i < phrases.Length; i++)
            {
                string escapePhrase = Regex.Escape(phrases[i]);
                escapePhrase = escapePhrase.Replace("'", "''");
                filter.FilterQuery.Append("final.Title LIKE " + ToWildCard(escapePhrase));

                if (i < phrases.Length - 1)
                {
                    filter.FilterQuery.Append(" AND ");
                }
            }

            filter.FilterQuery.Append(")");

            filter.Modified = true;
            filter.FilterQuery.Append(" ");
            return filter;
        }
        public static Filter FilterAuthors(this Filter filter, List<string> authors, string pairing)
        {
            // If there are no author filters return
            if (authors.Count == 0) { return filter; }

            filter.AuthorFilterQuery.Append("WHERE art.ID IN\r\n");
            filter.AuthorFilterQuery.Append("(");

            string[] names;
            foreach (string author in authors)
            {
                filter.AuthorFilterQuery.Append("SELECT aa.Article_ID\r\nFROM jntArticleAuthor AS aa\r\nLEFT JOIN tblAuthor AS ath ON aa.Author_ID = ath.ID\r\nWHERE ");
                names = author.Split(' ');

                if (names.Length == 1)
                {
                    filter.AuthorFilterQuery.Append("ath.Name LIKE " + ToWildCard(author) + "\r\n");
                }
                else
                {
                    filter.AuthorFilterQuery.Append($@"ath.Name REGEXP '(?i)\b{names[0][0]}[\w\.]*.*\b{names[names.Length - 1]}\b'" + "\r\n");
                }
                

                // If its not the last iteration add "AND"
                if (author != authors.Last())
                {
                    filter.AuthorFilterQuery.Append("INTERSECT\r\n");
                }
            }

            filter.AuthorFilterQuery.Append(")");
            filter.AuthorFilterQuery.Append(" ");
            //filter.Modified = true;
            return filter;
        }
        public static Filter FilterKeywords(this Filter filter, List<string> keywords, string pairing)
        {
            if (keywords.Count == 0) { return filter; }

            AppendModifed(filter);

            filter.FilterQuery.Append("(");

            filter.FilterQuery.Append("final.Keywords IS NOT NULL AND ");

            foreach (string keyword in keywords)
            {
                string escapeKeyword = keyword.Replace("'", "''");
                filter.FilterQuery.Append("final.Keywords LIKE " + ToWildCard(escapeKeyword));

                // If its not the last iteration add "AND"
                if (keyword != keywords.Last())
                    filter.FilterQuery.Append($" {pairing} ");
            }

            filter.FilterQuery.Append(")");

            filter.FilterQuery.Append(" ");
            filter.Modified = true;
            return filter;
        }
        public static Filter FilterYear(this Filter filter, string year)
        {
            if (String.IsNullOrEmpty(year)) { return filter; }

            AppendModifed(filter);

            filter.FilterQuery.Append("(");

            if (year.Contains("-"))
            {
                string[] dates = year.Split('-');
                if (dates.Length == 2 && !string.IsNullOrWhiteSpace(dates[1]))
                    filter.FilterQuery.Append($" final.year >= {dates[0]} AND year <= {dates[1]}");
                else
                    filter.FilterQuery.Append($" final.year = {dates[0]}");
            }
            else
            {
                filter.FilterQuery.Append($" final.year = {year}");
            }

            filter.FilterQuery.Append(")");

            filter.FilterQuery.Append(" ");
            filter.Modified = true;
            return filter;
        }
        public static Filter FilterPersonalComment(this Filter filter, string personalComment)
        {
            if (String.IsNullOrEmpty(personalComment)) { return filter; }

            AppendModifed(filter);

            filter.FilterQuery.Append("(");

            filter.FilterQuery.Append($" final.PersonalComment LIKE {ToWildCard(personalComment)}");

            filter.FilterQuery.Append(")");

            filter.FilterQuery.Append(" ");
            filter.Modified = true;
            return filter;
        }
        public static Filter FilterIds(this Filter filter, int[] idFilters)
        {
            if (idFilters == null) { return filter; }

            AppendModifed(filter);

            filter.FilterQuery.Append("(");

            filter.FilterQuery.Append($" final.ID >= {idFilters[0]} AND final.ID <= {idFilters[1]}");

            filter.FilterQuery.Append(")");

            filter.FilterQuery.Append(" ");
            filter.Modified = true;
            return filter;
        }
        public static Filter FilterAbstract(this Filter filter, string[] words, string[] phrases)
        {
            if ((words.Length == 0 && phrases.Length == 0)) { return filter; }
            
            AppendModifed(filter);

            filter.FilterQuery.Append("(");

            // Append each word as wildcard
            for (int i = 0; i < words.Length; i++)
            {
                //filter.FilterQuery.Append("abst.Body REGEXP " + $@"'(?i)\b{words[i]}\b'");
                //filter.FilterQuery.Append("abst.Body REGEXP " + $@"'(?i)(?<!\w){words[i]}(?!\w)'");
                //(?i)\b[a-z]?{words[i]}[a-z]?\b
                //(?i)(?<!\w)(?:\w?{words[i]}|{words[i]}\w?)(?!\w)
                //(?i)(?<=^|[^a-z]|\b[a-z])({words[i]})(?=$|[^a-z]|[a-z]\b)
                string escapeWord = Regex.Escape(words[i]);
                escapeWord = escapeWord.Replace("'", "''");
                filter.FilterQuery.Append("abst.Body REGEXP " + wordRegex.Replace("{word}", escapeWord));

                if (i < words.Length - 1 || phrases.Length > 0)
                {
                    filter.FilterQuery.Append(" AND ");
                }
            }

            // Append each phrase as wildcard
            for (int i = 0; i < phrases.Length; i++)
            {
                filter.FilterQuery.Append("abst.Body LIKE " + ToWildCard(phrases[i]));

                if (i < phrases.Length - 1)
                {
                    filter.FilterQuery.Append(" AND ");
                }
            }

            filter.FilterQuery.Append(")");

            filter.Modified = true;
            filter.FilterQuery.Append(" ");
            return filter;
        }
        public static Filter Sort(this Filter filter, string sort = "Title ASC")
        {
            filter.FilterQuery.Append($" ORDER BY {sort}");

            filter.FilterQuery.Append(" ");
            filter.Modified = true;
            return filter;
        }
        // This doesn't return Filter because this should be the last filter to add
        public static void Paginate(this Filter filter, int itemsPerPage, int offset)
        {
            filter.Modified = true;
            filter.FilterQuery.Append(" LIMIT " + itemsPerPage.ToString() + " OFFSET " + offset.ToString() + ";");
        }

        // Checks if filter was already modifed, if it was we have to add AND
        private static void AppendModifed(Filter filter)
        {
            filter.FilterQuery.Append(filter.Modified ? GlobalPairing : " WHERE ");
        }
        private static string ToWildCard(string input)
        {
            return "'%" + input + "%'";
        }
        private static string ToWildCardUpper(string input)
        {
            return "'% " + input + " %'";
        }
        private static bool IsAllUpper(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (!Char.IsUpper(input[i]))
                    return false;
            }

            return true;
        }
    }
}
