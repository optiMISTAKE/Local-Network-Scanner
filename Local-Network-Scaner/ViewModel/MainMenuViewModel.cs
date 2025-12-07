using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Local_Network_Scaner.Services;
using Local_Network_Scaner.ViewModel.Base;
using Local_Network_Scaner.Model;
using System.Windows.Input;
using System.Net;
using System.Xml.Linq;
using System.Windows;

namespace Local_Network_Scaner.ViewModel
{
    public class MainMenuViewModel : ViewModelBase
    {
        // PRIVATE FIELDS

        private readonly NavigationService _navigationService;
        private readonly ViewModelFactory _viewModelFactory;
        private readonly ScanService _scanService = new ScanService();
        private readonly NetworkInterfaceService _networkInterfaceService = new NetworkInterfaceService();
        private readonly OuiDatabaseService _ouiDb = new OuiDatabaseService();

        // PUBLIC PROPERTIES, AVAILABLE FOR DATA BINDING

        public ObservableCollection<DeviceInfo> Devices { get; set; } = new ObservableCollection<DeviceInfo>();
        public ObservableCollection<LocalNetworksInfo> AvailableNetworkInterfaces { get; } = new ObservableCollection<LocalNetworksInfo>();
        public LocalNetworksInfo CurrentlySetNetInterface { get; set; }

        // COMMANDS

        public ICommand ScanCommand => new RelayCommand(async () =>
        {
            if (CurrentlySetNetInterface == null) return;

            string baseIp = CurrentlySetNetInterface.NetworkAddress;

            var parts = baseIp.Split('.');
            if (parts.Length != 4) ; // TO-DO: handle error. Do throw exception or smth

            string subnetBase = $"{parts[0]}.{parts[1]}.{parts[2]}";

            Devices.Clear();
            var results = await _scanService.ScanSubnetAsync(subnetBase);
            foreach (var d in results)
                Devices.Add(d);
        });
        public ICommand LoadNetworkInterfacesCommand => new RelayCommand(LoadNetworkInterfaces);

        public ICommand TestOUI => new RelayCommand(TestOUIDatabase);

        // CONSTRUCTORS

        public MainMenuViewModel(NavigationService navigationService, ViewModelFactory viewModelFactory)
        {
            _navigationService = navigationService;
            _viewModelFactory = viewModelFactory;
            _ouiDb.LoadDatabaseCSV("Resources/oui.csv");

            LoadNetworkInterfaces();
        }

        public MainMenuViewModel() { }

        private void LoadNetworkInterfaces()
        {
            // Logic moved from the Command lambda to here
            AvailableNetworkInterfaces.Clear();
            var results = _networkInterfaceService.GetActiveNetworkInterfaces();

            foreach (var ni in results)
            {
                AvailableNetworkInterfaces.Add(ni);
            }

            // Automatically select the first network found so the box isn't blank
            if (AvailableNetworkInterfaces.Count > 0)
            {
                CurrentlySetNetInterface = AvailableNetworkInterfaces[0];
                OnPropertyChanged(nameof(CurrentlySetNetInterface));
            }
        }

        // DELETE LATER
        private void TestOUIDatabase()
        {
            string testMac = "F0-20-FF-22-D9-A9";
            OuiRecord record = _ouiDb.GetVendor(testMac);
            if (record != null)
            {
                MessageBox.Show($"Producentem urzadzenia o MAC {testMac} jest {record.Vendor}\nAdres: {record.Address}");
            }
            else
            {
                MessageBox.Show("OUI record not found in the database. The given MAC address is most probably random or private");
            }
        }
    }

}
