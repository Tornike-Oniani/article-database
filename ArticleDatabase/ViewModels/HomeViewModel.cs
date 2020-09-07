using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using ArticleDatabase.Dialogs.DialogOk;
using ArticleDatabase.Dialogs.DialogService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleDatabase.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private User _user;

        public RelayCommand ValidateCommand { get; set; }

        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }


        public HomeViewModel(User user)
        {
            this.User = user;

            ValidateCommand = new RelayCommand(Validate);
        }

        public void Validate(object input = null)
        {
            // Path to Logs folder (root directory + Logs)
            string current_directory = System.IO.Path.Combine(Environment.CurrentDirectory, "Logs\\");

            // All pdf files in Files folder
            //string[] existingFiles = Directory.GetFiles(User.GetFilesFolder(), " *.pdf").Select(System.IO.Path.GetFileName).ToArray();
            string[] existingFiles = Directory.GetFiles(System.IO.Path.Combine(Environment.CurrentDirectory, "Files"), "*.pdf").Select(System.IO.Path.GetFileName).ToArray();
            // All file names in tblArticle database table
            string[] databaseFiles = (new GlobalRepo()).GetFileNames();

            // Mismatch between physical pdf files and database names
            if (databaseFiles != null)
            {
                string[] mismatch = databaseFiles.Except(existingFiles).ToArray();
                // If any mismatch was found create log file and write mismatched files in there
                if (mismatch.Length > 0)
                {
                    // Current date will be set as file name
                    DateTime current_date = DateTime.Today;
                    // Full file path including its name
                    string file_path = current_directory + current_date.ToLongDateString() + ".txt";

                    // Open or Create file
                    using (StreamWriter sw = File.AppendText(file_path))
                    {
                        // Start writing log startin with current hour as timestamp
                        sw.WriteLine(current_date.ToLongTimeString());
                        sw.WriteLine("Following files are missing:");
                        // Write each file name from mismatch folder into text file
                        foreach (string file in mismatch)
                            sw.WriteLine(file);
                        sw.WriteLine("");
                    }
                    DialogService.OpenDialog(new DialogOkViewModel("Some files are missing see Logs for more info...", "Result", DialogType.Warning), MainWindow.CurrentMain);
                }
                else
                {
                    DialogService.OpenDialog(new DialogOkViewModel("No missing files were found!", "Result", DialogType.Information), MainWindow.CurrentMain);
                }
            }
            else
            {
                DialogService.OpenDialog(new DialogOkViewModel("There are no records in database yet", "Result", DialogType.Information), MainWindow.CurrentMain);
            }
        }
    }
}
