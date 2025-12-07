using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Local_Network_Scaner.Services
{
    public class TcpConnectActiveService
    {
        public readonly int[] CommonPorts = new[]
        {
            20,   // FTP data
            21,   // FTP control
            22,   // SSH
            23,   // Telnet
            25,   // SMTP
            53,   // DNS (TCP fallback)
            80,   // HTTP
            110,  // POP3
            143,  // IMAP
            443,  // HTTPS
            445,  // Microsoft-DS (SMB)
            3306, // MySQL
            3389, // RDP
            5432, // PostgreSQL
            5900, // VNC
            6379, // Redis
            27017,// MongoDB
            8080  // HTTP-alt / common proxy
        };

        public record ScanResult(int Port, bool IsOpen, string Message);

        public async Task<ScanResult> ProbeTcpPort(string ipAddress, int port, int timeout)
        {
            using var tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(ipAddress, port);
            var delayTask = Task.Delay(timeout);

            var completedTask = await Task.WhenAny(connectTask, delayTask);

            if (completedTask == connectTask)
            {
                try
                {
                    await connectTask; // Ensure any exceptions are observed
                    // At this point, the connection was successful
                    try
                    {
                        tcpClient.Close();
                    }
                    catch { }
                    return new ScanResult(port, true, "TCP connect succeeded. Port is open");
                }
                catch (SocketException sockEx)
                {
                    // Connection attempt failed with a socket error
                    return new ScanResult(port, false, $"TCP connect failed, socket exception: {sockEx.Message}");
                }
                catch (Exception ex)
                {
                    // Connection attempt failed
                    return new ScanResult(port, false, $"TCP connect failed: {ex.Message}");
                }
            }
            else // Timeout occurred
            {
                try { tcpClient.Close(); } catch { }
                return new ScanResult(port, false, "TCP connect timed out");
            }
        }
    }
}
