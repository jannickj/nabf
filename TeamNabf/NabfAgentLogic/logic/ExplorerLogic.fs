namespace NabfAgentLogic

module ExplorerLogic =

    open AgentTypes
    open AgentLogicLib

    let explorerReact (s:State) (agents:Agent list * Agent list) =
        let enemySabs = List.filter (fun a -> (a.Role = Some Saboteur) || (a.Role = None)) (fst agents)
        runAway s enemySabs

    let generateOccupyJobExplorer (s:State) (knownJobs:Job list) =
        match s.NewZone.Value with
        | (g,true) -> None
        | _ -> None

    let probeVertex (s:State) =
        if s.World.[s.Self.Node].Value = None then tryDo (Probe None) s else (false,None)