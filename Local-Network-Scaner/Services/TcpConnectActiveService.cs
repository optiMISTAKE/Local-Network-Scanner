using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Local_Network_Scanner.Services
{
    public enum ScanStatus
    {
        Open,
        Closed, // Connection Refused (RST)
        Timeout // No response (Packet Loss)
    }
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

        public record ScanResult(int Port, ScanStatus Status, string Message);

        public async Task<ScanResult> ProbeTcpPort(string ipAddress, int port, int timeout, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var tcpClient = new TcpClient();

                tcpClient.LingerState = new LingerOption(true, 0);

                var connectTask = tcpClient.ConnectAsync(ipAddress, port);
                var delayTask = Task.Delay(timeout);

                var completedTask = await Task.WhenAny(connectTask, delayTask);

                if (completedTask == connectTask)
                {
                    await connectTask; // Ensure any exceptions are observed
                    // At this point, the connection was successful
                    try
                    {
                            tcpClient.Close();
                    }
                    catch { }
                    return new ScanResult(port, ScanStatus.Open, "Open");
                    
                }
                else // Timeout occurred
                {
                    try { tcpClient.Close(); } catch { }
                    return new ScanResult(port, ScanStatus.Timeout, "Timed out");
                }
            }
            catch (SocketException sockEx)
            {
                // Connection attempt failed with a socket error (SocketErrorCode 10061 is "Connection Refused")
                if (sockEx.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    return new ScanResult(port, ScanStatus.Closed, "Refused");
                }
                // Check for "No Buffer Space"
                // treating it as a Timeout/Skip allows the scan to continue without crashing.
                if (sockEx.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                {
                    return new ScanResult(port, ScanStatus.Timeout, "Skipped (Socket Resource Exhaustion)");
                }

                // Other errors might be network unreachable, etc. Treat as Timeout/Error
                return new ScanResult(port, ScanStatus.Timeout, $"Error: {sockEx.Message}");
            }
            catch (Exception ex)
            {
                // Connection attempt failed
                return new ScanResult(port, ScanStatus.Timeout, $"Error: {ex.Message}");
            }
        }
    }
}
