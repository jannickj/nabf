namespace NabfAgentLogic

module InspectorLogic =

    open AgentTypes
    open AgentLogicLib

    let inspectorReact (s:State) (agents:Agent list * Agent list) =
        let potentialEnemySabs = List.filter (fun a -> (a.Role = Some Saboteur)) (fst agents)
        runAway s potentialEnemySabs

    let inspect (s:State) =
        let agents = List.filter (fun a -> a.Node = s.Self.Node && a.Team <> s.Self.Team) s.NearbyAgents
        let unknownEnemies = List.filter (fun a -> (a.Role = None)) agents
        if not unknownEnemies.IsEmpty then tryDo (Inspect None) s else (false,None)