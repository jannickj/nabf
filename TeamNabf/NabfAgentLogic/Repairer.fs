namespace NabfAgentLogic

module Repairer =

    open AgentTypes
    open AgentLogicLib

    let getRepairerTree : Decision<(State -> (bool*Option<Action>))> =
        Options 
            [
                
            ]

    let repairerReact (s:State) (agents:Agent list * Agent list) =
        let potentialEnemySabs = List.filter (fun a -> (a.Role = Some Saboteur) || (a.Role = None)) (fst agents)
        runAway s potentialEnemySabs