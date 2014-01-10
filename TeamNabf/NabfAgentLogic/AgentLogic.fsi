namespace NabfAgentLogic
    module AgentLogic=

        open Graph

        type Action = int

        type AgentType =
            | Saboteur
            | Explorer
            | Repairer
            | Inspector
            | Sentinel

        type Agent =
            {
                Id          : string;
                Type        : AgentType;
                Energy      : int;
                Health      : int;
                Strength    : int;
                VisionRange : int;
            }

        type Percept =
            | EnemySeen of Agent
            | NodeSeen of Graph.Vertex

        type State =
            {
                World : Graph;
                Self : Agent;
                Enemies : Agent list;
            }