using Local_Network_Scanner.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Radios;

namespace Local_Network_Scanner.Services
{
    public class BluetoothScanService: IDisposable
    {
        // private fields
        private readonly BluetoothUuidService _bluetoothUuidService = new BluetoothUuidService();
        private BluetoothLEAdvertisementWatcher? _watcher;
        private CancellationTokenSource? _cts;
        private readonly OuiDatabaseService _ouiDb = new OuiDatabaseService();

        public event Action<BluetoothDeviceInfo>? DeviceFound;
        public bool IsScanning => _watcher != null;

        // Parameterless constructor to load Bluetooth UUID database
        public BluetoothScanService()
        {
            // Load the OUI and UUID database on initialization
            string pathUuid = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Resources",
            "bluetooth-16-bit-uuids-2022-05-19.csv"
            );

            string pathOui = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Resources",
            "oui.csv"
            );

            _bluetoothUuidService.LoadBluetoothUuidDatabase(pathUuid);
            _ouiDb.LoadDatabaseCSV(pathOui);
        }

        public void Start(ScanSpeedPreset speed, CancellationToken cancellationToken)
        {
            if (_watcher != null) return;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            UseSpeedPreset(speed);

            _watcher.Received += OnAdvertisementReceived;
            try
            {
                _watcher.Start();
            }
            catch (COMException ex)
            {
                // Handle exception if Bluetooth is not available
                Console.WriteLine($"Error starting Bluetooth LE watcher: {ex.Message}");
                _watcher.Received -= OnAdvertisementReceived;
                _watcher = null;
            }

        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (_watcher == null) return;
            _watcher.Stop();
            _watcher.Received -= OnAdvertisementReceived;
            _watcher = null;
        }

        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (_cts?.Token.IsCancellationRequested == true) return;

            var deviceInfo = BluetoothDeviceInfo.FromAdvertisement(args, _bluetoothUuidService);
            DeviceFound?.Invoke(deviceInfo);
        }

        private void UseSpeedPreset(ScanSpeedPreset speed)
        {
            if (_watcher == null) return;
            switch (speed)
            {
                case ScanSpeedPreset.Slow:
                    _watcher.SignalStrengthFilter.SamplingInterval = TimeSpan.FromSeconds(2);
                    break;

                case ScanSpeedPreset.Normal:
                    _watcher.SignalStrengthFilter.SamplingInterval = TimeSpan.FromSeconds(1);
                    break;

                case ScanSpeedPreset.Aggressive:
                    _watcher.SignalStrengthFilter.SamplingInterval = TimeSpan.FromMilliseconds(300);
                    break;

                default:
                    // fallback
                    _watcher.SignalStrengthFilter.SamplingInterval = TimeSpan.FromSeconds(1);
                    break;
            }
        }

        public async Task<bool> IsBluetoothEnabledAsync()
        {
            var radios = await Radio.GetRadiosAsync();
            var bluetoothRadio = radios.FirstOrDefault(r => r.Kind == RadioKind.Bluetooth);
            return bluetoothRadio != null && bluetoothRadio.State == RadioState.On;
        }

        public void Dispose() => Stop();
    }
}
