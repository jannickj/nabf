using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabfProject.ServerMessages
{
    public class ActionMessage : InternalSendMessage
    {
        private string actionType;
        private string actionParam;

        public ActionMessage(string actionType, string actionParam)
        {
            this.actionType = actionType;
            this.actionParam = actionParam;
        }

        protected override string MessageType
        {
            get { return "action"; }
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            throw new NotImplementedException();
        }

        protected override void WriteXmlInternal(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("action");
            writer.WriteAttributeString("type", actionType);
            if  ("" != actionParam)
                writer.WriteAttributeString("param", actionParam);
            writer.WriteEndElement();
        }
    }
}
