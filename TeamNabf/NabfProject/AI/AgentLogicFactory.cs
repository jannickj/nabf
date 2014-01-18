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

        public virtual IAgentLogic ConstructAgentLogic()
        {
            return new AgentLogicClient();
        }
    }
}
