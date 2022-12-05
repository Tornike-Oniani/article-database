using System.ComponentModel;

namespace MainLib.ViewModels.UIStructs
{
    public class AbstractBrowseItem : INotifyPropertyChanged
    {
        private bool _isInEditMode;
        private string _title;

        public int Id { get; set; }
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged("Title"); }
        }
        public string Body { get; set; }
        public int Match { get; set; }
        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set { _isInEditMode = value; OnPropertyChanged("IsInEditMode"); }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
