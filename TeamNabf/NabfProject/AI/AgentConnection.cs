using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSLibrary.IiLang;
using JSLibrary.IiLang.DataContainers;
using XmasEngineController.AI;
using XmasEngineModel.EntityLib;
using XmasEngineModel.Management;

namespace NabfProject.AI
{
	public class AgentConnection : AgentController
	{
		private ThreadSafeEventManager evtman = new ThreadSafeEventManager();
		private XmlPacketTransmitter<IilAction, IilPerceptCollection> transmitter;

		public AgentConnection(Agent agent, XmlPacketTransmitter<IilAction, IilPerceptCollection> transmitter)
			: base(agent)
		{
			this.transmitter = transmitter;
			evtman.AddEventQueue(agent.ConstructEventQueue());
		}


		public override void Start()
		{
			throw new NotImplementedException();
		}
	}
}
