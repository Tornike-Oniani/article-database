﻿using ArticleDatabase.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleDatabase.ViewModels
{
    public class SearchDialogViewModel : BaseWindow
    {
        public DataViewViewModel Parent { get; set; }

        public SearchDialogViewModel(DataViewViewModel parent)
        {
            this.Title = "Set filter";

            this.Parent = parent;
        }
    }
}
