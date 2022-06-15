using Lib.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.DataAccessLayer.Utils
{
    public static class FilterExstension
    {
        public static Filter FilterTitle(this Filter filter, string title, bool wordBreak)
        {
            AppendModifed(filter);

            if (wordBreak)
            {
                string[] words = title.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    filter.FilterQuery.Append("final.Title LIKE " + ToWildCard(words[i]));

                    if (i < words.Length - 1)
                    {
                        filter.FilterQuery.Append(" AND ");
                    }
                }
                return filter;
            }
            else
            {
                filter.FilterQuery.Append("final.Title LIKE " + ToWildCard(title));
                filter.Modified = true;
                return filter;
            }
        }
        public static Filter FilterAuthors(this Filter filter, List<string> authors, string pairing)
        {
            AppendModifed(filter);

            foreach (string author in authors)
            {
                filter.FilterQuery.Append("final.Authors LIKE " + ToWildCard(author));

                // If its not the last iteration add "AND"
                if (author != authors.Last())
                    filter.FilterQuery.Append($" {pairing} ");
            }

            return filter;
        }
        public static Filter FilterKeywords(this Filter filter, List<string> keywords, string pairing)
        {
            AppendModifed(filter);

            foreach (string keyword in keywords)
            {
                filter.FilterQuery.Append("final.Keywords LIKE " + ToWildCard(keyword));

                // If its not the last iteration add "AND"
                if (keyword != keywords.Last())
                    filter.FilterQuery.Append($" {pairing} ");
            }

            return filter;
        }
        public static Filter FilterYear(this Filter filter, string year)
        {
            AppendModifed(filter);

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

            return filter;
        }
        public static Filter FilterPersonalComment(this Filter filter, string personalComment)
        {
            AppendModifed(filter);

            filter.FilterQuery.Append($" final.PersonalComment LIKE {ToWildCard(personalComment)}");

            return filter;
        }
        public static Filter FilterIds(this Filter filter, int[] idFilters)
        {
            AppendModifed(filter);

            filter.FilterQuery.Append($" ID >= {idFilters[0]} AND ID <= {idFilters[1]}");

            return filter;
        }
        public static Filter Sort(this Filter filter, string sort)
        {
            AppendModifed(filter);

            filter.FilterQuery.Append($" ORDER BY {sort}");

            return filter;
        }

        // Checks if filter was already modifed, if it was we have to add AND
        private static void AppendModifed(Filter filter)
        {
            filter.FilterQuery.Append(filter.Modified ? " AND " : "");
        }
        private static string ToWildCard(string input)
        {
            return "'%" + input + "%'";
        }
    }
}
