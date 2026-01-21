using Local_Network_Scanner.Interfaces;
using Local_Network_Scanner.Model;
using Local_Network_Scanner.Services;
using Local_Network_Scanner.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Radios;

namespace Local_Network_Scanner.ViewModel
{
    public class BluetoothScanViewModel : ViewModelBase, ICleanup
    {
        // PRIVATE FIELDS
        private readonly NavigationService _navigationService;
        private readonly ViewModelFactory _viewModelFactory;
        private readonly BluetoothScanService _bluetoothScanService;
        private ScanSpeedPreset _selectedScanSpeedPreset = ScanSpeedPreset.Normal;
        private CancellationTokenSource? _bluetoothScanCts;
        private DialogService _dialogService = new DialogService();

        // PUBLIC PROPERTIES, AVAILABLE FOR DATA BINDING
        public ObservableCollection<BluetoothDeviceInfo> BluetoothDevices { get; } = new ObservableCollection<BluetoothDeviceInfo>();
        public ICommand StartScanCommand { get; }
        public ICommand StopScanCommand { get; }
        public bool IsScanningForUI
        {
            get => _bluetoothScanService.IsScanning;
        }
        public IReadOnlyList<ScanSpeedPreset> ScanSpeedPresets { get; } = Enum.GetValues(typeof(ScanSpeedPreset)).Cast<ScanSpeedPreset>().ToList();
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

        public ICommand NavigateToMainMenuCommand { get; }


        // CONSTRUCTORS
        public BluetoothScanViewModel(NavigationService navigationService, ViewModelFactory viewModelFactory)
        {
            _navigationService = navigationService;
            _viewModelFactory = viewModelFactory;

            NavigateToMainMenuCommand = new RelayCommand(_ =>
            {
                Cleanup();
                _navigationService.NavigateTo(_viewModelFactory.CreateMainMenuVM());
            }, _ => true);

            // TO-DO : Initialize BluetoothScanService, start scanning, and handle device discovery events
            _bluetoothScanService = new BluetoothScanService();
            _bluetoothScanService.DeviceFound += OnDeviceDiscovered;

            StartScanCommand = new RelayCommand(StartScan);
            StopScanCommand = new RelayCommand(StopScan);
        }

        // METHODS
        private async Task StartScan()
        {
            _bluetoothScanCts = new CancellationTokenSource();
            var speed = SelectedScanSpeedPreset;

            if(!Enum.IsDefined(typeof(ScanSpeedPreset), speed))
            {
                speed = ScanSpeedPreset.Normal;
            }

            bool isBluetoothEnabled = await _bluetoothScanService.IsBluetoothEnabledAsync();
            if (!isBluetoothEnabled)
            {
                string title = System.Windows.Application.Current.Resources["Alert_BtDisabled_Title"] as string
                       ?? "Bluetooth Disabled";
                string message = System.Windows.Application.Current.Resources["Alert_BtDisabled_Msg"] as string
                                 ?? "Please enable Bluetooth.";

                bool openSettings = _dialogService.ShowConfirmation(title, message);

                if (openSettings)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "ms-settings:bluetooth",
                        UseShellExecute = true
                    });
                }

                return;
            }

            _bluetoothScanService.Start(speed, _bluetoothScanCts.Token);
            OnPropertyChanged(nameof(IsScanningForUI));
        }

        public void StopScan()
        {
            _bluetoothScanCts?.Cancel();
            _bluetoothScanService.Stop();
            OnPropertyChanged(nameof(IsScanningForUI));
        }


        private void OnDeviceDiscovered(BluetoothDeviceInfo device)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;

            if (dispatcher == null || dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
            {
                return;
            }

            try
            {
                // Always marshal to UI thread because ObservableCollection
                // is bound to the WPF UI
                dispatcher.Invoke(() =>
                {
                    // Try to find an existing device by Bluetooth address
                    var existing = BluetoothDevices.FirstOrDefault(
                        d => d.BluetoothAddress == device.BluetoothAddress);

                    if (existing == null)
                    {
                        // New device → add it to the list
                        BluetoothDevices.Add(device);
                    }
                    else
                    {
                        // Existing device → update dynamic fields
                        existing.Rssi = device.Rssi;
                        existing.ServiceSummary = device.ServiceSummary;
                        existing.Timestamp = device.Timestamp;

                        // Optional: update name if it appears later
                        if (!string.IsNullOrWhiteSpace(device.LocalName))
                        {
                            existing.LocalName = device.LocalName;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                // Catching specific exceptions like TaskCanceledException is good, 
                // but during shutdown, a general catch is a safe "last resort" 
                // to prevent the process from terminating abruptly.
                Debug.WriteLine($"Suppressed shutdown crash: {ex.Message}");
            }
        }

    }
}
