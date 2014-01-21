namespace NabfAgentLogic
module CommonLogic =

    open AgentTypes
    open Graphing.Graph
    open PathFinding
    open Saboteur
    open Repairer
    open Sentinel
    open Explorer
    open Inspector
    open AgentLogicLib

    let reactToEnemyAgent (s:State) =
        let agents = List.partition (fun a -> a.Node = s.Self.Node && a.Team <> s.Self.Team) s.NearbyAgents
        if not (fst agents).IsEmpty then
            match s.Self.Role.Value with
            | Saboteur -> saboteurReact s agents
            | Repairer -> repairerReact s agents
            | Sentinel -> tryDo Parry s
            | Explorer -> explorerReact s agents
            | Inspector -> inspectorReact s agents
        else
            (false,None)

    let exploreLocalGraph (s:State) =
        let unexplored = (pathToNearestUnExplored s.Self s.World)
        if unexplored = None 
        then 
            (false,None) 
        else
            tryGo s.World.[unexplored.Value.Head] s
            
    let idle (s:State) = recharge

