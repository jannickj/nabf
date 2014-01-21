namespace NabfAgentLogic

module Inspector =

    open AgentTypes
    open AgentLogicLib

    let getInspectorTree : Decision<(State -> (bool*Option<Action>))> =
        Options 
            [
                
            ]