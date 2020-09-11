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
        private EditDialog _editDialog;
        private AddPersonalDialog _addPersonalDialog;
        private BookmarkManager _bookmarkManager;
        private ReferenceManager _referenceManager;
        private MassBookmarkManager _massBookmarkManager;

        /**
         * Commands:
         *  - Open search filter
         *  - Open edit dialog
         *  - Open dialog which edits/adds personal comment and sic
         *  - Open bookmark manager
         */
        public RelayCommand OpenSearchDialogCommand { get; set; }
        public RelayCommand OpenEditDialogCommand { get; set; }
        public RelayCommand OpenAddPersonalDialogCommand { get; set; }
        public RelayCommand OpenBookmarkManagerCommand { get; set; }
        public RelayCommand OpenReferencekManagerCommand { get; set; }
        public RelayCommand OpenMassBookmarkManagerCommand { get; set; }

        public static DataView CurrentView;

        // Construcotr
        public DataView()
        {
            InitializeComponent();

            // Initialize commands
            OpenSearchDialogCommand = new RelayCommand(OpenSearchDialog);
            OpenEditDialogCommand = new RelayCommand(OpenEditDialog, CanOpenBookmarkManager);
            OpenAddPersonalDialogCommand = new RelayCommand(OpenAddPersonalDialog, CanOpenBookmarkManager);
            OpenBookmarkManagerCommand = new RelayCommand(OpenBookmarkManager, CanOpenBookmarkManager);
            OpenReferencekManagerCommand = new RelayCommand(OpenReferenceManager, CanOpenBookmarkManager);
            OpenMassBookmarkManagerCommand = new RelayCommand(OpenMassBookmarkManager, CanOpenMassBookmarkManager);

            this.Loaded += (s, e) =>
            {
                // 1. If main window is too small make it bigger
                //if (MainWindow.CurrentMain.Width < 1200)
                //    MainWindow.CurrentMain.Width = 1200;

                /**
                 * 2. Set commands for follwoing menu items:
                 * - Edit
                 * - Add personal
                 * - Bookmark
                 */
                itemEdit.Command = OpenEditDialogCommand;
                itemAddCommentSIC.Command = OpenAddPersonalDialogCommand;
                itemBookmark.Command = OpenBookmarkManagerCommand;
                itemReference.Command = OpenReferencekManagerCommand;

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
                if (_bookmarkManager != null)
                    _bookmarkManager.Close();
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
        public void OpenEditDialog(object input = null)
        {
            //MessageBox.Show("Invoked");
            if (!EditDialog.Open)
            {
                _editDialog = new EditDialog();
                _editDialog.DataContext = new EditDialogViewModel(this.DataContext as DataViewViewModel);
                _editDialog.Owner = MainWindow.CurrentMain;
                _editDialog.ShowDialog();
            }
        }
        public void OpenAddPersonalDialog(object input)
        {
            _addPersonalDialog = new AddPersonalDialog();
            _addPersonalDialog.DataContext = new AddPersonalDialogViewModel(this.DataContext as DataViewViewModel);
            _addPersonalDialog.Owner = MainWindow.CurrentMain;
            _addPersonalDialog.ShowDialog();
        }
        public void OpenBookmarkManager(object input = null)
        {
            _bookmarkManager = new BookmarkManager();
            _bookmarkManager.Owner = MainWindow.CurrentMain;
            _bookmarkManager.DataContext = new BookmarkManagerViewModel(_bookmarkManager, ((DataViewViewModel)this.DataContext), (Article)input);
            _bookmarkManager.ShowDialog();
        }
        public bool CanOpenBookmarkManager(object input = null)
        {
            if (this.DataContext == null)
                return false;

            if (((DataViewViewModel)this.DataContext).SelectedArticle != null)
                return true;

            return false;
        }
        public void OpenReferenceManager(object input)
        {
            Article article = input as Article;
            _referenceManager = new ReferenceManager();
            _referenceManager.Owner = MainWindow.CurrentMain;
            _referenceManager.DataContext = new ReferenceManagerViewModel(_referenceManager, article);
            _referenceManager.ShowDialog();
        }
        public void OpenMassBookmarkManager(object input = null)
        {
            DataViewViewModel _vm = this.DataContext as DataViewViewModel;
            _massBookmarkManager = new MassBookmarkManager();
            _massBookmarkManager.DataContext = new MassBookmarkManagerViewModel(_massBookmarkManager, _vm.User, _vm.BMCheckedArticles);
            _massBookmarkManager.Owner = MainWindow.CurrentMain;
            _massBookmarkManager.ShowDialog();
        }
        public bool CanOpenMassBookmarkManager(object input = null)
        {
            DataViewViewModel _vm = this.DataContext as DataViewViewModel;
            if (_vm == null) return false;

            return _vm.BMCheckedArticles.Count > 0;
        }
    }
}
