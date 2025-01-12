﻿using Lib.DataAccessLayer.Models;
using Lib.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ViewModels.UIStructs;

namespace MainLib.Views.UserControls
{
    /// <summary>
    /// Interaction logic for FilterBox.xaml
    /// </summary>
    public partial class FilterBox : UserControl, INotifyPropertyChanged
    {
        // Registered properties
        private static readonly DependencyProperty ItemsSourceProperty =
    DependencyProperty.Register("ItemsSource", typeof(CollectionViewSource), typeof(FilterBox), new PropertyMetadata(null));

        public CollectionViewSource ItemsSource
        {
            get { return (CollectionViewSource)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static readonly DependencyProperty WatermarkProperty =
    DependencyProperty.Register("Watermark", typeof(string), typeof(FilterBox), new PropertyMetadata("Search"));

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        // Private members
        private string _filterText;

        // Public properties
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                ItemsSource.View.Refresh();
                OnPropertyChanged("FilterText");
            }
        }

        public ICommand ClearCommand { get; set; }

        // Constructor
        public FilterBox()
        {
            InitializeComponent();

            this.ClearCommand = new RelayCommand(Clear);

            // Hook filter event on load
            this.Loaded += (s, e) =>
            {
                if (ItemsSource != null)
                {
                    ItemsSource.Filter += ItemsSource_Filter;
                    searchBox.Focus();
                }
                
            };
        }

        public void Clear(object input = null)
        {
            FilterText = String.Empty;
        }

        private void ItemsSource_Filter(object sender, FilterEventArgs e)
        {
            // Edge case: filter text is empty
            if (string.IsNullOrWhiteSpace(FilterText))
            {
                e.Accepted = true;
                return;
            }

            e.Accepted = false;

            // BookmarkBox
            if (e.Item is BookmarkBox)
            {
                BookmarkBox current = e.Item as BookmarkBox;
                if (current.Bookmark.Name.ToUpper().Contains(FilterText.ToUpper()))
                {
                    e.Accepted = true;
                }
            }
            // ReferenceBox
            else if (e.Item is ReferenceBox)
            {
                ReferenceBox current = e.Item as ReferenceBox;
                if (current.Reference.Name.ToUpper().Contains(FilterText.ToUpper()))
                {
                    e.Accepted = true;
                }
            }
            // Bookmark
            else if (e.Item is Bookmark)
            {
                Bookmark current = e.Item as Bookmark;
                if (current.Name.ToUpper().Contains(FilterText.ToUpper()))
                {
                    e.Accepted = true;
                }
            }
            // Reference
            else if (e.Item is Reference)
            {
                Reference current = e.Item as Reference;
                if (current.Name.ToUpper().Contains(FilterText.ToUpper()))
                {
                    e.Accepted = true;
                }
            }
            else
            {
                throw new ArgumentException("Invalid argument type!");
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
