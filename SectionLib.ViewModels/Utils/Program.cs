using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectionLib.ViewModels.Utils
{
    public static class Program
    {
        public static string SelectedSection { get; set; }
        public static string GetSectionPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "Sections", SelectedSection);
        }
        public static string GetSectionFilesPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "Sections", SelectedSection, "Files\\");
        }

        public static string GetSectionLogsPath()
        {
            string sect = GetSectionFilesPath();
            return Path.Combine(Environment.CurrentDirectory, "Sections", SelectedSection, "Logs\\");
        }
    }
}
