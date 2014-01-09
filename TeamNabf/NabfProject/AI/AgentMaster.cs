using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NabfProject.Actions;
using XmasEngineController.AI;
using XmasEngineModel;

namespace NabfProject.AI
{
	public class AgentMaster : AgentManager
	{
		private TcpListener listener;
		private int agentid = 0;

		public AgentMaster(TcpListener listener)
		{
			this.listener = listener;
		}

		protected override Func<KeyValuePair<string, AgentController>> AquireAgentControllerContructor()
		{
			TcpClient client = listener.AcceptTcpClient();
			string agentName = agentid + "";
			agentid++;
			Func<KeyValuePair<string, AgentController>> func = () => 
			{ 
			
				Agent agent = new Agent(agentName);
			
				AgentConnection connection = new AgentConnection(agent);
				this.ActionManager.Queue(new AddStandbyAgentAction(agent));
				return new KeyValuePair<string,AgentController>(agentName, connection);
			};
			return func;
		}
	}
}
