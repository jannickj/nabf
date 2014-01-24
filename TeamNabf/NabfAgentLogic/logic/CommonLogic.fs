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
        printfn "Self: %A" s.Self
        let agents = List.partition (fun a -> (a.Node = s.Self.Node) && (a.Team <> s.Self.Team)) s.NearbyAgents
        printfn "enemies at this vertex: %A" agents
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
        printfn "Unexplored nodes: %A\n"  unexplored
        ignore <| match unexplored with
                    | Some (head :: _) -> printfn "Edges from first unexplored node: %A\n" s.World.[head]
                    | _ -> ()

        if unexplored = None 
        then 
            printfn "Unexplored NONE!"
            (false,None) 
        else            
            printfn "TRY GO! to %s \n\t %A" unexplored.Value.Head s.World.[unexplored.Value.Head]
            tryGo s.World.[unexplored.Value.Head] s
            
    let idle (s:State) = recharge

