using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Classes
{
    public class SearchHistory
    {
        public List<SearchEntry> RecentSearches { get; set; }
        public List<SearchEntry> FavoriteSearches { get; set; }

        public SearchHistory()
        {
            this.RecentSearches = new List<SearchEntry>();
            this.FavoriteSearches = new List<SearchEntry>();
        }
    }
}
