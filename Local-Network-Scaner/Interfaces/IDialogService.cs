using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Local_Network_Scanner.Interfaces
{
    public interface IDialogService
    {
        bool ShowConfirmation(string title, string message);
    }
}
