using JSLibrary.IiLang;
using NabfProject.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XmasEngineModel.Management;

namespace NabfProject.Events
{
    public class AgentReceivedPerceptsEvent : XmasEvent
    {

        public NabfAgent Agent { get; private set; }
        public IilPerceptCollection Percepts { get; private set; }
        public AgentReceivedPerceptsEvent(NabfAgent agent, IilPerceptCollection percepts)
        {
            this.Agent = agent;
            this.Percepts = percepts;
        }

    }
}
