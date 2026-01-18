using Local_Network_Scanner.Model;
using Local_Network_Scanner.Services;
using Local_Network_Scanner.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Local_Network_Scanner.ViewModel
{
    public class MainMenuViewModel: ViewModelBase
    {
        // PRIVATE FIELDS
        private readonly NavigationService _navigationService;
        private readonly ViewModelFactory _viewModelFactory;
        private readonly LanguageService _languageService = new LanguageService();
        private bool _isDyslexicModeOn = false;

        // Font size defaults for normal mode
        private const double NORMAL_BASE_SIZE = 14.0;
        private const double NORMAL_SMALL_SIZE = 12.0;
        private const double NORMAL_MEDIUM_SIZE = 16.0;
        private const double NORMAL_LARGE_SIZE = 20.0;
        private const double NORMAL_EXTRA_LARGE_SIZE = 28.0;
        private const double NORMAL_HUGE_SIZE = 42.0;

        // Font size adjustments for dyslexic mode (scaled down ~15%)
        private const double DYSLEXIC_BASE_SIZE = 10.0;
        private const double DYSLEXIC_SMALL_SIZE = 10.0;
        private const double DYSLEXIC_MEDIUM_SIZE = 14.0;
        private const double DYSLEXIC_LARGE_SIZE = 17.0;
        private const double DYSLEXIC_EXTRA_LARGE_SIZE = 24.0;
        private const double DYSLEXIC_HUGE_SIZE = 36.0;

        // PUBLIC PROPERTIES, AVAILABLE FOR DATA BINDING

        // COMMANDS
        public ICommand NavigateToMainScanCommand { get; }
        public ICommand NavigateToBluetoothScanningCommand { get; }
        public ICommand ToggleDyslexicFontCommand { get; }
        public ICommand ChangeLanguageCommand { get; }

        // CONSTRUCTORS
        public MainMenuViewModel(NavigationService navigationService, ViewModelFactory viewModelFactory)
        {
            _navigationService = navigationService;
            _viewModelFactory = viewModelFactory;

            NavigateToMainScanCommand = new RelayCommand(_ =>
            _navigationService.NavigateTo(_viewModelFactory.CreateMainScanVM()), _ => true);

            NavigateToBluetoothScanningCommand = new RelayCommand(_ =>
            _navigationService.NavigateTo(_viewModelFactory.CreateBluetoothScanVM()), _ => true);

            ToggleDyslexicFontCommand = new RelayCommand(ExecuteToggleFont);

            // This command takes a string parameter (e.g., "en", "pl", "uk")
            ChangeLanguageCommand = new RelayCommand(param =>
            {
                if (param is string langCode)
                    _languageService.ChangeLanguage(langCode);
            });
        }

        private void ExecuteToggleFont()
        {
            _isDyslexicModeOn = !_isDyslexicModeOn;

            var appResources = Application.Current.Resources;

            if (_isDyslexicModeOn)
            {
                // we take the OpecDyslexic font and set it as main
                if (appResources["DyslexicFont"] is FontFamily dyslexicFont)
                {
                    appResources["MainFont"] = dyslexicFont;
                }

                // Scale down font sizes to compensate for OpenDyslexic's larger appearance
                appResources["BaseFontSize"] = DYSLEXIC_BASE_SIZE;
                appResources["SmallFontSize"] = DYSLEXIC_SMALL_SIZE;
                appResources["MediumFontSize"] = DYSLEXIC_MEDIUM_SIZE;
                appResources["LargeFontSize"] = DYSLEXIC_LARGE_SIZE;
                appResources["ExtraLargeFontSize"] = DYSLEXIC_EXTRA_LARGE_SIZE;
                appResources["HugeFontSize"] = DYSLEXIC_HUGE_SIZE;
            }
            else
            {
                // restore to default font
                if (appResources["DefaultFont"] is FontFamily defaultFont)
                {
                    appResources["MainFont"] = defaultFont;
                }

                // Restore original font sizes
                appResources["BaseFontSize"] = NORMAL_BASE_SIZE;
                appResources["SmallFontSize"] = NORMAL_SMALL_SIZE;
                appResources["MediumFontSize"] = NORMAL_MEDIUM_SIZE;
                appResources["LargeFontSize"] = NORMAL_LARGE_SIZE;
                appResources["ExtraLargeFontSize"] = NORMAL_EXTRA_LARGE_SIZE;
                appResources["HugeFontSize"] = NORMAL_HUGE_SIZE;
            }
        }
    }
}
