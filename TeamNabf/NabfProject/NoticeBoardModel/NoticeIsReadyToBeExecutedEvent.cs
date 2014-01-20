using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NabfProject.AI;

namespace NabfProject.NoticeBoardModel
{
    public class NoticeIsReadyToBeExecutedEvent : XmasEngineModel.Management.XmasEvent
    {
        public Notice Notice { get; private set; }
        public List<NabfAgent> Agents { get; private set; }

        public NoticeIsReadyToBeExecutedEvent(Notice n, List<NabfAgent> agents)
        {
            Notice = n;
            Agents = agents;
        }
    }
    
}
