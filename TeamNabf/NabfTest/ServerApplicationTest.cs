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

            //tcpClient = new TcpClient();
            //tcpClient.Connect("localhost", 12300);

            //Stream stream = tcpClient.GetStream();

            //StreamWriter streamWriter = new StreamWriter(stream);
            //StreamReader streamReader = new StreamReader(stream);

            //servCom = new ServerCommunication(streamReader, streamWriter);            

            for (int i = 1; i <= nrOfAgents; i++)
            {
                tcpClient = new TcpClient();
                tcpClient.Connect("localhost", 12300);

                Stream stream = tcpClient.GetStream();

                StreamWriter streamWriter = new StreamWriter(stream);
                StreamReader streamReader = new StreamReader(stream);

                servCom = new ServerCommunication(streamReader, streamWriter);
                servCom.SeralizePacket(new AuthenticationRequestMessage("Nabf" + i, "pass"));

                var authMessage = servCom.DeserializeMessage();

                var simStartMessage = servCom.DeserializeMessage();
            }

            
            

            //int id = Convert.ToInt32(((SimStartMessage)simStartMessage).Response["id"]);
            //int simstartid = id;

            //var actionReqMessage = (RequestActionMessage)servCom.DeserializeMessage();

            //id = ((PerceptionMessage)actionReqMessage.Response).Id;
            //int firstactionid = id;

            //servCom.SeralizePacket(new ActionMessage(id, "recharge"));

            //actionReqMessage = (RequestActionMessage)servCom.DeserializeMessage();

            //id = ((PerceptionMessage)actionReqMessage.Response).Id;
            //int secondactionid = id;
            



            //XmlWriter xmlWriter = XmlWriter.Create(streamWrite);

            //servCom = new ServerCommunication(xmlWriter, null, xmlSerSend, xmlSerReceive);
            //servCom.TestStream = tcpClient.GetStream();

            //ServerApplication servApp = new ServerApplication(servCom, nrOfAgents, agentName, passwords);

            //servApp.Start();
            
        }

        [Test]
        public void SendActionToServer_Connected_ReceiveLastActionResultSuccessful()
        {
            int nrOfAgents = 1;        

            tcpClient = new TcpClient();
            tcpClient.Connect("localhost", 12300);

            Stream stream = tcpClient.GetStream();

            StreamWriter streamWriter = new StreamWriter(stream);
            StreamReader streamReader = new StreamReader(stream);

            servCom = new ServerCommunication(streamReader, streamWriter);
            servCom.SeralizePacket(new AuthenticationRequestMessage("Nabf" + 1, "pass"));

            var authMessage = servCom.DeserializeMessage();


            var simStartMessage = servCom.DeserializeMessage();

			var simMsg = (SimStartMessage)simStartMessage;

            int id = simMsg.Id;
            int simstartid = id;

            var actionReqMessage = (RequestActionMessage)servCom.DeserializeMessage();

            id = ((PerceptionMessage)actionReqMessage.Response).Id;
            int firstactionid = id;

            servCom.SeralizePacket(new ActionMessage(id, "recharge"));

            actionReqMessage = (RequestActionMessage)servCom.DeserializeMessage();

            string lastAction = ((SelfMessage)((PerceptionMessage)actionReqMessage.Response).Elements[1]).LastAction;
            string lastActionResult = ((SelfMessage)((PerceptionMessage)actionReqMessage.Response).Elements[1]).LastActionResult;
            Assert.AreEqual("successful", lastActionResult);
            Assert.AreEqual("recharge", lastAction);


        }
    }
}
