﻿using Lib.DataAccessLayer.Info;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Lib.DataAccessLayer.Models
{
    public class Article : BaseModel
    {
        private long? _id;
        private string _title;
        private string _authors;
        private string _keywords;
        private int? _year;
        private string _fileName;
        private string _personalComment;
        private int _sic;
        private int _abstractOnly;
        private bool _checked;
        private string _abstractBody;
        private bool _abstractShown;
        private bool _abstractExpanded;

        public long? ID
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged("ID"); }
        }
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged("Title"); }
        }
        public string Authors
        {
            get { return _authors; }
            set { _authors = value; OnPropertyChanged("Authors"); }
        }
        public string Keywords
        {
            get { return _keywords; }
            set { _keywords = value; OnPropertyChanged("Keywords"); }
        }
        public int? Year
        {
            get { return _year; }
            set { _year = value; OnPropertyChanged("Year"); }
        }
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; OnPropertyChanged("FileName"); }
        }
        public string PersonalComment
        {
            get { return _personalComment; }
            set { _personalComment = value; OnPropertyChanged("PersonalComment"); }
        }
        public int SIC
        {
            get { return _sic; }
            set { _sic = value; OnPropertyChanged("SIC"); }
        }
        public int AbstractOnly
        {
            get { return _abstractOnly; }
            set { _abstractOnly = value; OnPropertyChanged("AbstractOnly"); }
        }
        public string AbstractBody
        {
            get { return _abstractBody; }
            set { _abstractBody = value; OnPropertyChanged("AbstractBody"); }
        }
        public bool AbstractExpanded
        {
            get { return _abstractExpanded; }
            set { _abstractExpanded = value; OnPropertyChanged("AbstractExpanded"); }
        }

        public bool AbstractShown
        {
            get { return _abstractShown; }
            set { _abstractShown = value; OnPropertyChanged("AbstractShown"); }
        }
        public bool Checked
        {
            get { return _checked; }
            set { _checked = value; OnPropertyChanged("Checked"); }
        }
        public bool BMChecked { get; set; }

        public ObservableCollection<string> AuthorsCollection { get; set; }
        public ObservableCollection<string> KeywordsCollection { get; set; }

        public Article()
        {
            AuthorsCollection = new ObservableCollection<string>();
            KeywordsCollection = new ObservableCollection<string>();
        }

        public Article(ArticleInfo info)
        {
            this.ID = info.ID;
            this.Title = info.Title;
            this.Authors = info.Authors;
            this.Keywords = info.Keywords;
            this.Year = info.Year;
            this.FileName = info.FileName;
            this.PersonalComment = info.PersonalComment;
            this.SIC = info.SIC;
            this.AbstractOnly = info.AbstractOnly;

            AuthorsCollection = new ObservableCollection<string>();
            KeywordsCollection = new ObservableCollection<string>();

            CollectionFromItemsString("Authors");
            CollectionFromItemsString("Keywords");
        }

        public void Clear()
        {
            ID = null;
            Title = null;
            Authors = null;
            Keywords = null;
            Year = null;
            FileName = null;
            PersonalComment = null;
            SIC = 0;
            AbstractOnly = 0;
            AuthorsCollection.Clear();
            KeywordsCollection.Clear();
        }
        public void CopyByValue(Article article, bool collections, bool itemsString)
        {
            // 1. If marked build items string on 'from' article (this is used to update authors and keyword on data grid when copying back modified article)
            if (itemsString)
            {
                article.ItemsStringFromCollection("Authors");
                article.ItemsStringFromCollection("Keywords");
            }

            // 2. Copy necessary properties
            ID = article.ID;
            Title = article.Title;
            Authors = article.Authors;
            Keywords = article.Keywords;
            Year = article.Year;
            FileName = article.FileName;
            PersonalComment = article.PersonalComment;
            SIC = article.SIC;
            this.AbstractOnly = article.AbstractOnly;

            // 3. If marked as true build collections from items strings
            if (collections)
            {
                CollectionFromItemsString("Authors");
                CollectionFromItemsString("Keywords");
            }
        }

        private void CollectionFromItemsString(string type)
        {
            if (type == "Authors")
            {

                // 1. Clear the collections
                AuthorsCollection.Clear();

                // 2. Check if items string is blank
                if (String.IsNullOrWhiteSpace(Authors))
                    return;

                // 3. Add each item from item string into collection
                foreach (string author in Authors.Split(new string[] { ", " }, StringSplitOptions.None))
                {
                    AuthorsCollection.Add(author);
                }
            }
            else if (type == "Keywords")
            {
                KeywordsCollection.Clear();

                if (String.IsNullOrWhiteSpace(Keywords))
                    return;

                foreach (string keyword in Keywords.Split(new string[] { ", " }, StringSplitOptions.None))
                {
                    KeywordsCollection.Add(keyword);
                }
            }
        }
        private void ItemsStringFromCollection(string type)
        {
            if (type == "Authors")
            {

                // 1. Clear the items string
                Authors = "";

                // 2. Check if collection is empty
                if (AuthorsCollection.Count <= 0)
                    return;

                // 3. Build string from each item in collection
                foreach (string author in AuthorsCollection)
                {
                    Authors += author;

                    // If current author is not the last one add separator ', '
                    if (author != AuthorsCollection.Last())
                        Authors += ", ";
                }
            }
            else if (type == "Keywords")
            {
                Keywords = "";

                if (KeywordsCollection.Count <= 0)
                    return;

                foreach (string keyword in KeywordsCollection)
                {
                    Keywords += keyword;

                    if (keyword != KeywordsCollection.Last())
                        Keywords += ", ";
                }
            }
        }

    }
}
