using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmasEngineController.AI;
using XmasEngineModel;

namespace NabfProject.AI
{
	public class AgentMaster : AgentManager
	{
		

		protected override Func<KeyValuePair<string, AgentController>> AquireAgentControllerContructor()
		{
			throw new NotImplementedException();
		}
	}
}
