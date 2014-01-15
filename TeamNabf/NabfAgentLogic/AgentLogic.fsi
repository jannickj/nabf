namespace NabfAgentLogic
    module AgentLogic=

        open Graph

        type Action = int

        type AgentRole =
            | Saboteur
            | Explorer
            | Repairer
            | Inspector
            | Sentinel

        type Agent =
            { Energy      : int
            ; Health      : int
            ; MaxEnergy   : int
            ; MaxHealth   : int
            ; Name        : string
            ; Node        : string
            ; Role        : AgentRole
            ; Strength    : int
            ; Team        : string
            ; VisionRange : int
            ; Position    : string
            }

        type Percept =
            | EnemySeen      of Agent
            | VertexSeen     of Graph.Vertex
            | EdgeSeen       of Graph.Edge
            | ProbedVertex   of Graph.Vertex
            | SurveyedEdge   of Graph.Edge
            | Achievement    of string
            | SimulationStep of int

        type State =
            { World          : Graph
            ; Self           : Agent
            ; Enemies        : Agent list
            ; Achievements   : Set<string>
            ; SimulationStep : int
            }