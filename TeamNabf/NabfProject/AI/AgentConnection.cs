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

namespace NabfProject.AI
{
	public class AgentConnection : AgentController
	{

        private ConcurrentQueue<IilPerceptCollection> packets = new ConcurrentQueue<IilPerceptCollection>();
        private AutoResetEvent packetadded = new AutoResetEvent(false);
		private XmlPacketTransmitter<IilAction, IilPerceptCollection> transmitter;

		public AgentConnection(Agent agent, XmlPacketTransmitter<IilAction, IilPerceptCollection> transmitter)
			: base(agent)
		{
			this.transmitter = transmitter;
			this.Agent.Register(new Trigger<AgentReceivedPerceptsEvent>(receivedPercepts));
            
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
            
            var action = transmitter.DeserializePacket();
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

        private void receivedPercepts(AgentReceivedPerceptsEvent evt)
        {
            this.packets.Enqueue(evt.Percepts);
            this.packetadded.Set();
        }
	}
}
