using Local_Network_Scanner.Interfaces;
using Local_Network_Scanner.Services;
using Local_Network_Scanner.ViewModel;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Local_Network_Scanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindowViewModel _mainViewModel;
        protected override void OnStartup(StartupEventArgs e)
        {
            var navigationService = new NavigationService();
            var viewModelFactory = new ViewModel.Base.ViewModelFactory(navigationService);

            _mainViewModel = new MainWindowViewModel();

            navigationService.SetNavigator(vm => _mainViewModel.CurrentViewModel = vm);
            _mainViewModel.CurrentViewModel = viewModelFactory.CreateMainMenuVM();

            var mainWindow = new MainWindow()
            {
                DataContext = _mainViewModel
            };

            mainWindow.Closing += (s, args) => {
                if (_mainViewModel?.CurrentViewModel is ICleanup vm) vm.Cleanup();
            };

            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // This will work for ANY ViewModel that implements ICleanup
            if (_mainViewModel?.CurrentViewModel is ICleanup cleanupVm)
            {
                cleanupVm.Cleanup();
            }

            base.OnExit(e);
        }
    }

}
