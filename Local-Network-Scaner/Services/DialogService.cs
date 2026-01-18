using Local_Network_Scanner.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Local_Network_Scanner.Services
{
    public class DialogService : IDialogService
    {
        public bool ShowConfirmation(string title, string message)
        {
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.OK;
        }
    }
}
