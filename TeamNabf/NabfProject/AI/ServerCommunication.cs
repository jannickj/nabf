using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace NabfProject.AI
{
    public class ServerCommunication
    {
        private XmlWriter stream;
        private XmlSerializer xmlSer;

        public ServerCommunication(XmlWriter stream, XmlSerializer xmlSer)
        {
            // TODO: Complete member initialization
            this.stream = stream;
            this.xmlSer = xmlSer;
        }
        
        public void SendMessage(ServerMessages.ServerMessage message)
        {
            stream.WriteStartDocument(false);
            xmlSer.Serialize(stream, message);
        }
    }
}
