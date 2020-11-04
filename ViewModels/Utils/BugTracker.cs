using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Utils
{
    public class BugTracker
    {
        public string directoryPath { get; set; }
        public string filePath { get; set; }

        public BugTracker()
        {
            directoryPath = Path.Combine(Environment.CurrentDirectory, "Logs");
            filePath = Path.Combine(directoryPath, "Bugs.txt");

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        public void Track(string name, string action, string message)
        {
            using(StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(DateTime.Today.ToString());
                sw.WriteLine($"{name} -> {action}");
                sw.WriteLine(message);
                sw.WriteLine();
            }
        }
    }
}
