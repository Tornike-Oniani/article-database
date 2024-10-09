using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Classes
{
    public class SearchEntryComparer : IEqualityComparer<SearchEntry>
    {
        public bool Equals(SearchEntry x, SearchEntry y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return String.Equals(x.Terms ?? String.Empty, y.Terms ?? String.Empty, StringComparison.OrdinalIgnoreCase) &&
                String.Equals(x.Authors ?? String.Empty, y.Authors ?? String.Empty, StringComparison.OrdinalIgnoreCase) &&
                String.Equals(x.Year ?? String.Empty, y.Year ?? String.Empty, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(SearchEntry obj)
        {
            if (obj == null)
            {
                return 0;
            }

            int hashTerms = (obj.Terms ?? String.Empty).GetHashCode();
            int hashAuthors = (obj.Authors ?? String.Empty).GetHashCode();
            int hashYear = (obj.Year ?? String.Empty).GetHashCode();

            return hashTerms ^ hashAuthors ^ hashYear;
        }
    }
}
