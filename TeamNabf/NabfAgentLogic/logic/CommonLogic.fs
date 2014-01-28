namespace NabfAgentLogic
module CommonLogic =

    open AgentTypes
    open Graphing.Graph
    open PathFinding
    open SaboteurLogic
    open RepairerLogic
    open SentinelLogic
    open ExplorerLogic
    open InspectorLogic
    open AgentLogicLib

    let reactToEnemyAgent (s:State) =
        
        let agents = List.partition (fun a -> (a.Node = s.Self.Node) && (a.Team <> s.Self.Team)) s.NearbyAgents
        if not (fst agents).IsEmpty then
            match s.Self.Role.Value with
            | Saboteur -> saboteurReact s agents
            | Repairer -> repairerReact s agents
            | Sentinel -> sentinelReact s agents
            | Explorer -> explorerReact s agents
            | Inspector -> inspectorReact s agents
        else
            (false,None)

    let exploreLocalGraph (s:State) =
        let unexplored = (pathToNearestUnExplored s.Self s.World)
      
//        ignore <| match unexplored with
//                    | Some (head :: _) -> printfn "Edges from first unexplored node: %A\n" s.World.[head]
//                    | _ -> ()

        if unexplored = None 
        then 
            (false,None) 
        else            
           
            tryGo s.World.[unexplored.Value.Head] s
            
    let idle (s:State) = recharge

