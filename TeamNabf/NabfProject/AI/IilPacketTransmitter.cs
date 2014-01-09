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
	public class IilPacketTransmitter
	{
		private XmlReader reader;
		private XmlWriter writer;
		private XmlSerializer serializerReciever;
		private XmlSerializer serializerSender;

		public IilPacketTransmitter(XmlReader reader, XmlWriter writer)
		{
			
			this.reader = reader;
			this.writer = writer;
			serializerReciever = new XmlSerializer(typeof(IilPerceptCollection));
			serializerSender = new XmlSerializer(typeof(IilAction));
		}

		public IilPerceptCollection DeserializePacket()
		{
			reader.ReadStartElement("packet");
			var percepts = (IilPerceptCollection)serializerReciever.Deserialize(reader);
			reader.ReadEndElement();
			return percepts;

		}

		public void SeralizePacket(IilAction action)
		{
			writer.WriteStartElement("packet");
			serializerSender.Serialize(writer, action);
			writer.WriteEndElement();
		}
	}
}
