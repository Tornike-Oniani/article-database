using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.ViewModels;
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
        public static DataView CurrentView;

        // Construcotr
        public DataView()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                // 1. Focus keyobard on this view (We need this so that Usercontrol.Inputbindings work)
                Keyboard.Focus(this);

                // 2. Get the view to static
                CurrentView = this;
            };

            // Close all windows that were left open when view gets changed
            this.Unloaded += (s, e) =>
            {
                foreach(Window win in MainWindow.CurrentMain.OwnedWindows)
                {
                    win.Close();
                }
            };
        }
    }
}
