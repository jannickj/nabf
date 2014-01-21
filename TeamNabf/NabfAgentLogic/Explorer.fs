namespace NabfAgentLogic

module Explorer =

    open AgentTypes
    open AgentLogicLib

    let getExplorerTree : Decision<(State -> (bool*Option<Action>))> =
        Options 
            [
                
            ]

    let explorerReact (s:State) (agents:Agent list * Agent list) =
        let enemySabs = List.filter (fun a -> (a.Role = Some Saboteur) || (a.Role = None)) (fst agents)
        runAway s enemySabs

    let generateOccupyJobExplorer (s:State) (knownJobs:Job list) =
        match s.NewZone.Value with
        | (g,true) -> None
        | _ -> None
            