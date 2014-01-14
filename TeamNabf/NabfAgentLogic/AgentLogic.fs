namespace NabfAgentLogic
    module AgentLogic=
        open System
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

        (* handlePercept State -> Percept -> State *)
        let handlePercept state percept =
            match percept with
                | EnemySeen enemy   
                    -> { state with Enemies = enemy :: state.Enemies }
                | VertexSeen vertex 
                    -> { state with World = addVertex state.World vertex }
                | EdgeSeen edge          
                    -> { state with World = addEdge state.World edge }
                | ProbedVertex vertex
                    -> { state with World = addVertexValue state.World vertex }
                | SurveyedEdge edge
                    -> { state with World = addEdgeCost state.World edge}
                | Achievement achievement 
                    -> { state with Achievements = state.Achievements.Add achievement }
                | SimulationStep step
                    -> { state with SimulationStep = step }

        (* let updateState : State -> Percept list -> State *)
        let updateState state = 
            List.fold handlePercept state

        (* chooseAction : State -> Percept list -> Action *)
        let chooseAction currentState percepts =
            let newState = updateState currentState percepts
            0
