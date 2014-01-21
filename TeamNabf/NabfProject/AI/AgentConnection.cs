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
using JSLibrary.ParallelLib;
using System.Threading;
using NabfProject.Events;
using System.Xml.Linq;
using NabfProject.Parsers;
using NabfProject.Actions;

namespace NabfProject.AI
{
	public class AgentConnection : AgentController
	{

        private object simLock = new object();
        private bool simulationSubscribed = false;
        private ConcurrentQueue<IilPerceptCollection> packets = new ConcurrentQueue<IilPerceptCollection>();
        private AutoResetEvent packetadded = new AutoResetEvent(false);
		private XmlPacketTransmitter<IilAction, IilPerceptCollection> transmitter;
        private AgentMasterToAgentParser masterToAgentParser;
        private AgentToAgentMasterParser agentToMasterParser;

		public AgentConnection  (   Agent agent
                                ,   XmlPacketTransmitter<IilAction, IilPerceptCollection> transmitter
                                ,   AgentMasterToAgentParser masterToAgentParser
                                ,   AgentToAgentMasterParser agentToMasterParser
                                )
			: base(agent)
		{
			this.transmitter = transmitter;
            this.Agent.Register(new Trigger<NewKnowledgeEvent>(evt => receivedEvent(evt)));
            this.Agent.Register(new Trigger<NewNoticeEvent>(evt => receivedEvent(evt)));
            this.Agent.Register(new Trigger<NoticeRemovedEvent>(evt => receivedEvent(evt)));
            this.Agent.Register(new Trigger<NoticeUpdatedEvent>(evt => receivedEvent(evt)));
            this.Agent.Register(new Trigger<ReceivedAssignmentEvent>(evt => receivedEvent(evt)));
            this.Agent.Register(new Trigger<SimulationSubscribed>(simSubscribedEvent));
			this.masterToAgentParser = masterToAgentParser;
            this.agentToMasterParser = agentToMasterParser;
		}


		public override void Start()
		{
            new Thread(new ThreadStart(() => { while (true) updateSend();})).Start();
            while (true)
            {
                updateReceive();
            }
		}

        private void updateReceive()
        {

            var action = transmitter.DeserializeMessage();
            var agent = (NabfAgent)this.Agent;
            var xaction = agentToMasterParser.ConvertToKnown(action);
            if (xaction is SubscribeSimulationAction)
                lock (simLock)
                    this.simulationSubscribed = false;
        }

        private void updateSend()
        {
            this.packetadded.WaitOne();
            bool hasPacket = false;
            do
            {
                IilPerceptCollection packet;
                hasPacket = this.packets.TryDequeue(out packet);
                if (hasPacket)
                {
                    this.transmitter.SeralizePacket(packet);
                }
            } while (hasPacket);
        }

        private void simSubscribedEvent(SimulationSubscribed evt)
        {
            lock (simLock)
                this.simulationSubscribed = true;
        }

        private void receivedEvent(XmasEvent evt)
        {
            var packet = this.masterToAgentParser.ConvertToForeign(evt);

            lock (simLock)
                if (simulationSubscribed)
                    addPacket(packet);
        }

        private void addPacket(IilPerceptCollection packet)
        {
            this.packets.Enqueue(packet);
            this.packetadded.Set();
        }

	}
}
