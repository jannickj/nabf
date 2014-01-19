namespace NabfAgentLogic

module Inspector =

    open AgentTypes
    open AgentLogicLib

    let getInspectorTree : Decision<(State -> (bool*Option<Action>))> =
        Options 
            [
                
            ]

    let inspectorReact (s:State) (agents:Agent list * Agent list) =
        let potentialEnemySabs = List.filter (fun a -> (a.Role = Some Saboteur) || (a.Role = None)) (fst agents)
        runAway s potentialEnemySabs