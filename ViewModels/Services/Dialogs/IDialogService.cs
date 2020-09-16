using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Services.Dialogs;

namespace ViewModels.Services
{
    public interface IDialogService
    {
        bool OpenDialog(DialogViewModelBase vm);
    }
}
