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

namespace NabfProject.AI.Client
{
	public class ClientApplication
	{
		private IAgentLogic logic;
		private XmlPacketTransmitter<IilPerceptCollection,IilAction> transmitter;
		private HashSet<Thread> activeThreads = new HashSet<Thread>();
		private ConcurrentQueue<IilAction> packets = new ConcurrentQueue<IilAction>();
		private AutoResetEvent packetadded = new AutoResetEvent(false);

		public ClientApplication(XmlPacketTransmitter<IilPerceptCollection, IilAction> transmitter, IAgentLogic logic)
		{
			this.logic = logic;
			this.transmitter = transmitter;

			logic.EvaluationCompleted += logic_EvaluationCompleted;
			logic.JobCreated += logic_JobCreated;
			logic.PerceptsLoaded += logic_PerceptsLoaded;
			logic.JobLoaded += logic_JobLoaded;
		}

		void logic_JobLoaded(object sender, JSLibrary.Data.GenericEvents.UnaryValueEvent<int> evt)
		{
			EvaluateJob(evt.Value);
		}

		void logic_PerceptsLoaded(object sender, EventArgs e)
		{
			var jobsToEval = logic.GetJobs;
			foreach (var job in jobsToEval)
			{
				EvaluateJob(job.Item1);
			}
		}

		void logic_JobCreated(object sender, JSLibrary.Data.GenericEvents.UnaryValueEvent<IilAction> evt)
		{
			this.AddPacket(evt.Value);
		}

		void logic_EvaluationCompleted(object sender, EventArgs e)
		{
			var decision = logic.CurrentDecision;
			this.AddPacket(decision);
		}

		public void UpdateSender()
		{
			this.packetadded.WaitOne();
			bool hasPacket = false;
			do
			{
				IilAction packet;
				hasPacket = this.packets.TryDequeue(out packet);
				if (hasPacket)
				{
					this.transmitter.SeralizePacket(packet);
				}
			} while (hasPacket);

		}

		public void UpdateReceiver()
		{
			var data = transmitter.DeserializePacket();
			if(data.Percepts.Count != 0)
			{
				logic.HandlePercepts(data);
			}
		}

		public void StartThread(Action action)
		{
			var thread = new Thread(new ThreadStart(action));
			lock (activeThreads)
			{
				this.activeThreads.Add(thread);
			}
			thread.Start();
		}

		
		public void EvaluateJob(int jobid)
		{
			StartThread(() =>
				{
					var info = this.logic.EvaluateJob(jobid);
					if (info.Item2)
					{
						
						this.AddPacket(info.Item1);
					}

				});
		}

		private void AddPacket(IilAction packet)
		{
			this.packets.Enqueue(packet);
			this.packetadded.Set();
		}
		
	}
}
