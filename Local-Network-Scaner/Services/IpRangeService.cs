using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Local_Network_Scanner.Services
{
    public static class IpRangeService
    {
        public static (int[] startIp, int[] endIp) GetIpRange(string baseIp, string[] subnetMask)
        {
            var baseIpParts = baseIp.Split('.').Select(int.Parse).ToArray();
            var maskParts = subnetMask.Select(int.Parse).ToArray();
            int startIp0 = (baseIpParts[0] & maskParts[0]) ;
            int startIp1 = (baseIpParts[1] & maskParts[1]);
            int startIp2 = (baseIpParts[2] & maskParts[2]);
            int startIp3 = (baseIpParts[3] & maskParts[3]) + 1;
            int endIp0 = (baseIpParts[0] | (~maskParts[0] & 0xFF)) ;
            int endIp1 = (baseIpParts[1] | (~maskParts[1] & 0xFF));
            int endIp2 = (baseIpParts[2] | (~maskParts[2] & 0xFF));
            int endIp3 = (baseIpParts[3] | (~maskParts[3] & 0xFF)) - 1;
            return (new int[] { startIp0, startIp1, startIp2, startIp3 },
                    new int[] { endIp0, endIp1, endIp2, endIp3 });
        }
    }
}
