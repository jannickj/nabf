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
    public class ServerCommunication : XmlPacketTransmitter<InternalReceiveMessage, InternalSendMessage>
    {              
        public ServerCommunication(StreamReader reader, StreamWriter writer) : base(reader, writer) { }
        
        //public void SendMessage(ServerMessages.SendMessage message)
        //{
        //    stream.WriteStartDocument(false);
        //    xmlSerSend.Serialize(stream, message);
        //    stream.Flush();
        //}

        //public ServerMessages.ReceiveMessage ReceiveMessage()
        //{
        //    if (reader == null)
        //        reader = XmlReader.Create(this.TestStream);
        //    return (ReceiveMessage) xmlSerReceive.Deserialize(reader);

        //}

        protected override XmlTransmitterMessage<InternalReceiveMessage> ConstrutReceiverMessage()
        {
            return new ReceiveMessage();
        }

        protected override XmlTransmitterMessage<InternalSendMessage> ConstrutSenderMessage(InternalSendMessage data)
        {
            return new SendMessage(data);
        }

        public override void BeforeSerialize(XmlWriter writer, StreamWriter swriter, XmlTransmitterMessage<InternalSendMessage> packet)
        {
            //swriter.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>");
        }

        public override void AfterSerialize(XmlWriter writer, StreamWriter swriter, XmlTransmitterMessage<InternalSendMessage> packet)
        {
            swriter.BaseStream.WriteByte(0);
            swriter.BaseStream.Flush();
        }
    }
}
