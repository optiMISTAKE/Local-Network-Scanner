using Local_Network_Scanner.Interfaces;
using Local_Network_Scanner.Model;
using Local_Network_Scanner.Services;
using Local_Network_Scanner.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Local_Network_Scanner.ViewModel
{
    public class MainScanViewModel : ViewModelBase, ICleanup
    {
        // PRIVATE FIELDS

        private readonly NavigationService _navigationService;
        private readonly ViewModelFactory _viewModelFactory;
        private readonly ScanService _scanService = new ScanService();
        private readonly NetworkInterfaceService _networkInterfaceService = new NetworkInterfaceService();
        private int _scannedDevicesCount;
        private int _totalHostsToScan;
        private ScanSpeedPreset _selectedScanSpeedPreset = ScanSpeedPreset.Normal;
        private CancellationTokenSource _scanCts;
        private bool _isScanningForUI;

        // !!! - ACTIVATE OR DELETE LATER
        //private readonly OuiDatabaseService _ouiDb = new OuiDatabaseService();

        // PUBLIC PROPERTIES, AVAILABLE FOR DATA BINDING

        public ObservableCollection<DeviceInfo> Devices { get; set; } = new ObservableCollection<DeviceInfo>();
        public ObservableCollection<LocalNetworksInfo> AvailableNetworkInterfaces { get; } = new ObservableCollection<LocalNetworksInfo>();
        public LocalNetworksInfo CurrentlySetNetInterface { get; set; }
        public IReadOnlyList<ScanSpeedPreset> ScanSpeedPresets { get; } = Enum.GetValues(typeof(ScanSpeedPreset)).Cast<ScanSpeedPreset>().ToList();

        public int ScannedDevicesCount
        {
            get => _scannedDevicesCount;
            set
            {
                _scannedDevicesCount = value;
                OnPropertyChanged(nameof(ScannedDevicesCount));
                OnPropertyChanged(nameof(ScanProgressPercent));
            }
        }

        public int TotalHostsToScan
        {
            get => _totalHostsToScan;
            set
            {
                _totalHostsToScan = value;
                OnPropertyChanged(nameof(TotalHostsToScan));
                OnPropertyChanged(nameof(ScanProgressPercent));
            }
        }

        public bool IsScanningForUI
        {
            get => _isScanningForUI;
            set
            {
                if (_isScanningForUI != value)
                {
                    _isScanningForUI = value;
                    OnPropertyChanged(nameof(IsScanningForUI));
                }
            }
        }

        public int ScanProgressPercent => TotalHostsToScan == 0 ? 0 : (int)((double)ScannedDevicesCount / TotalHostsToScan * 100);

        public ScanSpeedPreset SelectedScanSpeedPreset
        {
            get => _selectedScanSpeedPreset;
            set
            {
                if (_selectedScanSpeedPreset != value)
                {
                    _selectedScanSpeedPreset = value;
                    OnPropertyChanged(nameof(SelectedScanSpeedPreset));
                }
            }
        }

        public void Cleanup() => StopScan();

        // COMMANDS

        public ICommand ScanCommand => new RelayCommand(async () =>
        {
            if (CurrentlySetNetInterface == null) return;

            IsScanningForUI = true;
            _scanCts = new CancellationTokenSource();
            var speedPreset = SelectedScanSpeedPreset;

            if (!Enum.IsDefined(typeof(ScanSpeedPreset), speedPreset))
            {
                speedPreset = ScanSpeedPreset.Normal;
            }

            string currentIp = CurrentlySetNetInterface.IPv4Address;
            string[] maskParts = CurrentlySetNetInterface.SubnetMask.Split('.');

            Devices.Clear();
            ScannedDevicesCount = 0;
            TotalHostsToScan = 0;

            var deviceProgress = new Progress<DeviceInfo>(device =>
            {
                Devices.Add(device);
            });

            var scanProgress = new Progress<int>(scannedCount =>
            {
                ScannedDevicesCount = scannedCount;
            });

            var (firstIp, endIp) = IpRangeService.GetIpRange(currentIp, maskParts);
            TotalHostsToScan = (int)(HelperIpConverter.IpToUInt(endIp) - HelperIpConverter.IpToUInt(firstIp) + 1);

            try
            {
                await _scanService.ScanSubnetAsync(currentIp, maskParts, speedPreset, deviceProgress, scanProgress, _scanCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Scan was canceled, handle if necessary (TO-DO)
            }
            finally
            {
                IsScanningForUI = false;
            }
        });

        public ICommand StopScanCommand => new RelayCommand(StopScan);
        public ICommand LoadNetworkInterfacesCommand => new RelayCommand(LoadNetworkInterfaces);

        public ICommand NavigateToBluetoothScanningCommand { get; }
        public ICommand NavigateToMainMenuCommand { get; }

        // !!! - ACTIVATE OR DELETE LATER
        // public ICommand TestOUI => new RelayCommand(TestOUIDatabase);

        // CONSTRUCTORS

        public MainScanViewModel(NavigationService navigationService, ViewModelFactory viewModelFactory)
        {
            _navigationService = navigationService;
            _viewModelFactory = viewModelFactory;

            NavigateToBluetoothScanningCommand = new RelayCommand(_ =>
            _navigationService.NavigateTo(_viewModelFactory.CreateBluetoothScanVM()), _ => true);

            NavigateToMainMenuCommand = new RelayCommand(_ =>
            _navigationService.NavigateTo(_viewModelFactory.CreateMainMenuVM()), _ => true);

            LoadNetworkInterfaces();
        }

        public MainScanViewModel() { }

        // METHODS

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

        private void StopScan()
        {
            _scanCts?.Cancel();
        }

        // DELETE LATER
        // !!! - ACTIVATE OR DELETE LATER

        //private void TestOUIDatabase()
        //{
        //    string testMac = "f0-20-FF-22-D9-A9";
        //    OuiRecord record = _ouiDb.GetVendor(testMac);
        //    if (record != null)
        //    {
        //        MessageBox.Show($"Producentem urzadzenia o MAC {testMac} jest {record.Vendor}\nAdres: {record.Address}");
        //    }
        //    else
        //    {
        //        MessageBox.Show("OUI record not found in the database. The given MAC address is most probably random or private");
        //    }
        //}
    }

}
