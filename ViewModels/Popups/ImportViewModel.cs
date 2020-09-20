using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ViewModels.Base;
using Lib.ViewModels.Services.Dialogs;

namespace MainLib.ViewModels.Popups
{
    public class ImportViewModel : BaseViewModel
    {
        private int _progressPercent;
        private string destination;
        private Dictionary<string, string> _filesToCopy;
        private IDialogService _dialogService;

        public int ProgressPercent
        {
            get { return _progressPercent; }
            set { _progressPercent = value; OnPropertyChanged("ProgressPercent"); }
        }

        public ImportViewModel(Dictionary<string, string> filesToCopy, string destination, IDialogService dialogService)
        {
            // 1. Initialize starting fields
            Title = "Importing section...";
            ProgressPercent = 0;
            _filesToCopy = filesToCopy;
            this.destination = destination;
            this._dialogService = dialogService;

            Import();
        }

        public void Import(object input = null)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerAsync(_filesToCopy);
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressPercent = e.ProgressPercentage;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            int number_of_copied = 0;
            // For each record in dictionary copy the file with correct name
            foreach (KeyValuePair<string, string> entry in (Dictionary<string, string>)e.Argument)
            {
                File.Copy(destination + @"\Files\" + entry.Key, Environment.CurrentDirectory + @"\Files\" + entry.Value);
                number_of_copied++;
                if (number_of_copied * 100 / _filesToCopy.Count % 5 == 0)
                    worker.ReportProgress(number_of_copied * 100 / _filesToCopy.Count);
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Delete the temp query file
            File.Delete(Environment.CurrentDirectory + "\\" + "temp_query.txt");

            _dialogService.OpenDialog(new DialogOkViewModel("Imported successfully", "Result", DialogType.Success));

            // Close progress window
            Window.Close();
        }
    }
}
