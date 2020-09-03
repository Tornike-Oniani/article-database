using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.ViewModels.Dialogs;
using ArticleDatabase.ViewModels.Pages;
using ArticleDatabase.Views.Dialogs;
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

namespace ArticleDatabase.Views.Pages
{
    /// <summary>
    /// Interaction logic for ReferenceList.xaml
    /// </summary>
    public partial class ReferenceList : Page
    {
        /**
         * Commands:
         *  - Edit bookmark
         */
        public RelayCommand EditReferenceCommand { get; set; }

        public ReferenceList(ReferenceListViewModel vm)
        {
            InitializeComponent();
            // 1. Initialize commands
            vm.OpenReferenceCommand = new RelayCommand(OpenReference);
            EditReferenceCommand = new RelayCommand(EditReference);
        }

        /**
         * Command actions:
         *  - Open bookmark
         *  [The actual command is in viewmodel]
         */
        public void OpenReference(object input)
        {
            Page _referenceView = new ReferenceView();
            _referenceView.DataContext = new ReferenceViewViewModel(input as Reference, ((ReferenceListViewModel)this.DataContext).User);
            NavigationService.Navigate(_referenceView);
        }
        public void EditReference(object input)
        {
            ReferenceEditor reference_editor = new ReferenceEditor();
            reference_editor.DataContext = new ReferenceEditorViewModel(input as Reference, (ReferenceListViewModel)this.DataContext, reference_editor);
            reference_editor.Owner = MainWindow.CurrentMain;
            reference_editor.ShowDialog();

        }
    }
}
