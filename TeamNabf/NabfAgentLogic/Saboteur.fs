namespace NabfAgentLogic

module Saboteur =

    open AgentTypes

    let rec getAttackActions (agents : Agent list) (state:State) =
        match agents with
        | [] -> []
        | head :: tail -> 
            if (not (head.Team = state.Self.Team)) && (head.Position = state.Self.Position)
            then
                List.append [Attack(head)] (getAttackActions tail state)
            else 
                getAttackActions tail state

    let getSaboteurActions (state:State) = 
        List.append [Parry;Buy(SabotageDevice)] (getAttackActions state.NearbyAgents state)
        
        