namespace NabfAgentLogic
module Planning = 
    
    open FsPlanning
    open AgentTypes
    open Graphing
    
    let getMoveActions (s:State) =
        List.map (fun (v:Graph.Vertex) -> Action.Goto v.Identifier) (Graph.getNeighbours s.Self.Node s.World)

    let getRechargeAction gt s =
        if gt s then [Recharge] else []

    let getAttackActions s =
        let enems = List.filter (fun ea -> ea.Team <> s.Self.Team && ea.Node = s.Self.Node ) s.NearbyAgents
        if s.Self.Role = Some Saboteur then 
            List.map (fun ea -> Attack ea.Name ) enems
        else
            []

    let getProbeAction s =
        if s.Self.Role = Some Explorer then
            [Probe <| Some s.Self.Node]
        else
            []

    let getRepairActions s = 
        let friends =  List.filter (fun ea -> ea.Team = s.Self.Team && ea.Node = s.Self.Node && ea.Name <> s.Self.Name ) s.NearbyAgents
        if s.Self.Role = Some Repairer then
            List.map (fun f -> Repair f.Name) friends
        else
            []

    let getActions goalTest s =
        getRechargeAction goalTest s
        @getMoveActions s
        @getAttackActions s
        @getProbeAction s
        @getRepairActions s

        

    let actionResult state a =
        let s = { state with LastAction = a}
        match a with
        | Goto vn -> { s with Self = { s.Self with Node = vn }}
        | Attack agentname -> 
            let attacked, rest = List.partition (fun e -> e.Name = agentname) s.EnemyData 
            let updateAtt = { List.head attacked with Status = Disabled }
            { s with EnemyData = updateAtt::rest }
        | Recharge -> 
            let curE = match s.Self.Energy with Some e -> e | _ -> 0
            let maxE = match s.Self.MaxEnergy with Some e -> e | _ -> 20
            { s with Self = { s.Self with Energy = Some (curE + maxE/2)}}
        | Repair a -> 
            let ra, rest = List.partition (fun e -> e.Name = a) s.NearbyAgents 
            let ua = { List.head ra with Health = ra.MaxHealth; Status = Normal }
            { s with NearbyAgents = ua::rest }
        | skip -> s
        | Buy _ -> s
        | Inspect agentname -> 
            let inspectedAgent, rest = List.partition (fun nearbyAgent -> nearbyAgent.Name = agentname) s.NearbyAgents
            let UpdateInspectedAgent = { List.head inspectedAgent with 
        | Parry _ -> s
        | Probe _ -> s
        | _ -> failwith "fail"

    let stepcost s a = 1

    let solve goalTest initState =
        let (problem:Searching.Problem<_,_>) = { InitialState = initState; GoalTest = goalTest; Actions = getActions goalTest; Result = actionResult; StepCost = stepcost}
        Searching.solve Searching.aStar problem