namespace NabfAgentLogic
    module AgentLogic =
        open System
        open Graphing.Graph
        open JSLibrary.IiLang
        open JSLibrary.IiLang.DataContainers
        open AgentTypes
        open DecisionTree
        open IiLang.IiLangDefinitions
        open IiLang.IilTranslator
        
        (* handlePercept State -> Percept -> State *)
        let handlePercept state percept =
            match percept with
                | EnemySeen enemy   
                    -> { state with EnemyData = enemy :: state.EnemyData }
                | VertexSeen (id, team) ->
                    let ownedVertices = 
                        match team with
                        | Some t -> Map.add id t state.OwnedVertices
                        | None -> state.OwnedVertices

                    if not <| Map.containsKey id state.World then
                        { state with
                            World = Map.add id { Identifier = id; Value = None; Edges = Set.empty } state.World
                            NewVertices = (id, team) :: state.NewVertices
                            OwnedVertices = ownedVertices
                        }
                    else 
                        { state with OwnedVertices = ownedVertices }
                | VertexProbed (name, value) ->
                    { state with World = addVertexValue state.World {state.World.[name] with Value = Some value} }
                | EdgeSeen (cost, node1, node2) when (not <| Set.contains (cost, node2) state.World.[node1].Edges) ->
                    { state with 
                        World = addEdge state.World (cost, node1, node2) 
                        NewEdges = (cost, node1, node2) :: state.NewEdges
                    }
                | Team team ->
                    { state with 
                        TeamZoneScore = team.ZoneScore
                        Achievements = team.Achievements @ state.Achievements
                        Money = team.Money
                        LastStepScore = team.LastStepScore
                        Score = team.Score
                    }
                | SimulationStep step  -> { state with SimulationStep = step }
                | ZoneScore score      -> { state with ThisZoneScore = score }
                | Self self            -> { state with Self = self }
                | LastAction action    -> { state with LastAction = action }
                | LastActionResult res -> { state with LastActionResult = res }
                | _ -> state
        
        let buildInitState (name ,simData:SimStartData) =
            {   World = Map.empty
            ;   Self =  {   Energy = Some 0
                        ;   MaxEnergy = Some 0
                        ;   Health = Some 0
                        ;   MaxHealth = Some 0
                        ;   Name = name
                        ;   Node = ""
                        ;   Role = Some (simData.SimRole)
                        ;   Strength = Some 0
                        ;   Team = ""
                        ;   Status = Normal
                        ;   VisionRange = Some 0
                        }
            ;   EnemyData = List.Empty
            ;   SimulationStep = 0
            ;   NearbyAgents = List.Empty
            ;   OwnedVertices = Map.empty
            ;   NewVertices = []
            ;   NewEdges = []
            ;   LastStepScore = 0
            ;   LastAction = Skip
            ;   LastActionResult = Successful
            ;   Money = 0
            ;   Score = 0
            ;   TeamZoneScore = 0
            ;   ThisZoneScore = 0
            ;   Achievements = []
            } : State

        let updateTraversedEdgeCost (oldState : State) (newState : State) =
            match (oldState.Self.Node, newState.LastAction, newState.LastActionResult) with
            | (fromVertex, Goto toVertex, Successful) -> 
                let edge = (Some (oldState.Self.Energy.Value - newState.Self.Energy.Value), fromVertex, toVertex)
                { newState with 
                    World = addEdgeCost newState.World edge
                    NewEdges = edge :: newState.NewEdges 
                }
            | _ -> newState
           
        (* let updateState : State -> Percept list -> State *)
        let updateState state percepts = 
            let state = { state with 
                            NewEdges = []
                            NewVertices = []
                            NearbyAgents = [] 
                        }

            let newState = List.fold handlePercept state percepts
            updateTraversedEdgeCost state newState

        let sharedPercepts (percepts:Percept list) =
            []:(Percept list)
        
        let updateStateWhenGivenJob (state:State) (job:Job) =
            state

        let buildIilAction id (action:Action) =
            IiLang.IiLangDefinitions.buildIilAction (IiLang.IilTranslator.buildIilAction action id)

        let buildJobAccept (desire:Desirability,job:Job) =
            new IilAction "some action"

        let parseIilPercepts (perceptCollection:IilPerceptCollection) : ServerMessage =
            let percepts = parsePerceptCollection perceptCollection
            parseIilServerMessage percepts

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