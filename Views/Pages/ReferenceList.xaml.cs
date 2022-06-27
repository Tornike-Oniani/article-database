using Lib.DataAccessLayer.Models;
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
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Pages;
using Lib.Views.Services.Dialogs;
using Lib.Views.Services.Browser;

namespace MainLib.Views.Pages
{
    /// <summary>
    /// Interaction logic for ReferenceList.xaml
    /// </summary>
    public partial class ReferenceList : Page
    {
        public ReferenceList(ReferenceListViewModel vm)
        {
            InitializeComponent();

            // 1. Initialize commands
            vm.OpenReferenceCommand = new RelayCommand(OpenReference);
        }

        /**
         * Command actions:
         *  - Open bookmark
         *  [The actual command is in viewmodel]
         */
        public void OpenReference(object input)
        {
            ReferenceListViewModel vm = (ReferenceListViewModel)this.DataContext;
            Reference sent_reference = input as Reference;
            Page _referenceView = new ReferenceView();

            _referenceView.DataContext = new ReferenceViewViewModel(sent_reference);
            NavigationService.Navigate(_referenceView);
        }
    }
}
