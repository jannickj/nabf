using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NabfClientApplication
{
	class Program
	{
		static void Main(string[] args)
		{
            string master_server = args[0];
            string mars_server = args[1];
            string username = args[2];
            string password = args[3];



            IPEndPoint masterServerPoint = CreateIPEndPoint(master_server);
            IPEndPoint marsServerPoint = CreateIPEndPoint(mars_server);

            TcpClient marsClient = new TcpClient();
            //mars_client.Connect(
		}

        public static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (ep.Length > 2)
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    var addresses = System.Net.Dns.GetHostAddresses(ep[0]);
                    if (addresses.Length == 0)
                    {
                        throw new FormatException(ep[0] + " is invalid ip-adress or is unable to retrieve address from specified host name ");
                    }
                    else
                    {
                        ip = addresses[0];
                    }
                    
                }
            }
            int port;
            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }
	}
}
