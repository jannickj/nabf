namespace NabfAgentLogic
    module AgentLogic=
        open System
        open Graph
        open JSLibrary.IiLang
        open JSLibrary.IiLang.DataContainers

        type Action = int
        type JobID = int
        type JobValue = int
        type Desirability = int
        type AgentID = string

        type JobType = 
            | OccupyJob = 1
            | RepairJob = 2
            | DisruptJob = 3
            | AttackJob = 4

        type JobData =
            | Occupy of Vertex list
            | Repair of Vertex * AgentID
            | Disrupt  of Vertex
            | Attack of Vertex list

        type JobHeader = JobID * JobValue * JobType

        type Job = JobHeader * JobData



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
            | Achievement    of string
            | SimulationStep of int

        type ServerData = 
            | PerceptCollection of Percept list
            | NewJobs of Job list
            | AcceptedJob of JobID
            | SimulationEnd

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
                | Achievement achievement 
                    -> { state with Achievements = state.Achievements.Add achievement }
                | SimulationStep step
                    -> { state with SimulationStep = step }

        (* let updateState : State -> Percept list -> State *)
        let updateState state percepts = 
            List.fold handlePercept state percepts

  

        (* chooseAction : State -> Percept list -> Action *)
        let chooseAction (currentState:State) =
            //let newState = updateState currentState percepts
            0

        let buildIilAction (action:Action) =
            new IilAction "some action"

        let buildJobAccept (job:Job) =
            new IilAction "some action"

        let parseIilPercepts (perceptCollection:IilPerceptCollection) =
            PerceptCollection (List<Percept>.Empty)

        let generateJobs  (state:State) (jobs:Job list) = 
            List<Job>.Empty

        let generateActions (state:State) =
            List<Action>.Empty

        let actionDesirability (state:State) (action:Action) =
            0
        let actionDesirabilityBasedOnJob (state:State) (oldDesirability,action:Action) (job:Job) =
            0
        let generateJob (jt:JobType) (s:State) (knownJobs:Job list)  =
            option<Job>.None

        let buildJob (job:Job) = 
            new IilAction "some action"
        let decideJob (job:Job) =
            let d:Desirability = 1
            d