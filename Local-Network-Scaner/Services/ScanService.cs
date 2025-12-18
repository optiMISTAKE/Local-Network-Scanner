using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Local_Network_Scanner.Model;
using static Local_Network_Scanner.Services.TcpConnectActiveService;
using System.IO;

namespace Local_Network_Scanner.Services
{
    public class ScanService
    {
        private readonly OuiDatabaseService _ouiDb = new OuiDatabaseService();
        private readonly SimplePingService _pingService = new SimplePingService();
        private readonly ArpService _arpService = new ArpService();
        private readonly TcpConnectActiveService _tcpConnectService = new TcpConnectActiveService();
        private readonly ReverseDnsService _reverseDnsService = new ReverseDnsService();

        // Parameterless constructor to load OUI database
        public ScanService()
        {
            // Load the OUI database on initialization
            string path = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Resources",
            "oui.csv"
            );

            _ouiDb.LoadDatabaseCSV(path);
        }
        public async Task ScanSubnetAsync(string currentIp, string[] maskParts, IProgress<DeviceInfo> progress, IProgress<int> scanProgress)
        {
            int scannedCount = 0;
            int maxConcurrency = 50;
            using var sem = new SemaphoreSlim(maxConcurrency);

            var tasks = new List<Task>();

            (int[] firstIp, int[] endIp) = IpRangeService.GetIpRange(currentIp, maskParts);
            Debug.WriteLine($"Scanning IP range: {string.Join('.', firstIp)} - {string.Join('.', endIp)}");

            uint start = HelperIpConverter.IpToUInt(firstIp);
            uint end = HelperIpConverter.IpToUInt(endIp);

            uint totalHosts = end - start + 1;

            if (totalHosts > 10000)
                throw new InvalidOperationException("Subnet too large to scan safely.");
            // TO-DO: allow user to decide whether to proceed or not.

            for (uint ip = start; ip <= end; ip++)
            {
                string ipAddress = HelperIpConverter.UIntToIp(ip);
                Debug.WriteLine($"Scanning {ipAddress}");

                await sem.WaitAsync();

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var deviceInfo = await ScanSingleHost(ipAddress);
                        if (deviceInfo != null && deviceInfo.IsActive)
                        {
                            progress.Report(deviceInfo);
                        }
                    }
                    finally
                    {
                        Interlocked.Increment(ref scannedCount);
                        scanProgress.Report(scannedCount);
                        sem.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

        }

        public async Task<DeviceInfo> ScanSingleHost(string ipAddress)
        {
            var device = new DeviceInfo { IPAddress = ipAddress };

            // 1. Ping the host
            device.IsActive = await _pingService.PingAsync(ipAddress, 300);

            if (!device.IsActive)
            {
                device.IsActive = await _pingService.PingAsync(ipAddress, 1000);
            }

            // 2. Reverse DNS Lookup
            if (device.IsActive)
            {
                device.HostName = await _reverseDnsService.TryGetHostname(ipAddress, 500);
            }

            // 3. Get MAC Address and Vendor

            string? hostMacAddress = _arpService.GetMacAddress(ipAddress);
            device.MACAddress = hostMacAddress;

            if (!string.IsNullOrEmpty(device.MACAddress))
            {
                OuiRecord ouiRecord = _ouiDb.GetVendor(device.MACAddress);
                Debug.WriteLine($"OUI Lookup for {device.MACAddress}: {ouiRecord?.Vendor}");
                if (ouiRecord != null)
                {
                    device.Vendor = ouiRecord.Vendor;
                }
            }

            // 4. TCP Connect Scan

            //foreach (var port in new TcpConnectActiveService().CommonPorts)
            //{
            //    var result = await new TcpConnectActiveService().TryTCPConnectAsync(ipAddress, port, 500);
            //    if (result.IsOpen)
            //    {
            //        device.OpenPorts.Add(port);
            //    }
            //}

            int maxConcurrency = 100;
            int timeout = 500;

            using var sem = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task>();

            foreach (var port in _tcpConnectService.CommonPorts)
            {
                await sem.WaitAsync();
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var result = await _tcpConnectService.ProbeTcpPort(ipAddress, port, timeout);
                        
                        if (result.IsOpen)
                        {
                            lock (device.OpenPorts)
                                device.OpenPorts.Add(port);
                        }

                        return result;
                    }
                    finally
                    {
                        sem.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

            return device;

        }
    }
}
