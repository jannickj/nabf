﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NabfProject.ServerMessages
{
    [XmlRoot("message")]
    public class ReceiveMessage : IXmlSerializable
    {
        public InternalReceiveMessage Message { get; private set; }
        public long Timestamp { get; private set; }
        public string Type { get; private set; }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();
            var message = ServerMessageFactory.Instance.ConstructMessage(reader["type"]);
            var isEmpty = reader.IsEmptyElement;

            Timestamp = Convert.ToInt64(reader["timestamp"]);
            Type = reader["type"];

            reader.Read();
            
            this.Message = message;
            message.ReadXml(reader);
            if(!isEmpty)
                reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            throw new NotImplementedException();
        }

    }
}
