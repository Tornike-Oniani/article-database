using MainLib.ViewModels.Classes;
using MainLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Classes
{
    public class SearchHistoryManager
    {
        #region Private helpers
        private JSONWriterReader<SearchHistory> jsonWriterReader;
        #endregion

        #region Public properties
        public SearchHistory SearchHistory { get; set; }
        #endregion

        #region Constructors
        public SearchHistoryManager()
        {
            this.jsonWriterReader = new JSONWriterReader<SearchHistory>(Path.Combine(Environment.CurrentDirectory, "search_history.json"));
            try
            {
                this.SearchHistory = jsonWriterReader.ReadData();
            }
            catch
            {
                this.SearchHistory = new SearchHistory();
            }
        }
        #endregion

        #region Public methods
        public void AddSearchToHistory(SearchEntry searchEntry)
        {
            if (this.SearchHistory.RecentSearches.Contains(searchEntry, new SearchEntryComparer()))
            {
                return;
            }

            this.SearchHistory.RecentSearches.Insert(0, searchEntry);
            jsonWriterReader.WriteData(SearchHistory);
        }
        public void AddSearchToFavorites(SearchEntry searchEntry)
        {
            if (this.SearchHistory.FavoriteSearches.Contains(searchEntry, new SearchEntryComparer()))
            {
                return;
            }
            this.SearchHistory.FavoriteSearches.Insert(0, searchEntry);
            jsonWriterReader.WriteData(SearchHistory);
        }
        public void RemoveSearchFromFavorites(SearchEntry searchEntry)
        {
            SearchEntry searchEntryToRemove = this.SearchHistory.FavoriteSearches.Single(item => new SearchEntryComparer().Equals(item, searchEntry));
            this.SearchHistory.FavoriteSearches.Remove(searchEntryToRemove);
            // If we remove favorite from favorites list we have to make sure the IsFavorite property is also set to false to recent search item equivalent
            SearchEntry searchEntryToUnFavorite = this.SearchHistory.RecentSearches.SingleOrDefault(item => new SearchEntryComparer().Equals(item, searchEntry));
            if (searchEntryToUnFavorite != null && searchEntryToUnFavorite.IsFavorite)
            {
                searchEntryToUnFavorite.IsFavorite = false;
            }
            jsonWriterReader.WriteData(SearchHistory);
        }
        public void DeleteSearchFromHstory(SearchEntry searchEntry)
        {
            SearchEntry searchEntryToRemove = this.SearchHistory.RecentSearches.Single(item => new SearchEntryComparer().Equals(item, searchEntry));
            this.SearchHistory.RecentSearches.Remove(searchEntryToRemove);
            jsonWriterReader.WriteData(SearchHistory);
        }
        public void ClearSearchHistory()
        {
            this.SearchHistory.RecentSearches.Clear();
            jsonWriterReader.WriteData(SearchHistory);
        }
        #endregion
    }
}