using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.Services.Browser
{
    public interface IBrowserService
    {
        string OpenFileDialog(string defaultEx, string filter);
        string OpenFolderDialog();
    }
}
