using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabfProject.ServerMessages
{
    public class VisibleVerticesMessage : InternalReceiveMessage
    {
        private List<InternalReceiveMessage> visibleVertices = new List<InternalReceiveMessage>();

        public List<InternalReceiveMessage> VisibleVertices
        {
            get { return visibleVertices; }
        }

        private string messageName;

        public string MessageName
        {
            get { return messageName; }
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {            
            reader.MoveToContent();
            messageName = reader.LocalName;
            reader.Read();

            while (reader.LocalName != "visibleVertices")
            {
                var message = ServerMessageFactory.Instance.ConstructMessage(reader.LocalName);
                message.ReadXml(reader);
                visibleVertices.Add(message);
                reader.Read();
            } 
        }
    }
}
