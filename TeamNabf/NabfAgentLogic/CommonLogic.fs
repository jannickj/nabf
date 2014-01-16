namespace NabfAgentLogic
module CommonLogic =

    open AgentTypes
    open Graphing.Graph

    let advancedGoto (v:Vertex) (s:State) =
        let edges = Set.toList s.World.[s.Self.Node].Edges
        let edge = List.find (fun (_,id) -> id = v.Identifier) edges
        if (fst edge) = None 
        then 
            if s.Self.Energy < 10 then (true,Some(Recharge)) else (true,Some(Goto(v)))
        else
            let value = (fst edge).Value
            if value > s.Self.Energy then (true,Some(Recharge)) else (true,Some(Goto(v)))

    let reactToEnemyAgent (s:State) =
        let agents = List.filter (fun a -> a.Node = s.Self.Node && a.Team <> s.Self.Team && (a.Role = Some Saboteur || a.Role = None)) s.NearbyAgents
        if not agents.IsEmpty then
            match s.Self.Role.Value with
            | Saboteur -> (true,Some(Attack(agents.Head)))
            | Repairer
            | Sentinel -> (true,Some(Parry))
            | Explorer -> (true,Some(Goto((getNeighbours s.Self.Node s.World).Head)))
            | Inspector -> (true,Some(Recharge))
        else
            (false,None)


    let exploreLocalGraph (s:State) =
        let neighbours = getNeighbours s.Self.Node s.World
        let unexplored = List.tryFind (fun (v:Vertex) -> v.Edges.Count > 1) neighbours
        if unexplored = None 
        then 
            (false,None) 
        else
            advancedGoto unexplored.Value s
            
    let idle (s:State) = (true,Some(Recharge))

