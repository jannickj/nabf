using NabfProject;
using NabfProject.AI;
using NabfProject.SimManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using XmasEngine;
using XmasEngineController;
using XmasEngineModel.Management;
using XmasEngineView;

namespace NabfServerApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] ipp = args[0].Split(new Char[]{':'});
            string ip = ipp[0];
            int port = Convert.ToInt32(ipp[1]);

            TcpListener listener = new TcpListener(IPAddress.Parse(ip),port);

            listener.Start();

            EmptyWorldBuilder builder = new EmptyWorldBuilder();
            
            NabfModelFactory factory = new NabfModelFactory();

            NabfModel model = (NabfModel)factory.ConstructModel(builder);

            XmasEngineManager engine = new XmasEngineManager(factory);

            AgentMaster agentmaster = new AgentMaster(listener);

            engine.StartEngine(model, new XmasView[0], new XmasController[] { agentmaster });

        }
    }
}
