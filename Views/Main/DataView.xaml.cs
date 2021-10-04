using MainLib.ViewModels.Main;
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

namespace MainLib.Views.Main
{
    /// <summary>
    /// Interaction logic for DataView.xaml
    /// </summary>
    public partial class DataView : UserControl
    {
        public static DataView CurrentView;

        public DataView()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                // 1. Focus keyobard on this view (We need this so that Usercontrol.Inputbindings work)
                Keyboard.Focus(this);

                // 2. Get the view to static
                CurrentView = this;

                // 3. Open search dialog
                DataViewViewModel vm = (DataViewViewModel)this.DataContext;
                vm.OpenSearchDialogCommand.Execute(null);
            };
        }
    }
}
