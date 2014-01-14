using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using JSLibrary.IiLang;
using JSLibrary.IiLang.DataContainers;
using System.IO;

namespace NabfProject.AI
{
    
	public class XmlPacketTransmitter<TRecieve,TSend> 
		where TRecieve : IXmlSerializable
		where TSend : IXmlSerializable
	{
        private StreamReader reader;
        private StreamWriter writer;
		private XmlSerializer serializerReciever;
		private XmlSerializer serializerSender;
        private XmlReader xreader;
        private XmlWriter xwriter;

        public XmlPacketTransmitter(StreamReader reader, StreamWriter writer)
		{
			
			this.reader = reader;
			this.writer = writer;
			serializerReciever = new XmlSerializer(typeof(TRecieve));
			serializerSender = new XmlSerializer(typeof(TSend));
		}

        protected virtual XmlTransmitterMessage<TRecieve> ConstrutReceiverMessage()
        {
            return new XmlTransmitterMessage<TRecieve>();
        }

        protected virtual XmlTransmitterMessage<TSend> ConstrutSenderMessage(TSend data)
        {
            return new XmlTransmitterMessage<TSend>(data);
        }


		public XmlTransmitterMessage<TRecieve> DeserializePacket()
		{
            
            if(xreader == null)
                xreader = XmlReader.Create(reader, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment });

            var msg = this.ConstrutReceiverMessage();
            msg.ReadXml(xreader);

			return msg;

		}

        public TRecieve DeserializeMessage()
        {
            return this.DeserializePacket().Message;
        }

		public void SeralizePacket(TSend action)
		{
            SeralizePacket(this.ConstrutSenderMessage(action));
		}

        public void SeralizePacket(XmlTransmitterMessage<TSend> packet)
        {
            if (xwriter == null)
                xwriter = XmlWriter.Create(writer, new XmlWriterSettings() { ConformanceLevel = System.Xml.ConformanceLevel.Fragment });

            packet.WriteXml(this.xwriter,this.serializerSender);
            this.xwriter.Flush();
        }
	}
}
