using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lib.ViewModels.Services.Browser;

namespace Lib.Views.Services.Browser
{
    public class BrowserService : IBrowserService
    {
        public string OpenFileDialog(string defaultEx, string filter)
        {
            // 1. Create new dialog window
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // 2. Set filter for file extension and default file extension 
            dlg.DefaultExt = defaultEx;
            dlg.Filter = filter;


            // 3. Display the dialog window
            Nullable<bool> result = dlg.ShowDialog();

            // 4. Return either the selected file or null
            if (result == true)
                return dlg.FileName;
            else
                return null;
        }

        public string OpenFolderDialog(string savedPath)
        {
            string destination = null;

            // 1. Open forms folder dialog box
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fbd.RootFolder = Environment.SpecialFolder.Desktop;
                if (!String.IsNullOrEmpty(savedPath))
                    fbd.SelectedPath = savedPath;
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    // Set the destination to dialog box's result
                    destination = fbd.SelectedPath;
                }
            }

            return destination;
        }

        public string OpenSaveFileDialog(string filter, string defaultEx, string fileName, string savedPath = null)
        {
            string result = null;

            SaveFileDialog dlg = new SaveFileDialog()
            {
                Filter = filter,
                DefaultExt = defaultEx,
                FileName = fileName
            };

            if (!String.IsNullOrEmpty(savedPath))
            {
                dlg.InitialDirectory = savedPath;
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                result = dlg.FileName;
            }

            return result;
        }
    }
}
