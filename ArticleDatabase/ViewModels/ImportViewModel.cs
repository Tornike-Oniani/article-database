using ArticleDatabase.Commands;
using ArticleDatabase.Dialogs.DialogOk;
using ArticleDatabase.Dialogs.DialogService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleDatabase.ViewModels
{
    public class ImportViewModel : BaseViewModel
    {
        private int _progressPercent;
        private string destination;
        private Dictionary<string, string> _filesToCopy;

        private ArticleDatabase.Views.Progress _progress;

        public string Title { get; set; }
        public int ProgressPercent
        {
            get { return _progressPercent; }
            set { _progressPercent = value; OnPropertyChanged("ProgressPercent"); }
        }

        public ImportViewModel(Dictionary<string, string> filesToCopy, string destination, ArticleDatabase.Views.Progress progress)
        {
            // 1. Initialize starting fields
            Title = "Importing section...";
            ProgressPercent = 0;
            _filesToCopy = filesToCopy;
            this.destination = destination;
            _progress = progress;
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

            DialogService.OpenDialog(new DialogOkViewModel("Imported successfully", "Result", DialogType.Success), MainWindow.CurrentMain);

            // Close progress window
            _progress.Close();
        }

    }
}
