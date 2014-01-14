using NabfProject.AI;
using NabfProject.ServerMessages;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace NabfTest
{
    [TestFixture]
    public class ServerApplicationTest
    {
        StreamReader reader;
        StreamWriter writer;
        ServerCommunication servCom;
        private TcpClient tcpClient;


        [Test]
        public void ConnectToServerAndAuthenticate_Connected_AllAgentsAuthenticated()
        {
            int nrOfAgents = 28;
            //string agentName = "a";
            //string[] passwords = Enumerable.Repeat("pass", nrOfAgents).ToArray();
            //string authmessage = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>"
            //                    + "<message type=\"auth-request\">"
            //                    + "<authentication password=\"" + "pass" + "\" username=\"" + "a1" + "\"/>"
            //                    + "</message>";
            //string authResponse;

            tcpClient = new TcpClient();
            tcpClient.Connect("localhost", 12300);
            //IPAddress ipa = IPAddress.Parse("::1");
            //IPEndPoint ipeh = new IPEndPoint(ipa, 12300);
            //Socket connection = new Socket(
            //       AddressFamily.InterNetworkV6,
            //       SocketType.Stream,
            //       ProtocolType.Tcp);
            //connection.Connect(ipeh);
            //Assert.True(connection.Connected);
            
            //Stream streamWrite = tcpClient.GetStream();
            //Stream streamRead = tcpClient.GetStream();

            //Stream stream = new NetworkStream(connection);

            Stream stream = tcpClient.GetStream();

            StreamWriter streamWriter = new StreamWriter(stream);
            StreamReader streamReader = new StreamReader(stream);

            servCom = new ServerCommunication(streamReader, streamWriter);

            servCom.SeralizePacket(new AuthenticationRequestMessage("Nabf1", "pass"));

            //servCom.SeralizePacket(new ActionMessage("recharge",""));
            var authMessage = servCom.DeserializeMessage();

            var simStartMessage = servCom.DeserializeMessage();


            //XmlWriter xmlWriter = XmlWriter.Create(streamWrite);

            //servCom = new ServerCommunication(xmlWriter, null, xmlSerSend, xmlSerReceive);
            //servCom.TestStream = tcpClient.GetStream();

            //ServerApplication servApp = new ServerApplication(servCom, nrOfAgents, agentName, passwords);

            //servApp.Start();
            
        }
    }
}
