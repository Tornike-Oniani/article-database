﻿using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleDatabase.ViewModels
{
    public class AddPersonalDialogViewModel : BaseViewModel
    {
        public DataViewViewModel Parent { get; set; }

        public string PersonalComment { get; set; }
        public int SIC { get; set; }

        public RelayCommand AddPersonalCommand { get; set; }

        public AddPersonalDialogViewModel(DataViewViewModel parent)
        {
            // 1. Set attributes from parent
            Parent = parent;
            PersonalComment = parent.SelectedArticle.PersonalComment;
            SIC = parent.SelectedArticle.SIC;

            // 2. Initialize commands
            AddPersonalCommand = new RelayCommand(AddPersonal);

        }

        public void AddPersonal(object input = null)
        {
            if (PersonalComment != Parent.SelectedArticle.PersonalComment || SIC != Parent.SelectedArticle.SIC)
            {
                // 1. Send new comment and SIC values to parent
                Parent.SelectedArticle.PersonalComment = PersonalComment;
                Parent.SelectedArticle.SIC = SIC;

                // 2. Update record in database
                SqliteDataAccess.UpdatePersonal(Parent.SelectedArticle, Parent.User);
            }
        }
    }
}
