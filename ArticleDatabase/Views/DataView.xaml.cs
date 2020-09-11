using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.ViewModels;
using ArticleDatabase.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArticleDatabase.Views
{
    /// <summary>
    /// Interaction logic for DataView.xaml
    /// </summary>
    public partial class DataView : UserControl
    {
        /**
         * Private members
         */
        private SearchDialog _searchDialog;

        /**
         * Commands:
         *  - Open search filter
         *  - Open edit dialog
         *  - Open dialog which edits/adds personal comment and sic
         *  - Open bookmark manager
         */
        public RelayCommand OpenSearchDialogCommand { get; set; }       
        

        public static DataView CurrentView;

        // Construcotr
        public DataView()
        {
            InitializeComponent();

            // Initialize commands
            OpenSearchDialogCommand = new RelayCommand(OpenSearchDialog);
            
            
            
            

            this.Loaded += (s, e) =>
            {
                // 3. Open search dialog
                OpenSearchDialogCommand.Execute(null);

                // 4. Focus keyobard on this view (We need this so that Usercontrol.Inputbindings work)
                Keyboard.Focus(this);

                // 5. Get the view to static
                CurrentView = this;
            };

            // Close search dialog and bookmark manager when view gets changed
            this.Unloaded += (s, e) =>
            {
                _searchDialog.Close();
            };
        }

        /**
         * Command actions
         */
        public void OpenSearchDialog(object input = null)
        {
            if (!SearchDialog.Open)
            {
                _searchDialog = new SearchDialog();
                _searchDialog.DataContext = new SearchDialogViewModel(this.DataContext as DataViewViewModel);
                _searchDialog.Owner = MainWindow.CurrentMain;
                _searchDialog.Show();
            }
        }
        public bool CanOpenBookmarkManager(object input = null)
        {
            if (this.DataContext == null)
                return false;

            if (((DataViewViewModel)this.DataContext).SelectedArticle != null)
                return true;

            return false;
        }
    }
}
