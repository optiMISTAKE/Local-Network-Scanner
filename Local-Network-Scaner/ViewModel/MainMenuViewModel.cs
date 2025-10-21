using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Local_Network_Scaner.Services;
using Local_Network_Scaner.ViewModel.Base;

namespace Local_Network_Scaner.ViewModel
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly NavigationService _navigationService;
        private readonly ViewModelFactory _viewModelFactory;

        public MainMenuViewModel(NavigationService navigationService, ViewModelFactory viewModelFactory)
        {
            _navigationService = navigationService;
            _viewModelFactory = viewModelFactory;
        }

        public MainMenuViewModel() { }
    }

}
