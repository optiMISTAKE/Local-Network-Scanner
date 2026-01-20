using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Local_Network_Scanner.Services;
using Windows.Devices.Bluetooth.Advertisement;

namespace Local_Network_Scanner.Model
{
    public class BluetoothDeviceInfo
    {
        //  Bluetooth Device Address
        // similar to MAC address, usually represented as a hexadecimal string
        public string BluetoothAdressHex { get; set; } = "";
        public ulong BluetoothAddress { get; set; }
        public string? LocalName { get; set; }

        // relative quality level of a Bluetooth signal received on a device (in dBm)
        // closer to zero means a stronger signal
        public short Rssi { get; set; }
        public string? Vendor {  get; set; }

        public DateTimeOffset Timestamp { get; set; }
        public string ServiceSummary { get; set; } = "";

        public static BluetoothDeviceInfo FromAdvertisement(BluetoothLEAdvertisementReceivedEventArgs args, BluetoothUuidService uuidService)
        {
            var device = new BluetoothDeviceInfo
            {
                BluetoothAddress = args.BluetoothAddress,
                BluetoothAdressHex = args.BluetoothAddress.ToString("X"),
                LocalName = args.Advertisement.LocalName,
                Rssi = args.RawSignalStrengthInDBm,
                Timestamp = args.Timestamp
            };

            if (args.Advertisement.ServiceUuids != null)
            {
                var sb = new StringBuilder();
                foreach (var uuid in args.Advertisement.ServiceUuids)
                {
                    var record = uuidService.GetBluetoothRecord(uuid.ToString());
                    if (record != null)
                    {
                        if (sb.Length > 0) sb.Append(", ");
                        sb.Append($"UUID: {record.uuid}; Allocation Type: {record.AllocationType}; Allocated For {record.AllocatedFor}");
                    }
                }
                device.ServiceSummary = sb.ToString();
            }

            return device;
        }

    }
}
