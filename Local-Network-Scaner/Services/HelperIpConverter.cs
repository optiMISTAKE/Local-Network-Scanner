using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Local_Network_Scanner.Services
{
    public static class HelperIpConverter
    {
        public static uint IpToUInt(int[] ip)
        {
            return ((uint)ip[0] << 24) |
                   ((uint)ip[1] << 16) |
                   ((uint)ip[2] << 8) |
                    (uint)ip[3];
        }

        public static string UIntToIp(uint ip)
        {
            return string.Format("{0}.{1}.{2}.{3}",
                (ip >> 24) & 0xFF,
                (ip >> 16) & 0xFF,
                (ip >> 8) & 0xFF,
                ip & 0xFF);
        }
    }
}
