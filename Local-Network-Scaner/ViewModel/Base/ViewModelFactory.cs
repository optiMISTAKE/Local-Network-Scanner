using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Local_Network_Scaner.Services;

namespace Local_Network_Scaner.ViewModel.Base
{
    public class ViewModelFactory
    {
        private readonly NavigationService _navigationService;

        public ViewModelFactory(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public MainMenuViewModel CreateMainMenuVM()
        {
            return new MainMenuViewModel(_navigationService, this);
        }
    }
}
