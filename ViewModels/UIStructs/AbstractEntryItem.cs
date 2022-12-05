using System.ComponentModel;

namespace MainLib.ViewModels.UIStructs
{
    public class AbstractEntryItem : INotifyPropertyChanged
    {
        private string _title;
        private string _body;
        private string _fileName;

        public int Id { get; set; }
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged("Title"); }
        }
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; OnPropertyChanged("FileName"); }
        }
        public string Body
        {
            get { return _body; }
            set { _body = FormatText(value); OnPropertyChanged("Body"); }
        }

        // Removes carriage returns (CL RF) and unusual characters
        private string FormatText(string text)
        {
            if (text == null)
            {
                return "";
            }

            return text.Replace("\n", " ").Replace("\r", " ").Replace("–", "-").Replace("“", "\"").Replace("”", "\"").Replace("„", "\"").Replace("‟", "\"");
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
