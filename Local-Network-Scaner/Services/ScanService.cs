using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Local_Network_Scaner.Model;
using static Local_Network_Scaner.Services.TcpConnectActiveService;

namespace Local_Network_Scaner.Services
{
    public class ScanService
    {
        public async Task<List<DeviceInfo>> ScanSubnetAsync(string baseIp)
        {
            var tasks = new List<Task<DeviceInfo>>();

            for (int i = 1; i <= 254; i++)
            {
                string ipAddress = $"{baseIp}.{i}";
                Debug.WriteLine($"Scanning {ipAddress}");
                tasks.Add(ScanSingleHost(ipAddress));
            }

            return (await Task.WhenAll(tasks)).Where(d => d != null && d.IsActive).ToList();

        }

        public async Task<DeviceInfo> ScanSingleHost(string ipAddress)
        {
            var device = new DeviceInfo { IPAddress = ipAddress };

            // 1. Ping the host
            device.IsActive = await new SimplePingService().PingAsync(ipAddress, 300);

            if (!device.IsActive)
            {
                device.IsActive = await new SimplePingService().PingAsync(ipAddress, 1000);
            }

            // ...

            // 3. TCP Connect Scan
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
            var tasks = new List<Task<ScanResult>>();

            foreach (var port in new TcpConnectActiveService().CommonPorts)
            {
                await sem.WaitAsync();
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var result = await new TcpConnectActiveService().ProbeTcpPort(ipAddress, port, timeout);
                        
                        if (result.IsOpen) device.OpenPorts.Add(port);

                        return result;
                    }
                    finally
                    {
                        sem.Release();
                    }
                }));
            }

            var results = await Task.WhenAll(tasks);

            return device;
        }
    }
}
