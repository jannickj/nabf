namespace NabfAgentLogic

module SentinelLogic =

    open AgentTypes
    open AgentLogicLib

    let sentinelReact (s:State) (agents:Agent list * Agent list) =
        let potentialEnemySabs = List.filter (fun a -> (a.Role = Some Saboteur || a.Role = None)) (fst agents)
        if not potentialEnemySabs.IsEmpty then tryDo Parry s else (false,None)