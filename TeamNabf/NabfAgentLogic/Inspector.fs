namespace NabfAgentLogic

module Inspector =

    open AgentTypes

    let getInspectorTree : Decision<(State -> (bool*Option<Action>))> =
        Options 
            [
                
            ]