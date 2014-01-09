using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NabfProject.AI;
using System.IO;
using NabfProject.ServerMessages;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace NabfTest
{
    [TestFixture]
    public class ServerCommunicationTest
    {

        [Test]
        public void SendMessage_Connected_XMLmessageSent()
        {
            string username = "a1";
            string password = "1";
           
            StringBuilder sb = new StringBuilder();
            //StreamReader strRead = new StreamReader(memStream);
            XmlSerializer xmlSer = new XmlSerializer(typeof(ServerMessage));
            XmlWriterSettings xmlSettings = new XmlWriterSettings() {OmitXmlDeclaration = false};
            ServerCommunication servCom = new ServerCommunication(XmlWriter.Create(sb,xmlSettings), xmlSer);
            AuthenticationMessage message = new AuthenticationMessage(username, password);
            servCom.SendMessage(message);


            string actual = XDocument.Parse(sb.ToString()).ToString();
            string expected = XDocument.Parse("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>"
                                +"<message type=\"auth-request\">"
                                +"<authentication password=\""+password+"\" username=\""+username+"\"/>"
                                + "</message>").ToString();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SendMessage_Connected_XMLmessageSent2()
        {
            string actionType = "goto";
            string actionParam = "vertex1";

            StringBuilder sb = new StringBuilder();
            //StreamReader strRead = new StreamReader(memStream);
            XmlSerializer xmlSer = new XmlSerializer(typeof(ServerMessage));
            XmlWriterSettings xmlSettings = new XmlWriterSettings() { OmitXmlDeclaration = false };
            ServerCommunication servCom = new ServerCommunication(XmlWriter.Create(sb, xmlSettings), xmlSer);
            ActionMessage message = new ActionMessage(actionType, actionParam);
            servCom.SendMessage(message);


            string actual = XDocument.Parse(sb.ToString()).ToString();
            string expected = XDocument.Parse("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
                                                +"<message type=\"action\">"
                                                +"<action type=\""+actionType+"\" param=\""+actionParam+"\">"
                                                +"</message>").ToString();

            Assert.AreEqual(expected, actual);
        }
    }
    
}
