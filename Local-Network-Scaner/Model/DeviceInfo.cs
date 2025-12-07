using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Local_Network_Scaner.Model
{
    public class DeviceInfo
    {
        public string IPAddress { get; set; }
        public bool IsActive { get; set; }
        public List<int> OpenPorts { get; set; } = new List<int>();
        public string Banner { get; set; }
        public string MACAddress { get; set; }
        public string Vendor { get; set; }
        public string HostName { get; set; }

        public string OpenPortsDisplay
        {
            get
            {
                if (OpenPorts == null || OpenPorts.Count == 0)
                    return "None";
                return string.Join(", ", OpenPorts);
            }
        }

        public string IsActiveDisplay
        {
            get
            {
                return IsActive ? "Active" : "Inactive";
            }
        }
    }
}
