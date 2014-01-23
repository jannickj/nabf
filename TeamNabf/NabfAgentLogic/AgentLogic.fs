namespace NabfAgentLogic
    module AgentLogic =
        open System
        open Graphing.Graph
        open JSLibrary.IiLang
        open JSLibrary.IiLang.DataContainers
        open AgentTypes
        open DecisionTree
        open ExplorerLogic
        open IiLang.IiLangDefinitions
        open IiLang.IilTranslator
        
        (* handlePercept State -> Percept -> State *)
        let handlePercept state percept =
            match percept with
                | EnemySeen enemy   
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
                    World = addEdgeCost newState.World edge
                    NewEdges = edge :: newState.NewEdges 
                }
            | _ -> newState

        let updateSelf (oldState : State) (newState : State) =
            { newState with
                Self = { newState.Self with
                           Team = oldState.Self.Team
                           Name = oldState.Self.Name
                           Role = oldState.Self.Role
                       }
            }
           
        (* let updateState : State -> Percept list -> State *)
        let updateState state percepts = 
            let newState = clearTempBeliefs state
            List.fold handlePercept newState percepts
            |> updateTraversedEdgeCost state
            |> updateSelf state

            let newState = List.fold handlePercept state percepts
            let newSelf = { newState.Self with 
                              Team = state.Self.Team
                              Name = state.Self.Name
                              Role = state.Self.Role
                          }
            let newState = { newState with  Self = newSelf }

            match (state.LastAction, state.LastActionResult) with
            | (Goto _, Successful) ->
                printfn "coming from %s" state.Self.Node
                printfn "edges before: %A" (Set.toList <| newState.World.[newState.Self.Node].Edges)
                let newNewState = updateTraversedEdgeCost state newState
                printfn "edges after: %A" (Set.toList <| newNewState.World.[newState.Self.Node].Edges)
                newNewState
            | _ -> updateTraversedEdgeCost state newState

    
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
        
        let updateStateWhenGivenJob (state:State) (job:Job) (moveTo:VertexName) =
            state

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
            | _ -> None

        let rec tryFindRepairJob (s:State) (knownJobs:Job list) =
            match knownJobs with
            | (_ , rdata) :: tail -> if rdata = RepairJob(s.Self.Node,s.Self.Name) then Some knownJobs.Head else tryFindRepairJob s tail
            | [] -> None

        let generateRepairJob (s:State) (knownJobs:Job list) : Option<Job> =
            if s.Self.Health.Value = 0 
            then
                let j = tryFindRepairJob s knownJobs
                match j with
                | None -> Some ((None,5,JobType.RepairJob,1),RepairJob(s.Self.Node,s.Self.Name))
                | Some ((id,d,_,an),_) -> Some ((id,d,JobType.RepairJob,an),RepairJob(s.Self.Node,s.Self.Name))
            else
                None

        let generateDisruptJob (s:State) (knownJobs:Job list) = None

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
                Some ((None,-1,JobType.AttackJob,1),AttackJob [s.Self.Node])
            else 
                None

        let generateJob (jt:JobType) (s:State) (knownJobs:Job list) : option<Job> =
            match jt with
            | JobType.OccupyJob     -> generateOccupyJob s knownJobs
            | JobType.RepairJob     -> generateRepairJob s knownJobs
            | JobType.DisruptJob    -> generateDisruptJob s knownJobs
            | JobType.AttackJob     -> generateAttackJob s knownJobs
            | _                     -> failwithf "Wrong JobType parameter passed to generateJob"

        let decideJob (state:State) (job:Job) : Desirability * bool =
            if state.Goals.IsEmpty 
            then
                match job with
                | ((_,_,JobType.RepairJob,_),_) -> if state.Self.Role.Value = Repairer then (10,true) else (0,false)
                | ((_,_,JobType.AttackJob,_),_) -> if state.Self.Role.Value = Saboteur then (10,true) else (0,false)
                | ((_,_,JobType.DisruptJob,_),_) -> if state.Self.Role.Value = Sentinel then (10,true) else (0,false)
                | ((_,_,JobType.OccupyJob,_),_) -> (8,true)
                | _                           -> (0,false)
            else
                (0,false)


        let buildIilSendMessage ((id,act):SendMessage) =
            IiLang.IiLangDefinitions.buildIilAction (IiLang.IilTranslator.buildIilMetaAction act id)