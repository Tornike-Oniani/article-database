using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using System;
using System.Windows.Input;

namespace MainLib.ViewModels.Main
{
    public class AbstractsViewModel : BaseViewModel
    {
        private BaseViewModel _selectedTabContent;

        public BaseViewModel SelectedTabContent
        {
            get { return _selectedTabContent; }
            set { _selectedTabContent = value; OnPropertyChanged("SelectedTabContent"); }
        }

        public ICommand SwitchTabContentCommand { get; set; }

        // Constructor
        public AbstractsViewModel()
        {
            SwitchTabContentCommand = new RelayCommand(SwitchTabContent);
            SwitchTabContentCommand.Execute("Browse");
        }

        public void SwitchTabContent(object input)
        {
            string tab = input.ToString();
            if (String.IsNullOrEmpty(tab)) { return; }
            if (tab == "Browse")
            {
                SelectedTabContent = new AbstractsBrowseViewModel();
                return;
            }
            if (tab == "Entry")
            {
                SelectedTabContent = new AbstractsEntryViewModel();
                return;
            }
        }
    }
}
