﻿using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Lib.Views.Services.Windows
{
    /// <summary>
    /// Interaction logic for GenericWindow.xaml
    /// </summary>
    public partial class GenericWindow : WindowBase
    {
        public GenericWindow()
        {
            InitializeComponent();


            this.Loaded += (s, e) =>
            {
                this.MinHeight = this.ActualHeight;
                Keyboard.Focus(this);
            };
        }
    }
}
