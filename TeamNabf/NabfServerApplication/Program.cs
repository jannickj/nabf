﻿using NabfProject;
using NabfProject.Actions;
using NabfProject.AI;
using NabfProject.Events;
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
using XmasEngineModel.Management.Actions;
using XmasEngineModel.Management.Events;
using XmasEngineView;

namespace NabfServerApplication
{
    class Program
    {
        private static int simId = -1;
        private static int roundId = -1;
        private static int agentoffset = 3;
        private static int start = 0;
        private static Dictionary<NabfAgent, int> consolepos = new Dictionary<NabfAgent, int>();


        static void Main(string[] args)
        {
            Console.Clear();
            string[] ipp = args[0].Split(new Char[]{':'});
            string ip = ipp[0];
            int port = Convert.ToInt32(ipp[1]);

            TcpListener listener = new TcpListener(IPAddress.Parse(ip),port);

            listener.Start();

            EmptyWorldBuilder builder = new EmptyWorldBuilder();
            
            NabfModelFactory factory = new NabfModelFactory();

            NabfModel model = (NabfModel)factory.ConstructModel(builder);

            model.EventManager.Register(new Trigger<ActionCompletedEvent<AddXmasObjectAction>>(AddedXmasObject));
            model.EventManager.Register(new Trigger<ActionFailedEvent>(evt =>
                {
                    Console.SetCursorPosition(0, 1);
                    Console.Write("Error occured with " + evt.FailedAction.GetType().Name + ": " + evt.Exception.Message);
                }));
           

            XmasEngineManager engine = new XmasEngineManager(factory);

            AgentMaster agentmaster = new AgentMaster(listener);

            engine.StartEngine(model, new XmasView[0], new XmasController[] { agentmaster });

        }

        private static void ReceivedMessage(EntityXmasAction<NabfAgent> action)
        {
            Console.SetCursorPosition(15, consolepos[action.Source] * 2 + agentoffset);
            Console.Write("Received: " + action + "\t\t");
        }

        private static void SendMessage(NabfAgent agent, XmasEvent evt)
        {
            Console.SetCursorPosition(15, consolepos[agent] * 2 + agentoffset+1);
            Console.Write("Sent: " + evt + "\t\t");
        }

        private static void AddedXmasObject(ActionCompletedEvent<AddXmasObjectAction> evten)
        {
            if (evten.Action.Object is NabfAgent)
            {
                var agent = (NabfAgent)evten.Action.Object;
                Console.SetCursorPosition(0, start * 2 + agentoffset);
                Console.Write("Agent: "+agent.Name);
                consolepos.Add(agent, start);
                agent.Register(new Trigger<ActionStartingEvent<AddKnowledgeAction>>(evt => ReceivedMessage(evt.Action)));
                agent.Register(new Trigger<ActionStartingEvent<ApplyNoticeAction>>(evt => ReceivedMessage(evt.Action)));
                agent.Register(new Trigger<ActionStartingEvent<ChangeNoticeAction>>(evt => ReceivedMessage(evt.Action)));
                agent.Register(new Trigger<ActionStartingEvent<CreateNoticeAction>>(evt => ReceivedMessage(evt.Action)));
                agent.Register(new Trigger<ActionStartingEvent<DeleteNoticeAction>>(evt => ReceivedMessage(evt.Action)));
                agent.Register(new Trigger<ActionStartingEvent<NewRoundAction>>(evt => ReceivedMessage(evt.Action)));
                agent.Register(new Trigger<ActionStartingEvent<SubscribeSimulationAction>>(evt => ReceivedMessage(evt.Action)));
                agent.Register(new Trigger<ActionStartingEvent<AgentCrashed>>(evt =>
                    {
                        var message = "Crashed! (" + evt.Action.Exception.Message.Substring(0, 20) + "...)";
                        Console.SetCursorPosition(15, consolepos[agent] * 2 + agentoffset);
                        Console.Write("Received: " + message + "\t\t");

                        Console.SetCursorPosition(15, consolepos[agent] * 2 + agentoffset+1);
                        Console.Write("Sent: " + message + "\t\t");
                    }));

                //agent.Register(new Trigger<NewKnowledgeEvent>(evt => SendMessage(agent, evt)));
                agent.Register(new Trigger<NewNoticeEvent>(evt => SendMessage(agent, evt)));
                agent.Register(new Trigger<NoticeRemovedEvent>(evt => SendMessage(agent, evt)));
                agent.Register(new Trigger<NoticeUpdatedEvent>(evt => SendMessage(agent, evt)));
                agent.Register(new Trigger<ReceivedJobEvent>(evt => SendMessage(agent, evt)));
                agent.Register(new Trigger<SimulationSubscribedEvent>(evt => SendMessage(agent, evt)));

                agent.Register(new Trigger<ActionStartingEvent<NewRoundAction>>(evt =>
                {
                    bool updated = false;
                    if (simId != evt.Action.SimId)
                    {
                        updated = true;
                        simId = evt.Action.SimId;
                    }
                    if (roundId != evt.Action.RoundNumber)
                    {
                        updated = true;
                        roundId = evt.Action.RoundNumber;
                    }
                    if (updated)
                    {
                        Console.SetCursorPosition(0, 0);
                        Console.Write("Simulation: " + simId + ", Round: " + roundId + "\t\t");
                    }


                }));
                start++;
            }
        }
    }
}
