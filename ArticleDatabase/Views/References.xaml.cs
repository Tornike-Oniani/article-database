using ArticleDatabase.ViewModels;
using ArticleDatabase.ViewModels.Pages;
using ArticleDatabase.Views.Pages;
using System;
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

namespace ArticleDatabase.Views
{
    /// <summary>
    /// Interaction logic for References.xaml
    /// </summary>
    public partial class References : UserControl
    {
        public References()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                ReferenceListViewModel view_model = new ReferenceListViewModel(((ReferencesViewModel)this.DataContext).User);
                Page _mainPage = new ReferenceList(view_model);
                _mainPage.DataContext = view_model;
                _mainFrame.Navigate(_mainPage);
            };
        }
    }
}
