using NabfClientApplication.Client;
using NabfProject.AI;
using NabfProject.Parsers;
using NabfProject.ServerMessages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            int marsinfo_pos = 0;
            //string master_server = args[0];
            string mars_server = args[marsinfo_pos];
            string username = args[marsinfo_pos+1];
            string password = args[marsinfo_pos+2];



            //IPEndPoint masterServerPoint = CreateIPEndPoint(master_server);
            IPEndPoint marsServerPoint = CreateIPEndPoint(mars_server);

            TcpClient marsClient = new TcpClient();

            Console.WriteLine("Connecting to Mars Server: " + mars_server);

            marsClient.Connect(marsServerPoint);
            Console.WriteLine("Successfully connected to mars server");

            AgentLogicFactory logicFactory = new AgentLogicFactory(username);
            ServerCommunication marsSerCom = new ServerCommunication(
                new StreamReader(marsClient.GetStream()),
                new StreamWriter(marsClient.GetStream()));

            MarsToAgentParser marsToAgentParser = new MarsToAgentParser();
            AgentToMarsParser agentToMarsParser = new AgentToMarsParser();

            ClientApplication client = new ClientApplication(marsSerCom, marsToAgentParser, agentToMarsParser, logicFactory);

            Console.WriteLine("Authenticating: username=" + username + ", password=" + password);

            marsSerCom.SeralizePacket(new AuthenticationRequestMessage(username, password));

            AuthenticationResponseMessage authmsg = (AuthenticationResponseMessage)marsSerCom.DeserializeMessage();

            if (authmsg.Response == ServerResponseTypes.Success)
            {
                Console.WriteLine("Successfully Authenticated");
                client.Start();
            }
            else
            {
                Console.WriteLine("Failed to Authenticate");
            }

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
