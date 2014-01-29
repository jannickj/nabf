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
        open Logging
        open ExplorerLogic
        open SaboteurLogic
        open RepairerLogic
        open InspectorLogic
        open RepairerLogic
        open SentinelLogic

        let OurTeam = "Nabf"
        
        (* handlePercept State -> Percept -> State *)
        let handlePercept state percept =
            match percept with
                | EnemySeen enemy when enemy.Name <> state.Self.Name
                    -> { state with 
                           EnemyData = enemy :: state.EnemyData
                           NearbyAgents = enemy :: state.NearbyAgents 
                       }
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
                    { state with 
                            World = addVertexValue name value state.World
                    }
                        
                | EdgeSeen (cost, node1, node2) ->
                    let edgeAlreadyExists = fun (cost':Option<_>, otherVertexId) -> cost'.IsSome && otherVertexId = node2

                    let containNode = (Map.containsKey node1 state.World)
                    //let edges = state.World.[node1].Edges
                    //logInfo ("Contains Node: "+containNode.ToString())
                    if ( not (containNode && (Set.exists edgeAlreadyExists state.World.[node1].Edges))) then
                        { state with 
                            World = addEdge (cost, node1, node2) state.World 
                            NewEdges = (cost, node1, node2) :: state.NewEdges
                        }
                    else
                        state

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
                | Self self ->
                    let newSelf = { self with 
                        Name = state.Self.Name
                        Team = state.Self.Team
                        Role = state.Self.Role
                    }
                    { state with Self = newSelf }
                | LastAction action    -> { state with LastAction = action }
                | LastActionResult res -> { state with LastActionResult = res }
                | _ -> state
        
        let buildInitState (name, simData:SimStartData) =
            {   World = Map.empty
            ;   Self =  {   Energy = Some 0
                        ;   MaxEnergy = Some 0
                        ;   Health = Some 0
                        ;   MaxHealth = Some 0
                        ;   Name = name
                        ;   Node = ""
                        ;   Role = Some (simData.SimRole)
                        ;   Strength = Some 0
                        ;   Team = OurTeam
                        ;   Status = Normal
                        ;   VisionRange = Some 0
                        }
            ;   EnemyData = List.Empty
            ;   SimulationStep = 0
            ;   NearbyAgents = List.Empty
            ;   OwnedVertices = Map.empty
            ;   LastPosition = ""
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
            ;   NewZone = None
            ;   Goals = []
            ;   Jobs = []
            ;   NewZoneFrontier = []
            } : State

        let shouldRemoveJob (state:State) (job:Job) =
            false

        let clearTempBeliefs state =
            { state with 
                NewEdges = []
                NewVertices = []
                NearbyAgents = [] 
            }

        let updateTraversedEdgeCost (oldState : State) (newState : State) =
            match (oldState.Self.Node, newState.LastAction, newState.LastActionResult) with
            | (fromVertex, Goto toVertex, Successful) -> 
                let edge = (Some (oldState.Self.Energy.Value - newState.Self.Energy.Value), fromVertex, toVertex)
                { newState with 
                    World = addEdge edge newState.World
                    NewEdges = edge :: newState.NewEdges 
                }
            | _ -> newState

        let updateEdgeCosts (lastState:State) (state:State) =
            match (state.LastAction, state.LastActionResult) with
            | (Goto _, Successful) ->
                let state4 = updateTraversedEdgeCost lastState state
                state4
            | _ -> updateTraversedEdgeCost lastState state
        
        let updateLastPos (lastState:State) (state:State) =
            { state with LastPosition = lastState.Self.Node }

        (* let updateState : State -> Percept list -> State *)
        let updateState state percepts = 
            let clearedState = clearTempBeliefs state

            let updatedState = 
                List.fold handlePercept clearedState percepts
                |> updateEdgeCosts state
                |> updateLastPos state

            match updatedState.Self.Role.Value with
            | Explorer -> updateStateExplorer updatedState
            | _ -> updatedState

            
    
        let shouldSharePercept (state:State) percept =
            match percept with
            | VertexProbed (vp,d) -> 
                if state.World.ContainsKey(vp) then
                    let v = state.World.Item vp
                    if v.Value.IsNone then 
                        true
                    else 
                        false
                else 
                    false
            | EdgeSeen es -> true
            | VertexSeen (vp,t) -> 
                not (state.World.ContainsKey(vp))                
            | EnemySeen { Name = name; Role = Some _ } ->
                not (List.exists (fun { Role = Some _; Name = an } -> an = name) (state.EnemyData))
            | _ -> false

        let selectSharedPercepts state (percepts:Percept list) =
            List.filter (shouldSharePercept state) percepts
        
        let updateStateWhenGivenJob (state:State) (((_,_,jobType,_),jobdata):Job) (moveTo:VertexName) : State =
            match jobType with
            | JobType.OccupyJob -> if (List.tryFind (fun g -> g = OccupyGoal(moveTo)) state.Goals).IsNone then {state with Goals = OccupyGoal(moveTo)::state.Goals} else state
            | JobType.AttackJob -> if (List.tryFind (fun g -> g = AttackGoal(moveTo)) state.Goals).IsNone then {state with Goals = AttackGoal(moveTo)::state.Goals} else state
            | JobType.RepairJob -> match jobdata with 
                                   | RepairJob(vName,aName) ->
                                       if (List.tryFind (fun g -> g = RepairGoal(moveTo,aName)) state.Goals).IsSome then state
                                       elif (List.tryFind (fun g -> match g with 
                                                                            | RepairGoal(_,aName) -> true
                                                                            | _ -> false
                                                                            ) state.Goals).IsSome
                                       then
                                           {state with Goals = RepairGoal(moveTo,aName)::(List.filter (fun g -> match g with
                                                                                                                | RepairGoal(_,aName) -> false
                                                                                                                | _ -> true) state.Goals)}
                                       else
                                           {state with Goals = RepairGoal(moveTo,aName)::state.Goals}

            | JobType.DisruptJob -> if (List.tryFind (fun g -> g = AttackGoal(moveTo)) state.Goals).IsNone then {state with Goals = DisruptGoal(moveTo)::state.Goals} else state
            | _ -> state

        let buildIilAction id (action:Action) =
            IiLang.IiLangDefinitions.buildIilAction (IiLang.IilTranslator.buildIilAction action id)



        let parseIilPercepts (perceptCollection:IilPerceptCollection) : ServerMessage =
            let percepts = parsePerceptCollection perceptCollection
            parseIilServerMessage percepts

        let generateJobs  (state:State) (jobs:Job list) = []
        
       

        let generateDecisionTree : Decision<(State -> (bool*Option<Action>))> = DecisionTree.getTree

        let generateOccupyJob (s:State) (knownJobs:Job list) =
            match s.Self.Role with
            | Some Explorer -> generateOccupyJobExplorer s knownJobs
            | _ -> ([],[])

        let rec tryFindRepairJob (s:State) (knownJobs:Job list) =
            match knownJobs with
            | (_ , rdata) :: tail -> if rdata = RepairJob(s.Self.Node,s.Self.Name) then Some knownJobs.Head else tryFindRepairJob s tail
            | [] -> None

        let generateRepairJob (s:State) (knownJobs:Job list) =
            if s.Self.Health.Value = 0 
            then
                let j = tryFindRepairJob s knownJobs
                match j with
                | None -> ([((None,5,JobType.RepairJob,1),RepairJob(s.Self.Node,s.Self.Name))],[])
                | Some ((id,d,_,an),_) -> ([((id,d,JobType.RepairJob,an),RepairJob(s.Self.Node,s.Self.Name))],[])
            else
                ([],[])

        let generateDisruptJob (s:State) (knownJobs:Job list) = ([],[])

        let rec tryFindOccupyGoal (l:Goal list) =
            match l with
            | AttackGoal(v) :: tail -> Some(AttackGoal(v))
            | head :: tail -> tryFindOccupyGoal tail
            | [] -> None

        let isOccupyingPosition (s:State) =
            let g = tryFindOccupyGoal s.Goals
            match g with
            | Some(AttackGoal(v)) -> if s.Self.Node = v then true else false 
            | _ -> false

        let generateAttackJob (s:State) (knownJobs:Job list) = 
            let attackJobFound = List.exists (fun (_, jobdata) -> 
                    match jobdata with 
                    | AttackJob verts -> List.exists ((=) s.Self.Node) verts
                    | _ -> false ) knownJobs
            if (isOccupyingPosition s) && not attackJobFound 
            then 
                ([((None,-1,JobType.AttackJob,1),AttackJob [s.Self.Node])],[])
            else 
                ([],[])

        let generateJob (jt:JobType) (s:State) (knownJobs:Job list) : Job list * JobID list = 
        //Returns a tuple of jobs to add and IDs of jobs to remove
            match jt with
            | JobType.OccupyJob     -> generateOccupyJob s knownJobs
            | JobType.RepairJob     -> generateRepairJob s knownJobs
            | JobType.DisruptJob    -> generateDisruptJob s knownJobs
            | JobType.AttackJob     -> generateAttackJob s knownJobs
            | _                     -> failwithf "Wrong JobType parameter passed to generateJob"

        let decideJob (state:State) (job:Job) : Desirability * bool =
            if state.Goals.IsEmpty 
            then
                
                match state.Self.Role with
                | Some Explorer -> decideJobExplore state job
                | Some Inspector -> decideJobInspector state job
                | Some Sentinel -> decideJobSentinel state job
                | Some Repairer -> decideJobRepairer state job
                | Some Saboteur -> decideJobSaboteur state job
                | None -> (0,false)

//                match job with
//                | ((_,_,JobType.RepairJob,_),_) -> if state.Self.Role.Value = Repairer then (10,true) else (0,false)
//                | ((_,_,JobType.AttackJob,_),_) -> if state.Self.Role.Value = Saboteur then (10,true) else (0,false)
//                | ((_,_,JobType.DisruptJob,_),_) -> if state.Self.Role.Value = Sentinel then (10,true) else (0,false)
//                | ((_,_,JobType.OccupyJob,_),_) -> match state.Self.Role.Value with
//                                                   | Sentinel 
//                                                   | Inspector -> (10,true)
//                                                   | Repairer -> (5,true)
//                                                   | Explorer -> (3,true)
//                                                   | Saboteur -> (0,false)
//                | _                           -> (0,false)
            else
                (0,false)


        let buildIilSendMessage ((id,act):SendMessage) =
            IiLang.IiLangDefinitions.buildIilAction (IiLang.IilTranslator.buildIilMetaAction act id)