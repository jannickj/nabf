namespace NabfAgentLogic
    module AgentLogic=
        open System
        open Graph
        open JSLibrary.IiLang
        open JSLibrary.IiLang.DataContainers

        type Action = int

        type JobID = int
        type Desirability = int
        type Job = int

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

        (* handlePercept State -> Percept -> State *)
        let handlePercept state percept =
            match percept with
                | EnemySeen enemy -> { state with Enemies = enemy :: state.Enemies }
        //        | NodeSeen node -> { state with World = 
                | _ -> state

        (* let updateState : State -> Percept list -> State *)
        let updateState state percepts = 
            List.fold handlePercept state percepts

  

        (* chooseAction : State -> Percept list -> Action *)
        let chooseAction (currentState:State) =
            //let newState = updateState currentState percepts
            0

        let buildIilAction (action:Action) =
            new IilAction "some action"

        let parseIilPercepts (perceptCollection:IilPerceptCollection) =
            List<Percept>.Empty

        let generateJobs  (state:State) (jobs:Job list) = 
            List<Job>.Empty

        let generateActions (state:State) =
            List<Action>.Empty

        let actionDesirability (state:State) (action:Action) =
            0