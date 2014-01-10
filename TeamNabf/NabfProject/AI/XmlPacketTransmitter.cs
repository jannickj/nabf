using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using JSLibrary.IiLang;
using JSLibrary.IiLang.DataContainers;

namespace NabfProject.AI
{
	public class XmlPacketTransmitter<TRecieve,TSend> 
		where TRecieve : IXmlSerializable
		where TSend : IXmlSerializable
	{
		private XmlReader reader;
		private XmlWriter writer;
		private XmlSerializer serializerReciever;
		private XmlSerializer serializerSender;

		public XmlPacketTransmitter(XmlReader reader, XmlWriter writer)
		{
			
			this.reader = reader;
			this.writer = writer;
			serializerReciever = new XmlSerializer(typeof(TRecieve));
			serializerSender = new XmlSerializer(typeof(TSend));
		}

		public TRecieve DeserializePacket()
		{
			reader.ReadStartElement("packet");
			var percepts = (TRecieve)serializerReciever.Deserialize(reader);
			reader.ReadEndElement();
			return percepts;

		}

		public void SeralizePacket(TSend action)
		{
			writer.WriteStartElement("packet");
			serializerSender.Serialize(writer, action);
			writer.WriteEndElement();
		}
	}
}
