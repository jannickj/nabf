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
        private ConcurrentQueue<Tuple<int, InternalSendMessage>> marsPackets = new ConcurrentQueue<Tuple<int, InternalSendMessage>>();
		private AutoResetEvent marsPacketAdded = new AutoResetEvent(false);
		private ServerCommunication marsServCom;
        private MarsToAgentParser marsToAgentParser;
        private AgentToMarsParser agentToMarsParser;
        private DateTime actionTimeStart;

        public event UnaryValueHandler<Tuple<ActionMessage,TimeSpan>> ActionSent;

        public ClientApplication(ServerCommunication marsServCom, MarsToAgentParser marsParser, AgentToMarsParser agentToMarsParser, AgentLogicFactory factory)
		{
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
			bool hasPacket = false;
			do
			{
                Tuple<int, InternalSendMessage> packet;
				hasPacket = this.marsPackets.TryDequeue(out packet);
				if (hasPacket)
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
			} while (hasPacket);

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
			this.marsPackets.Enqueue(Tuple.Create(id,packet));
			this.marsPacketAdded.Set();
		}


        private void StartSim(SimStartMessage msg)
        {
            lock (simLock)
            {
                
                currentSimId = msg.Id;
                currentLogic = logicFactory.ConstructAgentLogic();

                int simId = currentSimId;
                //currentLogic.SendAgentServerAction += (sender,evt) => 
                //    {
                //        lock(simLock)
                //        {
                //            if (currentSimId == simId)
                //                logic_SendAgentServerAction(sender, evt);
                //        }
                //    };
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
            ReceiveMessage msg = (ReceiveMessage) marsServCom.DeserializePacket();
            SimStartMessage sMsg = (SimStartMessage) msg.Message;
            var sMsgPercepts = (IilPerceptCollection) marsToAgentParser.ConvertToForeign(msg);
            StartSim(sMsg);
            currentLogic.HandlePercepts(sMsgPercepts);

            Thread marsSenderThread = new Thread(new ThreadStart(() => ExecuteThread(() => { while (true) this.UpdateMarsSender(); })));
            Thread marsReceiverThread = new Thread(new ThreadStart(() => ExecuteThread(() => { while (true) this.UpdateMarsReceiver(); })));
            marsSenderThread.Start();
            marsReceiverThread.Start();
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
                //Environment.Exit(1); 
            }
        }

        

        public void Initialize()
        {
            
        }
    }
}
