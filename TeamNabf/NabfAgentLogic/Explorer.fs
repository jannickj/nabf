namespace NabfAgentLogic

module Explorer =

    open AgentTypes

    let getExplorerActions (state:State) = [Probe(None)]