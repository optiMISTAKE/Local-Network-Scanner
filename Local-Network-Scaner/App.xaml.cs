using System.Configuration;
using System.Data;
using System.Windows;
using Local_Network_Scaner.Services;
using Local_Network_Scaner.ViewModel;

namespace Local_Network_Scaner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var navigationService = new NavigationService();
            var viewModelFactory = new ViewModel.Base.ViewModelFactory(navigationService);

            var mainViewModel = new MainWindowViewModel();

            navigationService.SetNavigator(vm => mainViewModel.CurrentViewModel = vm);
            mainViewModel.CurrentViewModel = viewModelFactory.CreateMainMenuVM();

            var mainWindow = new MainWindow()
            {
                DataContext = mainViewModel
            };

            mainWindow.Show();
        }
    }

}
