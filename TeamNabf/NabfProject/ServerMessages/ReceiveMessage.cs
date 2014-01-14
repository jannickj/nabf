﻿using NabfProject.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NabfProject.ServerMessages
{
    public class ReceiveMessage : XmlTransmitterMessage<InternalReceiveMessage>
    {
        public long Timestamp { get; private set; }
        public string Type { get; private set; }
        
        public void ReadXml(System.Xml.XmlReader reader)
        {
            //reader.MoveToContent();
            //var message = ServerMessageFactory.Instance.ConstructMessage(reader["type"]);
            //var isEmpty = reader.IsEmptyElement;

            //Timestamp = Convert.ToInt64(reader["timestamp"]);
            //Type = reader["type"];

            //reader.Read();
            
            //this.Message = message;
            //message.ReadXml(reader);
            //if(!isEmpty)
            //    reader.ReadEndElement();


        }

        public override InternalReceiveMessage ConstructMessage(System.Xml.XmlReader reader)
        {
            return ServerMessageFactory.Instance.ConstructMessage(Type);
        }

        public override void ReadNode(System.Xml.XmlReader reader)
        {
            Timestamp = Convert.ToInt64(reader["timestamp"]);
            Type = reader["type"];
        }

        public override string NodeName
        {
            get
            {
                return "message";
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            throw new NotImplementedException();
        }

    }
}
