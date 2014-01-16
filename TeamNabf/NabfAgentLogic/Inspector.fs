namespace NabfAgentLogic

module Inspector =

    open AgentTypes

    let rec getInspectActions (agents : Agent list) (state:State) =
        match agents with
        | [] -> []
        | head :: tail -> 
            if (not (head.Team = state.Self.Team)) && (head.Node = state.Self.Node)//Only range 0 for now
            then
                List.append [Inspect(head)] (getInspectActions tail state)
            else 
                getInspectActions tail state

    let getInspectorActions (state:State) = getInspectActions state.NearbyAgents state