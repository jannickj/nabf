﻿namespace NabfAgentLogic

module RepairerLogic =

    open AgentTypes
    open AgentLogicLib
    open PathFinding

    let repairerReact (s:State) (agents:Agent list * Agent list) =
        let potentialEnemySabs = List.filter (fun a -> (a.Role = Some Saboteur) || (a.Role = None)) (fst agents)
        runAway s potentialEnemySabs

    let repairNearbyAgent (s:State) =
        let allies = List.filter (fun a -> a.Team = s.Self.Team && a.Node = s.Self.Node) s.NearbyAgents
        let injuredAllies = List.sortBy (fun a -> a.Health) allies
        if allies.Length > 0 && allies.Head.Health < allies.Head.MaxHealth 
        then 
            tryDo (Repair allies.Head.Name) s
        else 
            (false,None)

    let rec findRepairGoal (g:Goal list) =
        match g with
        | head :: tail -> 
            match head with
            | RepairGoal(v,a) -> Some (v,a)
            | _ -> findRepairGoal tail
        | [] -> None

    let workOnRepairGoal (s:State) =
        match (findRepairGoal s.Goals) with
        | Some (v,a) ->
            if v = s.Self.Node 
            then 
                tryDo (Repair a) s 
            else
                let goal = pathTo s.Self v s.World
                match goal with
                | Some vl -> tryGo s.World.[vl.Head] s
                | None -> (false,None)
        | None -> (false,None)