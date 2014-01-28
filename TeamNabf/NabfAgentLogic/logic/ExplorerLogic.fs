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

    type ZoneVertex = 
        {   
            Vertex          : Vertex
            ControlValue    : int
            Lock            : bool
            HasAgent        : bool
            Desire          : int
        }

    let buildZoneVertex (vertex:Vertex) =
        {
            Vertex = vertex
            ControlValue = 2 + vertex.Edges.Count
            Desire = 0
            Lock = false
            HasAgent = true
        } : ZoneVertex

    let isIsland (vertex:Vertex) =
        vertex.Edges.Count = 1

    let shouldZoneVertexLockBasedOnIsland (graph:Graph) (zoneV:ZoneVertex) =
        let vl = getNeighbours zoneV.Vertex.Identifier graph
        List.exists isIsland vl
    
    

    let getZoneVertexNeighbour (graph:Graph) (zl:ZoneVertex list) (zoneV:ZoneVertex) =
        let vl = getNeighbours zoneV.Vertex.Identifier graph
        List.filter (fun zv -> (List.exists (fun v -> v.Identifier = zv.Vertex.Identifier) vl)) zl
    
    let shouldZoneVertexLock (graph:Graph) (zl:ZoneVertex list) (zoneV:ZoneVertex) =
        if ((zoneV.ControlValue <= 3 && not (isIsland zoneV.Vertex)) || zoneV.Lock) && (zoneV.HasAgent) then true
        else
            let zneighbours = getZoneVertexNeighbour graph zl zoneV
            List.exists (fun zn -> zn.ControlValue = 2) zneighbours

    let calcControlValue (graph:Graph) (zl:ZoneVertex list) (zoneV:ZoneVertex) =
        let init = if zoneV.HasAgent then 2 else 0
        let vl = getNeighbours zoneV.Vertex.Identifier graph
        let zbours = getZoneVertexNeighbour graph zl zoneV 
        init + List.length (List.filter (fun zn -> zn.HasAgent) zbours)
    
    let calcDesire (graph:Graph) (zl:ZoneVertex list) (zoneV:ZoneVertex) =       
        let zneighbours = getZoneVertexNeighbour graph zl zoneV
        //IF Even -1; IF Uneven +1
        let calcSingleDesire cval = (((cval % 2) * 2) - 1)
        List.sum (List.map (fun zn -> calcSingleDesire zn.ControlValue ) zneighbours)
    
    let hasLockedNeighbour (graph:Graph) (zl:ZoneVertex list) (zoneV:ZoneVertex) =
        let zneighbours = getZoneVertexNeighbour graph zl zoneV
        List.exists (fun zn -> zn.Lock ) zneighbours

    let choseRemove (graph:Graph) (zl:ZoneVertex list)= 
        let sorted = List.rev (List.sortBy (fun zv -> zv.Desire) zl)
        let allLocked = List.forall (fun zv -> zv.Lock) zl
        if allLocked || List.isEmpty sorted then
            None
        else
            let findNLocked = List.tryFind ( fun zn -> (hasLockedNeighbour graph zl zn) && (not zn.Lock) && (zn.HasAgent)) sorted
            if findNLocked.IsSome then
                findNLocked
            else
                List.tryFind (fun zn -> (not zn.Lock) && (zn.HasAgent) ) sorted
                
    
    let rec calcAgentPositions (graph:Graph) (zl:ZoneVertex list) =
        let zl1 = List.map (fun zn -> {zn with ControlValue = calcControlValue graph zl zn}) zl
        let zl2 = List.map (fun zn -> {zn with Lock = (shouldZoneVertexLock graph zl1 zn)}) zl1
        let zl3 = List.map (fun zn -> {zn with Desire = calcDesire graph zl2 zn}) zl2
        let rz = choseRemove graph zl3
        match rz with
        | None -> zl3
        | Some removed -> 
            let zl4 =    {   removed with HasAgent = false}
                        ::  (List.filter (fun zn -> zn.Vertex.Identifier <> removed.Vertex.Identifier ) zl3)
            calcAgentPositions graph zl4

    let findAgentPlacement (subgraph:Graph) =
        let vl = snd (List.unzip (Map.toList subgraph))
        let zl = List.map buildZoneVertex vl
        let zll = List.map (fun zn -> {zn with Lock = (shouldZoneVertexLockBasedOnIsland subgraph zn); HasAgent = not (isIsland zn.Vertex)}) zl
        let zl = calcAgentPositions subgraph zll
        let zl = List.filter (fun zn -> zn.HasAgent ) zl
        List.map (fun zn -> zn.Vertex.Identifier) zl

    ////////////////////
    // Main functions //
    ////////////////////
    let explorerReact (s:State) (agents:Agent list * Agent list) =
        let enemySabs = List.filter (fun a -> (a.Role = Some Saboteur) || (a.Role = None)) (fst agents)
        runAway s enemySabs

    let generateOccupyJobExplorer (s:State) (knownJobs:Job list) : Option<Job> =
        match s.NewZone with
        | Some (g,true) -> 
            let overlapping = getOverlappingJobs s (List.filter (fun ((_,_,jType,_),_) -> jType = JobType.OccupyJob) knownJobs)
            
            if overlapping = []
            then
                let completeZone = List.map (fun v -> v.Identifier) (snd (List.unzip (Map.toList (fst s.NewZone.Value))))
                let zonepoints = findAgentPlacement (fst s.NewZone.Value)
                Some ((None,10(*Remember to add zone value*),JobType.OccupyJob,(List.length zonepoints)),JobData.OccupyJob(zonepoints,completeZone))
            else
                None // Join w. overlapping jobs and return the joined job
        | _ -> None

    let probeVertex (s:State) =
        if s.World.[s.Self.Node].Value.IsNone then tryDo (Probe None) s else (false,None)

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


   
