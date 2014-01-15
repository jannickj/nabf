namespace NabfAgentLogic

module Repairer =

    open AgentTypes

    let rec getRepairActions (agents : Agent list) (state:State) =
        match agents with
        | [] -> []
        | head :: tail -> 
            if (head.Team = state.Self.Team) //Only allies
                && (head.Position = state.Self.Position) //Only range 0 for now
                && (not (head.Health = head.MaxHealth)) //Only damaged agents
            then
                List.append [Repair(head)] (getRepairActions tail state)
            else 
                getRepairActions tail state

    let getRepairerActions (state:State) = 
        List.append [Parry;Buy(SabotageDevice)] (getRepairActions state.NearbyAgents state)