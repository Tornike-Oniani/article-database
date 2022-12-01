using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.UIStructs;
using MainLib.ViewModels.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainLib.ViewModels.Main
{
    public class AbstractsViewModel : BaseViewModel
    {
        private List<AbstractItem> _abstracts;

        public List<AbstractItem> Abstracts
        {
            get { return _abstracts; }
            set { _abstracts = value; OnPropertyChanged("Abstracts"); }
        }

        public ICommand ChangeAbstractToEditModeCommand { get; set; }
        public ICommand ChangeAbstractToDisplayModeCommand { get; set; }

        public AbstractsViewModel()
        {
            ChangeAbstractToEditModeCommand = new RelayCommand(ChangeAbstractToEditMode);
            ChangeAbstractToDisplayModeCommand = new RelayCommand(ChangeAbstractToDisplayMode);
            PopulateAbstracts();
        }

        public void ChangeAbstractToEditMode(object input)
        {
            (input as AbstractItem).IsInEditMode = true;
        }
        public void ChangeAbstractToDisplayMode(object input)
        {
            (input as AbstractItem).IsInEditMode = false;
        }

        private async void PopulateAbstracts()
        {
            Shared.GetInstance().IsWorking(true, "Fetching abstracts");
            await Task.Run(() =>
            {
                Abstracts = new AbstractRepo().GetAllAbstracts().Select(a => new AbstractItem() { Id = a.Id, Title = a.ArticleTitle, Body = a.Body, Match = 55 }).ToList();
            });
            OnPropertyChanged("Abstracts");
            Shared.GetInstance().IsWorking(false);
        }
    }
}
