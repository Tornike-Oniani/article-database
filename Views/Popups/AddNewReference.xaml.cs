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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainLib.Views.Popups
{
    /// <summary>
    /// Interaction logic for AddNewReference.xaml
    /// </summary>
    public partial class AddNewReference : UserControl
    {
        public AddNewReference()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                Keyboard.Focus(this);
                txbReferenceName.Focus();
            };
        }
    }
}
