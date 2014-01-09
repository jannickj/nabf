using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using JSLibrary.IiLang;
using JSLibrary.IiLang.DataContainers;
using NabfAgentLogic.AgentInterfaces;


namespace NabfProject.AI.Client
{
	public class ClientApplication
	{
		private IAgentLogic logic;
		private IilPacketTransmitter transmitter; 

		public ClientApplication(IilPacketTransmitter transmitter, IAgentLogic logic)
		{
			this.logic = logic;
			this.transmitter = transmitter;
		}

		public void Start()
		{
			
			
		}


		
	}
}
