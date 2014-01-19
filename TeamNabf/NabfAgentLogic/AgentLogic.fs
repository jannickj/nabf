namespace NabfAgentLogic
    module AgentLogic =
        open System
        open Graphing.Graph
        open JSLibrary.IiLang
        open JSLibrary.IiLang.DataContainers
        open AgentTypes
        open DecisionTree
        
        (* handlePercept State -> Percept -> State *)
        let handlePercept state percept =
            match percept with
                | EnemySeen enemy   
                    -> { state with EnemyData = enemy :: state.EnemyData }
                | VertexSeen vertex 
                    -> { state with World = addVertex state.World vertex }
                | EdgeSeen edge          
                    -> { state with World = addEdge state.World edge }
                | SimulationStep step
                    -> { state with SimulationStep = step }
        
        let buildInitState (name ,simData:SimStartData) =
            {   World = Map.empty
            ;   Self =  {   Energy = 0
                        ;   MaxEnergy = 0
                        ;   Health = 0
                        ;   MaxHealth = 0
                        ;   Name = name
                        ;   Node = ""
                        ;   Role = Some (simData.SimRole)
                        ;   Strength = 0
                        ;   TeamName = ""
                        ;   VisionRange = 0
                        }
            ;   EnemyData = List.Empty
            ;   Achievements = Set.empty
            ;   SimulationStep = 0
            ;   NearbyAgents = List.Empty
            ;   LastAction = (Skip,Successful)
            }

        (* let updateState : State -> Percept list -> State *)
        let updateState state percepts = 
            List.fold handlePercept state percepts

        let sharedPercepts (percepts:Percept list) =
            []:(Percept list)
        
        let updateStateWhenGivenJob (state:State) (job:Job) =
            state

        let buildIilAction (action:Action) =
            new IilAction "some action"

        let buildJobAccept (desire:Desirability,job:Job) =
            new IilAction "some action"

        let parseIilPercepts (perceptCollection:IilPerceptCollection) : ServerMessage =
            AgentServerMsg (AcceptedJob 1) 

        let generateJobs  (state:State) (jobs:Job list) = []
        
        //Obsolete
        let generateActions (state:State) = []
        

        let generateDecisionTree : Decision<(State -> (bool*Option<Action>))> = DecisionTree.getTree        

        let generateJob (jt:JobType) (s:State) (knownJobs:Job list)  =
            option<Job>.None

        let buildJob (job:Job) = 
            new IilAction "some action"
        let decideJob (state:State) (job:Job) =
            let d:Desirability = 1
            (d,true)

        let buildEvaluationStarted =
            new IilAction "evaluation_started"
        let buildEvaluationEnded =
            new IilAction "evaluation_ended"

        let buildSharePerceptsAction (percepts:Percept list) =
            new IilAction "percepts"