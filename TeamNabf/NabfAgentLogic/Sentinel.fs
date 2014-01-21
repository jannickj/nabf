namespace NabfAgentLogic

module Sentinel =

    open AgentTypes
    open AgentLogicLib

    let getSentinelTree : Decision<(State -> (bool*Option<Action>))> =
        Options 
            [
                
            ]