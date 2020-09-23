using Lib.DataAccessLayer.Models;
using Lib.ViewModels.Base;
using Lib.ViewModels.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectionLib.ViewModels.Utils
{
    public class ExportViewModel : BaseViewModel
    {
        private int _progressPercent;
        private string _destination;
        private string _folderName;
        private string _filesPath;
        private IDialogService _dialogService;

        public int ProgressPercent
        {
            get { return _progressPercent; }
            set { _progressPercent = value; OnPropertyChanged("ProgressPercent"); }
        }

        public ExportViewModel(string filesPath, string destination, string folderName, IDialogService dialogService)
        {
            // 1. Initialize starting fields
            this.Title = "Importing section...";
            this.ProgressPercent = 0;
            this._filesPath = filesPath;
            this._destination = destination;
            this._folderName = folderName;
            this._dialogService = dialogService;

            Export();
        }

        public void Export(object input = null)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerAsync();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressPercent = e.ProgressPercentage;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string[] files = Directory.GetFiles(_filesPath);
            int number_of_copied = 0;
            int number_of_files = files.Count();
            // For each record in dictionary copy the file with correct name
            // Copy each file from root/Files folder
            foreach (string file in files)
            {
                File.Copy(file, System.IO.Path.Combine(_destination + _folderName + @"\Files", System.IO.Path.GetFileName(file)));
                number_of_copied++;
                if ((number_of_copied * 100 / number_of_files) % 5 == 0)
                    worker.ReportProgress(number_of_copied * 100 / number_of_files);
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _dialogService.OpenDialog(new DialogOkViewModel("Exported successfully", "Export", DialogType.Success));

            // Close progress window
            Window.Close();
        }
    }
}
