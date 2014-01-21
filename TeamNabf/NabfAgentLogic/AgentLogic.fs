namespace NabfAgentLogic
    module AgentLogic =
        open System
        open Graphing.Graph
        open JSLibrary.IiLang
        open JSLibrary.IiLang.DataContainers
        open AgentTypes
        open DecisionTree
        open ExplorerLogic
        
        (* handlePercept State -> Percept -> State *)
        let handlePercept state percept =
            match percept with
                | EnemySeen enemy   
                    -> { state with EnemyData = enemy :: state.EnemyData }
                | VertexSeen (id, team) when not <| Map.containsKey id state.World ->
                    { state with
                        World = Map.add id { Identifier = id; Value = None; Edges = Set.empty } state.World;
                        NewVertices = (id, team) :: state.NewVertices
                    }
                | SimulationStep step
                    -> { state with SimulationStep = step }
        
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
            ;   ThisZoneScore = 0
            ;   TeamZoneScore = 0
            ;   Achievements = []
            ;   NewZone = None
            ;   Goals = []
            } : State

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

        let generateOccupyJob (s:State) (knownJobs:Job list) =
            match s.Self.Role with
            | Some Explorer -> generateOccupyJobExplorer s knownJobs
            | _ -> None

        let rec tryFindRepairJob (s:State) (knownJobs:Job list) =
            match knownJobs with
            | (_ , rdata) :: tail -> if rdata = RepairJob(s.Self.Node,s.Self.Name) then Some knownJobs.Head else tryFindRepairJob s tail
            | [] -> None

        let generateRepairJob (s:State) (knownJobs:Job list) =
            if s.Self.Health.Value = 0 
            then
                let j = tryFindRepairJob s knownJobs
                match j with
                | None -> Some ((-1,5,JobType.RepairJob),RepairJob(s.Self.Node,s.Self.Name))
                | Some ((id,d,_),_) -> Some ((id,d,JobType.RepairJob),RepairJob(s.Self.Node,s.Self.Name))
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
                Some ((-1,-1,JobType.AttackJob),AttackJob [s.Self.Node])
            else 
                None

        let generateJob (jt:JobType) (s:State) (knownJobs:Job list) : option<Job> =
            match jt with
            | JobType.OccupyJob     -> generateOccupyJob s knownJobs
            | JobType.RepairJob     -> generateRepairJob s knownJobs
            | JobType.DisruptJob    -> generateDisruptJob s knownJobs
            | JobType.AttackJob     -> generateAttackJob s knownJobs
            | _                     -> failwithf "Wrong JobType parameter passed to generateJob"

        let buildJob (job:Job) = 
            new IilAction "some action"

        let decideJob (state:State) (job:Job) : Desirability * bool =
            if state.Goals.IsEmpty 
            then
                match job with
                | ((_,_,JobType.RepairJob),_) -> if state.Self.Role.Value = Repairer then (10,true) else (0,false)
                | ((_,_,JobType.AttackJob),_) -> if state.Self.Role.Value = Saboteur then (10,true) else (0,false)
                | ((_,_,JobType.DisruptJob),_) -> if state.Self.Role.Value = Sentinel then (10,true) else (0,false)
                | ((_,_,JobType.OccupyJob),_) -> (8,true)
                | _                           -> (0,false)
            else
                (0,false)

        let buildEvaluationStarted =
            new IilAction "evaluation_started"
        let buildEvaluationEnded =
            new IilAction "evaluation_ended"

        let buildSharePerceptsAction (percepts:Percept list) =
            new IilAction "percepts"