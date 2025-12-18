using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace Local_Network_Scanner.Services
{
    public class ReverseDnsService
    {
        public async Task<string?> TryGetHostname(string ipAddress, int timeoutMs = 500)
        {
            try
            {
                using var cancellationTokenSource = new CancellationTokenSource(timeoutMs);
                var hostEntryTask = Dns.GetHostEntryAsync(ipAddress);
                var completedTask = await Task.WhenAny(hostEntryTask, Task.Delay(timeoutMs, cancellationTokenSource.Token));

                if (completedTask != hostEntryTask)
                {
                    // Timeout occurred
                    return null;
                }
                var hostEntry = await hostEntryTask;

                return hostEntry.HostName;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
