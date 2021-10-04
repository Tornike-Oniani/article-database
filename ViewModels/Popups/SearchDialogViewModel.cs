using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Main;

namespace MainLib.ViewModels.Popups
{
    public class SearchDialogViewModel : BaseViewModel
    {
        public DataViewViewModel Parent { get; set; }

        public ICommand SearchCommand { get; set; }
        public ObservableCollection<string> AuthorPairings { get; set; }
        public ObservableCollection<string> KeywordPairings { get; set; }

        public SearchDialogViewModel(DataViewViewModel parent)
        {
            this.Title = "Set filter...";
            this.Parent = parent;
            this.AuthorPairings = new ObservableCollection<string>() { "AND", "OR" };
            this.KeywordPairings = new ObservableCollection<string>() { "AND", "OR" };
            if (String.IsNullOrWhiteSpace(parent.SelectedAuthorPairing))
                parent.SelectedAuthorPairing = AuthorPairings[0];
            if (String.IsNullOrWhiteSpace(parent.SelectedKeywordPairing))
                parent.SelectedKeywordPairing = KeywordPairings[0];

            this.SearchCommand = new RelayCommand(Search);
        }

        public void Search(object input = null)
        {
            Parent.LoadArticlesCommand.Execute(null);
            this.Window.Close();
        }
    }
}
