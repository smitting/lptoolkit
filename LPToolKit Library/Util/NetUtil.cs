using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace LPToolKit.Util
{
    public class NetUtil
    {
        /// <summary>
        /// Get the name that this note would like to refer to itself as.
        /// </summary>
        /// <returns></returns>
        public static string GetLocalName()
        {
            // TODO: let people configure this
            return System.Environment.MachineName;
        }

        /// <summary>
        /// Gets a local IPv4 address.  Prefers ones starting with
        /// 192.168, but if none found it returns the first.
        /// </summary>
        public static string GetLocalIP()
        {
            string host = Dns.GetHostName();
            var ip = Dns.GetHostEntry(host);

            string bestMatch = null;
            foreach (var addr in ip.AddressList)
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    if (addr.ToString().StartsWith("192.168"))
                    {
                        return addr.ToString();
                    }
                    else if (bestMatch == null)
                    {
                        bestMatch = addr.ToString();
                    }
                }
            }
            return bestMatch;
        }

        /// <summary>
        /// Creates an IPEndPoint object from an IP and a port, and 
        /// the IP can contain the port using : notation.
        /// </summary>
        public static IPEndPoint CreateIPEndPoint(string endPoint, int port = -1)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length != 2 && port < 0) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (!IPAddress.TryParse(ep[0], out ip))
            {
                throw new FormatException("Invalid ip-adress");
            }
            if (ep.Length >= 2)
            {
                if (!int.TryParse(ep[1], out port))
                {
                    throw new FormatException("Invalid port");
                }
            }
            return new IPEndPoint(ip, port);
        }
    }
}
