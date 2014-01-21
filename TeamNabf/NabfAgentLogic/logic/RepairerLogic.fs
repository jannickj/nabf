namespace NabfAgentLogic

module RepairerLogic =

    open AgentTypes
    open AgentLogicLib

    let repairerReact (s:State) (agents:Agent list * Agent list) =
        let potentialEnemySabs = List.filter (fun a -> (a.Role = Some Saboteur) || (a.Role = None)) (fst agents)
        runAway s potentialEnemySabs

    let repairAgent (s:State) =
        let allies = List.filter (fun a -> a.Team = s.Self.Team && a.Node = s.Self.Node) s.NearbyAgents
        let injuredAllies = List.sortBy (fun a -> a.Health) allies
        if allies.Head.Health < allies.Head.MaxHealth 
        then 
            tryDo (Repair allies.Head.Name) s
        else 
            (false,None)