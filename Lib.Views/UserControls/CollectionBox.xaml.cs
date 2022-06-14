using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using Lib.Views.Services.Dialogs;
using Lib.Views.Services.Windows;

namespace Lib.Views.UserControls
{
    public enum InputRestriction
    {
        None,
        Author,
        Keyword
    }

    /// <summary>
    /// Interaction logic for CollectionBox.xaml
    /// </summary>
    public partial class CollectionBox : UserControl, INotifyPropertyChanged
    {
        private static readonly DependencyProperty TitleProperty =
    DependencyProperty.Register("Title", typeof(string), typeof(CollectionBox), new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        private static readonly DependencyProperty TextProperty =
    DependencyProperty.Register("Text", typeof(string), typeof(CollectionBox), new PropertyMetadata(null));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static readonly DependencyProperty ItemsSourceProperty =
    DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<string>), typeof(CollectionBox));

        public ObservableCollection<string> ItemsSource
        {
            get { return (ObservableCollection<string>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static readonly DependencyProperty SpellCheckProperty =
            DependencyProperty.Register("SpellCheck", typeof(bool), typeof(CollectionBox), new PropertyMetadata(false));

        public bool SpellCheck
        {
            get { return (bool)GetValue(SpellCheckProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static readonly DependencyProperty BoxHeightProperty =
    DependencyProperty.Register("BoxHeightProperty", typeof(int), typeof(CollectionBox), new PropertyMetadata(200));

        public int BoxHeight
        {
            get { return (int)GetValue(BoxHeightProperty); }
            set { SetValue(BoxHeightProperty, value); }
        }

        private static readonly DependencyProperty RestrictionProperty =
DependencyProperty.Register("RestrictionProperty", typeof(InputRestriction), typeof(CollectionBox), new PropertyMetadata(InputRestriction.None));

        public InputRestriction Restriction
        {
            get { return (InputRestriction)GetValue(RestrictionProperty); }
            set { SetValue(RestrictionProperty, value); }
        }

        private string _item;

        public string Item
        {
            get { return _item; }
            set { _item = value; OnPropertyChanged("Item"); }
        }


        public RelayCommand AddItemToListCommand { get; set; }
        public RelayCommand RemoveItemFromListCommand { get; set; }

        public CollectionBox()
        {
            InitializeComponent();

            AddItemToListCommand = new RelayCommand(AddItemToList);
            RemoveItemFromListCommand = new RelayCommand(RemoveItemFromList);
        }

        public void AddItemToList(object input)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            Regex unusualCharacters = null;

            // Restrict input with regex
            if (Restriction == InputRestriction.None)
                unusualCharacters = null;
            else if (Restriction == InputRestriction.Author)
                unusualCharacters = new Regex("^[A-Za-z .'()-]+$");
            else if (Restriction == InputRestriction.Keyword)
                unusualCharacters = new Regex("^[A-Za-z0-9 .'()+/_?:\"\\&-]+$");

            // 1. Make sure the item is not blank
            if (Item == null || String.IsNullOrWhiteSpace(Item))
                return;

            // 2. Format the item
            Item = Item.Trim();
            Item = regex.Replace(Item, " ");

            // Check for unusual characters
            if (unusualCharacters != null & !unusualCharacters.IsMatch(Item))
            {
                new DialogService().OpenDialog(new DialogOkViewModel("This input contains unusual characters, please retype it manually. (Don't copy & paste!)", "Error", DialogType.Error));
                Item = null;
                return;
            }

            // 3. Make sure the Listbox doesn't already contain the item
            if (ItemsSource.Contains(Item))
                return;

            // 4. Add item to collection
            ItemsSource.Add(Item);

            // 5. Reset the item
            Item = null;
        }
        public void RemoveItemFromList(object input)
        {
            // 1. Remove sent item from the collection
            ItemsSource.Remove(input.ToString());
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
