﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Lib.DataAccessLayer.Utils
{
    public static class FilterExstension
    {
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

        public static Filter FilterTitle(this Filter filter, string title, bool wordBreak)
        {
            // If title is blank return
            if (String.IsNullOrEmpty(title))
            {
                return filter;
            }

            // Double single quotes (') to avoid conflict with SQL query
            string formatedTitle = title.Replace("'", "''");

            AppendModifed(filter);

            // Append each word as wildcard
            filter.FilterQuery.Append("(");
            if (wordBreak)
            {
                string[] words = formatedTitle.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    if (IsAllUpper(words[i]))
                    {
                        filter.FilterQuery.Append("final.Title LIKE " + ToWildCardUpper(words[i]));
                    }
                    else
                    {
                        filter.FilterQuery.Append("final.Title LIKE " + ToWildCard(words[i]));
                    }

                    if (i < words.Length - 1)
                    {
                        filter.FilterQuery.Append(" AND ");
                    }
                }
            }
            // Append as whole substring
            else
            {
                filter.FilterQuery.Append("final.Title LIKE " + ToWildCard(formatedTitle));
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

            AppendModifed(filter);

            filter.FilterQuery.Append("(");
            foreach (string author in authors)
            {
                filter.FilterQuery.Append("final.Authors LIKE " + ToWildCard(author));

                // If its not the last iteration add "AND"
                if (author != authors.Last())
                    filter.FilterQuery.Append($" {pairing} ");
            }

            filter.FilterQuery.Append(")");

            filter.FilterQuery.Append(" ");
            filter.Modified = true;
            return filter;
        }
        public static Filter FilterKeywords(this Filter filter, List<string> keywords, string pairing)
        {
            if (keywords.Count == 0) { return filter; }

            AppendModifed(filter);

            filter.FilterQuery.Append("(");

            foreach (string keyword in keywords)
            {
                filter.FilterQuery.Append("final.Keywords LIKE " + ToWildCard(keyword));

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

            filter.FilterQuery.Append($" ID >= {idFilters[0]} AND ID <= {idFilters[1]}");

            filter.FilterQuery.Append(")");

            filter.FilterQuery.Append(" ");
            filter.Modified = true;
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