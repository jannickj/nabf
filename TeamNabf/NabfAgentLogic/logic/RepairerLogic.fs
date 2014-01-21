namespace NabfAgentLogic

module RepairerLogic =

    open AgentTypes
    open AgentLogicLib

    let repairerReact (s:State) (agents:Agent list * Agent list) =
        let potentialEnemySabs = List.filter (fun a -> (a.Role = Some Saboteur) || (a.Role = None)) (fst agents)
        runAway s potentialEnemySabs