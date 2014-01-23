using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using JSLibrary.IiLang;
using JSLibrary.IiLang.DataContainers;
using JSLibrary.IiLang.Parameters;
using NabfAgentLogic.AgentInterfaces;
using System.Collections.Concurrent;
using Microsoft.FSharp.Collections;
using NabfAgentLogic;
using JSLibrary.Data.GenericEvents;
using XmasEngineModel.Interfaces;
using NabfProject.ServerMessages;
using NabfProject.Parsers;
using NabfProject.AI;

namespace NabfClientApplication.Client
{
	public class ClientApplication : IStartable
	{
        private object simLock = new object();
        private int currentSimId = -1;
		private IAgentLogic currentLogic;
        private AgentLogicFactory logicFactory;
		//private XmlPacketTransmitter<IilPerceptCollection,IilAction> agentServCom;
		private HashSet<Thread> activeThreads = new HashSet<Thread>();
        private List<Tuple<int, InternalSendMessage>> marsPackets = new List<Tuple<int, InternalSendMessage>>();
        private List<Tuple<int, IilAction>> masterPackets = new List<Tuple<int, IilAction>>();
		private AutoResetEvent marsPacketAdded = new AutoResetEvent(false);
        private AutoResetEvent masterPacketAdded = new AutoResetEvent(false);
		private ServerCommunication marsServCom;
        private MarsToAgentParser marsToAgentParser;
        private AgentToMarsParser agentToMarsParser;
        private XmlPacketTransmitter<IilPerceptCollection, IilAction> masterSerCom;
        private DateTime actionTimeStart;

        
        public event UnaryValueHandler<Tuple<ActionMessage,TimeSpan>> ActionSent;

        public ClientApplication(XmlPacketTransmitter<IilPerceptCollection, IilAction> masterSerCom, ServerCommunication marsServCom, MarsToAgentParser marsParser, AgentToMarsParser agentToMarsParser, AgentLogicFactory factory)
		{
            this.masterSerCom = masterSerCom;
            this.marsServCom = marsServCom;
            this.marsToAgentParser = marsParser;
            this.agentToMarsParser = agentToMarsParser;
            //this.agentServCom = agentServCom;
            this.logicFactory = factory;
            //logic.EvaluationStarted += logic_needMessageSent;
            actionTimeStart = DateTime.Now;
		}

		public void UpdateMarsSender()
		{
			this.marsPacketAdded.WaitOne();

            Tuple<int, InternalSendMessage>[] packets;
            lock (marsPackets)
            {
                packets = this.marsPackets.ToArray();
                this.marsPackets.Clear();
            }

            foreach(var packet in packets)
            {
			    bool packetAccepted = false;
                lock (simLock)
                {
                    if (packet.Item1 == this.currentSimId)
                        packetAccepted = true;
                }
                if (ActionSent != null)
                {
                    var evtArgs = Tuple.Create((ActionMessage)packet.Item2, DateTime.Now - actionTimeStart);
                            
                    ActionSent(this, new UnaryValueEvent<Tuple<ActionMessage, TimeSpan>>(evtArgs));
                }

                if (packetAccepted)
                    this.marsServCom.SeralizePacket(packet.Item2);
                
			}

		}

        public void UpdateMasterSender()
        {
            this.masterPacketAdded.WaitOne();

            Tuple<int, IilAction>[] packets;
            lock (masterPackets)
            {
                packets = this.masterPackets.ToArray();
                this.marsPackets.Clear();
            }

            foreach (var packet in packets)
            {
                bool packetAccepted = false;
                lock (simLock)
                {
                    if (packet.Item1 == this.currentSimId)
                        packetAccepted = true;
                }
               
                if (packetAccepted)
                    this.masterSerCom.SeralizePacket(packet.Item2);

            }

        }

		public void UpdateMarsReceiver()
		{
            var data = (ReceiveMessage) marsServCom.DeserializePacket();
            IAgentLogic logic;
            lock (simLock)
            {
                logic = this.currentLogic;
            

                if (data.Message is SimStartMessage)
                {
                    StartSim((SimStartMessage)data.Message);
                    logic = this.currentLogic;
                }
                else if (data.Message is RequestActionMessage)
                {
                    actionTimeStart = DateTime.Now;
                }
            }

            var percepts = (IilPerceptCollection) marsToAgentParser.ConvertToForeign(data);

            if (percepts.Percepts.Count != 0)
			{
                lock (logic)
                {
                    logic.HandlePercepts(percepts);
                }
			}
            
		}

        public void UpdateMasterReceiver()
        {
            var percepts = masterSerCom.DeserializeMessage();
            

            if (percepts.Percepts.Count != 0)
            {
                lock (this.currentLogic)
                {
                    this.currentLogic.HandlePercepts(percepts);
                }
            }

        }


		private void StartThread(Action action)
		{
			var thread = new Thread(new ThreadStart(action));
			lock (activeThreads)
			{
				this.activeThreads.Add(thread);
			}
			thread.Start();
		}


		private void AddMarsPacket(int id, InternalSendMessage packet)
		{
            lock(this.marsPackets)
			    this.marsPackets.Add(Tuple.Create(id,packet));
			this.marsPacketAdded.Set();
		}

        private void AddMasterPacket(int id, IilAction packet)
        {
            lock(this.masterPackets)
                this.masterPackets.Add(Tuple.Create(id, packet));
            this.masterPacketAdded.Set();
        }

        private void StartSim(SimStartMessage msg)
        {
            lock (simLock)
            {
                
                currentSimId = msg.Id;
                currentLogic = logicFactory.ConstructAgentLogic();

                int simId = currentSimId;
                currentLogic.SendAgentServerAction += (sender, evt) =>
                    {
                        lock (simLock)
                        {
                            bool acceptedPacket = false;
                            lock (simLock)
                            {
                                if (currentSimId == simId)
                                    acceptedPacket = true;
                            }
                            if (acceptedPacket)
                            {
                                this.AddMasterPacket(currentSimId, evt.Value);
                            }
                        }
                    };
                currentLogic.SendMarsServerAction += (sender,evt) => 
                    {
                        bool acceptedPacket = false;
                        lock(simLock)
                        {
                            if (currentSimId == simId)
                                acceptedPacket = true;
                        }
                        if (acceptedPacket)
                        {
                            var marsMsg = agentToMarsParser.ConvertToForeign(evt.Value);
                            this.AddMarsPacket(currentSimId, marsMsg);
                        }
                    };

            }

            
        }

        public void Start()
        {
			this.currentLogic = this.logicFactory.ConstructAgentLogic ();
            ReceiveMessage msg = (ReceiveMessage) marsServCom.DeserializePacket();
            if(msg.Message is SimEndMessage)
                msg = (ReceiveMessage)marsServCom.DeserializePacket();
            SimStartMessage sMsg = (SimStartMessage) msg.Message;
            var sMsgPercepts = (IilPerceptCollection) marsToAgentParser.ConvertToForeign(msg);
            StartSim(sMsg);
            currentLogic.HandlePercepts(sMsgPercepts);

			Thread marsSenderThread = new Thread(new ThreadStart(() => ExecuteThread(() => { while (true) this.UpdateMarsSender(); })));
            Thread marsReceiverThread = new Thread(new ThreadStart(() => ExecuteThread(() => { while (true) this.UpdateMarsReceiver(); })));
            Thread masterSenderThread = new Thread(new ThreadStart(() => { while (true) this.UpdateMasterSender(); }));
            Thread masterReceiverThread = new Thread(new ThreadStart(() => { while (true) this.UpdateMasterReceiver(); }));
            marsSenderThread.Start();
            marsReceiverThread.Start();
            masterSenderThread.Start();
            masterReceiverThread.Start();
        }

        private void ExecuteThread(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Console.WriteLine("Client failure: " + e.Message);
                Environment.Exit(1); 
            }
        }

        

        public void Initialize()
        {
            
        }
    }
}
