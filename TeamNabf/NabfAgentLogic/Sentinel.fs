namespace NabfAgentLogic

module Sentinel =

    open AgentTypes

    let getSentinelTree : Decision<(State -> (bool*Option<Action>))> =
        Options 
            [
                
            ]