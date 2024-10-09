using MainLib.ViewModels.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainLib.Views.UserControls
{
    /// <summary>
    /// Interaction logic for RecentSearchesList.xaml
    /// </summary>
    public partial class RecentSearchesList : UserControl
    {
        private static readonly DependencyProperty RecentSearchesItemSourceProperty =
    DependencyProperty.Register("RecentSearchesItemSource", typeof(ObservableCollection<SearchEntry>), typeof(RecentSearchesList));

        public ObservableCollection<SearchEntry> RecentSearchesItemSource
        {
            get { return (ObservableCollection<SearchEntry>)GetValue(RecentSearchesItemSourceProperty); }
            set { SetValue(RecentSearchesItemSourceProperty, value); }
        }

        private static readonly DependencyProperty FavoriteSearchesItemSourceProperty =
DependencyProperty.Register("FavoriteSearchesItemSource", typeof(ObservableCollection<SearchEntry>), typeof(RecentSearchesList));

        public ObservableCollection<SearchEntry> FavoriteSearchesItemSource
        {
            get { return (ObservableCollection<SearchEntry>)GetValue(FavoriteSearchesItemSourceProperty); }
            set { SetValue(FavoriteSearchesItemSourceProperty, value); }
        }

        private static readonly DependencyProperty ApplyRecentSearchCommandProperty =
DependencyProperty.Register("ApplyRecentSearchCommand", typeof(ICommand), typeof(RecentSearchesList));

        public ICommand ApplyRecentSearchCommand
        {
            get { return (ICommand)GetValue(ApplyRecentSearchCommandProperty); }
            set { SetValue(ApplyRecentSearchCommandProperty, value); }
        }

        private static readonly DependencyProperty ToggleFavoriteSearchCommandProperty =
DependencyProperty.Register("ToggleFavoriteSearchCommand", typeof(ICommand), typeof(RecentSearchesList));

        public ICommand ToggleFavoriteSearchCommand
        {
            get { return (ICommand)GetValue(ToggleFavoriteSearchCommandProperty); }
            set { SetValue(ToggleFavoriteSearchCommandProperty, value); }
        }

        public RecentSearchesList()
        {
            InitializeComponent();
        }
    }
}
