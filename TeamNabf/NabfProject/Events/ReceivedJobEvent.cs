using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NabfProject.NoticeBoardModel;
using XmasEngineModel.Management;
using NabfProject.AI;

namespace NabfProject.Events
{
    public class ReceivedJobEvent : XmasEvent
    {
        public Notice Notice { get; private set; }
        public NabfAgent Receiver { get; private set; }

        public ReceivedJobEvent(Notice n, NabfAgent agent)
        {
            Notice = n;
            Receiver = agent;
        }

		public override string ToString()
		{
			return "Recieved job: " + Notice.GetType().Name+" "+Notice.Id;
		}
    }
}
