using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using JSLibrary.IiLang;
using JSLibrary.IiLang.DataContainers;
using NabfProject.Actions;
using XmasEngineController.AI;
using XmasEngineModel;
using XmasEngineModel.EntityLib;

namespace NabfProject.AI
{
	public class AgentMaster : AgentManager
	{
		private TcpListener listener;
		private int agentid = 0;

		public AgentMaster(TcpListener listener)
		{
			this.listener = listener;
		}

		protected override Func<KeyValuePair<string, AgentController>> AquireAgentControllerContructor()
		{
			TcpClient client = listener.AcceptTcpClient();
			string agentName = agentid + "";
			agentid++;
			Func<KeyValuePair<string, AgentController>> func = () => 
			{ 
			
				Agent agent = new Agent(agentName);
				XmlReader reader = XmlReader.Create(client.GetStream(),new XmlReaderSettings(){ConformanceLevel=ConformanceLevel.Fragment});
				XmlWriter writer = XmlWriter.Create(client.GetStream(),new XmlWriterSettings(){ConformanceLevel = System.Xml.ConformanceLevel.Fragment});
				XmlPacketTransmitter<IilAction, IilPerceptCollection> transmitter = new XmlPacketTransmitter<IilAction, IilPerceptCollection>(reader, writer);
				AgentConnection connection = new AgentConnection(agent,transmitter);
				this.ActionManager.Queue(new AddStandbyAgentAction(agent));
				return new KeyValuePair<string,AgentController>(agentName, connection);
			};
			return func;
		}
	}
}
