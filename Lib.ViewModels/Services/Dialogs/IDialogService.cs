using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ViewModels.Services.Dialogs;

namespace Lib.ViewModels.Services.Dialogs
{
    public interface IDialogService
    {
        bool OpenDialog(DialogViewModelBase vm);
    }
}
