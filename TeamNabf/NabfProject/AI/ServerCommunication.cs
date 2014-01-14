using NabfProject.ServerMessages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace NabfProject.AI
{
    public class ServerCommunication
    {
        private XmlWriter stream;
        private XmlSerializer xmlSerSend;
        private XmlReader reader;
        private XmlSerializer xmlSerReceive;


        public Stream TestStream { get; set; }

        public ServerCommunication(XmlWriter stream, XmlReader reader, XmlSerializer xmlSerSend, XmlSerializer xmlSerReceive)
        {
            // TODO: Complete member initialization
            this.stream = stream;
            this.xmlSerSend = xmlSerSend;
            this.reader = reader;
            this.xmlSerReceive = xmlSerReceive;

            
            
        }
        
        public void SendMessage(ServerMessages.SendMessage message)
        {
            stream.WriteStartDocument(false);
            xmlSerSend.Serialize(stream, message);
            stream.Flush();
        }

        public ServerMessages.ReceiveMessage ReceiveMessage()
        {
            if (reader == null)
                reader = XmlReader.Create(this.TestStream);
            return (ReceiveMessage) xmlSerReceive.Deserialize(reader);

        }
	
    }
}
