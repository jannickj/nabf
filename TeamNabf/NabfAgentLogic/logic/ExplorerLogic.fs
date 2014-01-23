namespace NabfAgentLogic

module ExplorerLogic =

    open AgentTypes
    open AgentLogicLib
    open Graphing.Graph
    open PathFinding

    //////////////////////
    // Helper functions //
    //////////////////////
    let rec zoneAlreadyFound (jobs:Job list) (v:string) =
        match jobs with
        | [] -> false
        | (_,OccupyJob (_,coveredVertices)) :: tail -> 
            if List.exists (fun n -> n = v) coveredVertices 
            then 
                true 
            else
                zoneAlreadyFound tail v
        | _ -> failwithf "Incorrect input passed to zoneAlreadyFound"

    let getRelevantNeighbours (s:State) =
        let neighbours = List.map (fun v -> v.Identifier) (getNeighbours s.Self.Node s.World)
        List.filter (fun st -> (List.tryFind (fun (st2:VertexName) -> st2 = st) s.NewZoneFrontier).IsSome) neighbours

    let rec listIntersection (l1:string list) (l2:string list) =
        match l1 with
        | [] -> l2
        | head :: tail -> if (List.tryFind (fun s -> s = head) l2).IsNone then listIntersection tail (head :: l2) else listIntersection tail l2

    let rec getOverlappingVertices (s:State) (v:VertexName list) = 
        match v with
        | [] -> []
        | head :: tail -> if (Map.containsKey head (fst s.NewZone.Value)) 
                          then 
                              List.Cons (head,(getOverlappingVertices s tail)) 
                          else 
                              getOverlappingVertices s tail
        

    let getOverlappingJobs (s:State) (occupyJobs:Job list) =
        List.filter (fun ((_,OccupyJob(_,vertices)):Job) -> (getOverlappingVertices s vertices) <> []) occupyJobs

    ////////////////////
    // Main functions //
    ////////////////////
    let explorerReact (s:State) (agents:Agent list * Agent list) =
        let enemySabs = List.filter (fun a -> (a.Role = Some Saboteur) || (a.Role = None)) (fst agents)
        runAway s enemySabs

    let generateOccupyJobExplorer (s:State) (knownJobs:Job list) : Option<Job> =
        match s.NewZone.Value with
        | (g,true) -> 
            let overlapping = getOverlappingJobs s (List.filter (fun ((_,_,jType,_),_) -> jType = JobType.OccupyJob) knownJobs)
            
            if overlapping = []
            then
                let names = List.map (fun v -> v.Identifier) (snd (List.unzip (Map.toList (fst s.NewZone.Value))))
                Some ((None,0(*put value here*),JobType.OccupyJob,0(*put agents needed here*)),JobData.OccupyJob(([(*Put position list here*)]),names))
            else
                None // Join w. overlapping jobs and return the joined job
        | _ -> None

    let probeVertex (s:State) =
        if s.World.[s.Self.Node].Value = None then tryDo (Probe None) s else (false,None)

    let exploreZone (s:State) = 
        match s.NewZone with
        | Some (z,false) -> if (not s.NewZoneFrontier.IsEmpty) && (s.World.[s.Self.Node].Value.Value > 5) && (Map.tryFind s.Self.Node (fst s.NewZone.Value)).IsNone
                            then
                                let rn = listIntersection s.NewZoneFrontier (getRelevantNeighbours s)
                                let s = {s with NewZone = Some (addVertex (fst s.NewZone.Value) s.World.[s.Self.Node],true)
                                                NewZoneFrontier = rn.Tail}
                                let path = pathTo s.Self rn.Head s.World
                                tryGo s.World.[path.Value.Head] s
                            elif
                                (not s.NewZoneFrontier.IsEmpty)
                            then
                                let path = pathTo s.Self s.NewZoneFrontier.Head s.World
                                tryGo s.World.[path.Value.Head] s
                            else 
                                (false,None)
        | _ -> (false,None)

    let startDiscoveringZone (s:State) =
        if s.NewZone.IsNone && s.World.[s.Self.Node].Value.Value = 10 
            && not (zoneAlreadyFound (List.filter (fun ((_,_,jType,_),_) -> jType = JobType.OccupyJob) s.Jobs) s.Self.Node) 
        then
            let rn = getRelevantNeighbours s
            let s = {s with NewZone = Some ((Map.add s.Self.Node s.World.[s.Self.Node] Map.empty),false) 
                                      NewZoneFrontier = rn.Tail}
            tryGo s.World.[rn.Head] s
        else
            (false,None)