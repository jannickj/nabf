namespace NabfAgentLogic

module SentinelLogic =

    open AgentTypes
    open AgentLogicLib
    open PathFinding

    let sentinelReact (s:State) (agents:Agent list * Agent list) =
        let potentialEnemySabs = List.filter (fun a -> (a.Role = Some Saboteur || a.Role = None)) (fst agents)
        if not potentialEnemySabs.IsEmpty then tryDo Parry s else (false,None)

    let rec findOccupyGoal (g:Goal list) =
        match g with
        | head :: tail -> 
            match head with
            | OccupyGoal(v) -> Some v
            | _ -> findOccupyGoal tail
        | [] -> None

    let workOnOccupyGoal (s:State) =
        match (findOccupyGoal s.Goals) with
        | Some v ->
            let goal = pathTo s.Self v s.World
            match goal with
            | Some vl -> tryGo s.World.[vl.Head] s
            | None -> (false,None)
        | None -> (false,None)

    