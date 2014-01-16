namespace NabfAgentLogic

module Saboteur =

    open AgentTypes

    let getSaboteurTree : Decision<(State -> (bool*Option<Action>))> =
        Options 
            [
                
            ]