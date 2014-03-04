using NabfAgentLogic;
using NabfAgentLogic.AgentInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.AI
{
    public class AgentLogicFactory
    {
        private string name;
		private bool debugmode = false;

        public AgentLogicFactory(string agentName)
        {
            this.name = agentName;
        }

        public virtual IAgentLogic ConstructAgentLogic()
        {
			if (forcedMove == null)
				return new AgentLogicClient(name);
			else
				return new AgentLogicClient(name, forcedMove);
			
        }

		public void SetDebugMode()
		{
			debugmode = true;
		}
	}
}
