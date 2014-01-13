using NabfProject.AI;
using NabfProject.ServerMessages;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        StringBuilder sb;
        XmlWriterSettings xmlSettings;
        XmlSerializer xmlSerSend;
        XmlSerializer xmlSerReceive;
        XmlReader reader;
        XmlWriter sw;
        ServerCommunication servCom;
        MemoryStream memStream;
        private TcpClient tcpClient;

        [SetUp]
        public void Init()
        {
            sb = new StringBuilder();
            memStream = new MemoryStream();
            xmlSettings = new XmlWriterSettings() { OmitXmlDeclaration = false };
            sw = XmlWriter.Create(memStream, xmlSettings);

            tcpClient = new TcpClient();

            xmlSerSend = new XmlSerializer(typeof(SendMessage));
            xmlSerReceive = new XmlSerializer(typeof(ReceiveMessage));
            memStream.Position = 0;
            reader = XmlReader.Create(memStream);
            servCom = new ServerCommunication(XmlWriter.Create(sb, xmlSettings), reader, xmlSerSend, xmlSerReceive);
        }

        [Test]
        public void ConnectToServerAndAuthenticate_Connected_AllAgentsAuthenticated()
        {
            int nrOfAgents = 28;
            string agentName = "a";
            string[] passwords = Enumerable.Repeat("pass", nrOfAgents).ToArray();
            string authmessage = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>"
                                + "<message type=\"auth-request\">"
                                + "<authentication password=\"" + "pass" + "\" username=\"" + "a1" + "\"/>"
                                + "</message>";
            string authResponse;

            tcpClient.Connect("10.16.174.83", 12300);
            Assert.True(tcpClient.Connected);
            
            //Stream streamWrite = tcpClient.GetStream();
            //Stream streamRead = tcpClient.GetStream();

            Stream stream = tcpClient.GetStream();

            StreamWriter streamWriter = new StreamWriter(stream);
            StreamReader streamReader = new StreamReader(stream);

            streamWriter.Write(authmessage);
            streamWriter.Flush();
            int peek = streamReader.Peek();
            authResponse = streamReader.Read().ToString();

            //XmlWriter xmlWriter = XmlWriter.Create(streamWrite);

            //servCom = new ServerCommunication(xmlWriter, null, xmlSerSend, xmlSerReceive);
            //servCom.TestStream = tcpClient.GetStream();

            //ServerApplication servApp = new ServerApplication(servCom, nrOfAgents, agentName, passwords);

            //servApp.Start();
            
        }
    }
}
