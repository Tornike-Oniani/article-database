using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using ArticleDatabase.Dialogs.DialogOk;
using ArticleDatabase.Dialogs.DialogService;
using ArticleDatabase.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
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
using ViewModels.Services.Dialogs;

namespace ArticleDatabase.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        public Home()
        {
            InitializeComponent();
        }

        private void Import_Click(object sender, EventArgs e)
        {
            string destination = null;

            // 1. Using winforms dialog box select a folder
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    // Set selected folder path
                    destination = fbd.SelectedPath;
                }
            }

            // 2. If nothing was selected return
            if (destination == null)
                return;

            // 3. If the wrong folder was selected return with an error messagebox
            if ((!File.Exists(destination + "\\" + "NikasDB.sqlite3")) || (!Directory.Exists(destination + @"\Files")) || (!File.Exists(destination + "\\" + "user.sqlite3")))
            {
                new DialogService().OpenDialog(new DialogOkViewModel("Please select a correct folder.", "Error", DialogType.Error));
                return;
            }

            string root_directory = Environment.CurrentDirectory;

            // 4. If an old temp query file exists delete it
            if (File.Exists(Environment.CurrentDirectory + "\\" + "temp_query.txt"))
                File.Delete(Environment.CurrentDirectory + "\\" + "temp_query.txt");

            // 5. Set up queries for database import
            string base_query = System.IO.File.ReadAllText(root_directory + "\\" + "Query.txt");
            string file_path = root_directory + "\\" + "temp_query.txt";
            string attach_string =
                "ATTACH DATABASE \'" + root_directory + "\\" + "user.sqlite3\'" + "AS user;\n" +
                "ATTACH DATABASE \'" + destination + "\\" + "NikasDB.sqlite3\'" + "AS db2main;\n" +
                "ATTACH DATABASE \'" + destination + "\\" + "user.sqlite3\'" + "AS db2user;\n";
            string files_query = @"SELECT tf.File AS OldFile, tnf.File AS NewFile FROM temp_Files AS tf, temp_NewFiles AS tnf WHERE tf.Title = tnf.Title;";
            string duplicates_query = @"SELECT Title, File FROM temp_tblDuplicates;";

            // Open or Create file
            using (StreamWriter sw = File.AppendText(file_path))
            {
                sw.WriteLine(attach_string);
                sw.WriteLine(base_query);
            }

            // Path to temp file where full query is written (Including attaches)
            string full_query = System.IO.File.ReadAllText(root_directory + "\\" + "temp_query.txt");

            // 6. Import recrods to database, return list of files to copy and duplicates to log
            List<ExistCheck> duplicates;
            List<CompFile> files = (new GlobalRepo()).ImportSection(full_query, files_query, duplicates_query, out duplicates);

            // 7. Create dictionary of files to copy
            Dictionary<string, string> files_to_copy = new Dictionary<string, string>();
            foreach (CompFile file in files)
            {
                files_to_copy.Add(file.OldFile + ".pdf", file.NewFile + ".pdf");
            }

            // 8. Log duplicate files
            DirectoryInfo current_directory_logs = new DirectoryInfo("." + @"\Logs\");

            // Current date will be set as file name
            DateTime current_date = DateTime.Today;
            // Current time
            DateTime current_time = DateTime.Now;
            // Full file path including its name
            string merge_log_path = current_directory_logs.FullName + "Merge --- " + current_date.ToLongDateString() + ".txt";

            // Open or Create file
            using (StreamWriter sw = File.AppendText(merge_log_path))
            {
                // Start writing log startin with current hour as timestamp
                sw.WriteLine(current_time.ToLongTimeString());
                sw.WriteLine("Following titles are duplicate:");
                // Write each file name from mismatch folder into text file
                foreach (ExistCheck duplicate in duplicates)
                    sw.WriteLine(duplicate.Title + " ||| " + duplicate.File);
                sw.WriteLine("");
            }

            // 9. Create progress bar and copy physical .pdf files
            Progress _progress = new Progress();
            _progress.Owner = MainWindow.CurrentMain;
            ImportViewModel progress_view_model = new ImportViewModel(files_to_copy, destination, _progress);
            _progress.DataContext = progress_view_model;
            _progress.Show();
            progress_view_model.Import();
        }

        private void ChangeAccount_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Restart();
            Application.Current.Shutdown();
        }
    }
}
