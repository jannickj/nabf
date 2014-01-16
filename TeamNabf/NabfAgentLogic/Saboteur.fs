namespace NabfAgentLogic

module Saboteur =

    open AgentTypes

    let getSaboteurTree : Decision<(State -> (bool*Option<Action>))> =
        Options 
            [
                
            ]

    let saboteurRank (allies:Agent list) (self:Agent) =
        let order = List.sort (self :: allies)
        (List.findIndex (fun a -> a = self) order) + 1

    let rec getTarget (l : Agent list list) (rank : int) =
        match l with
        | head :: tail -> if head.Length >= rank then Some head.[rank-1] else getTarget tail (rank-head.Length)
        | [] -> None

    let saboteurReact (s:State) (agents:Agent list * Agent list) =
        let allySabs = List.filter (fun a -> a.Role = Some Saboteur) (snd agents)
        let myRank = saboteurRank allySabs s.Self

        let enemySabs = List.filter (fun a -> a.Role = Some Saboteur) (fst agents)
        let repairers = List.filter (fun a -> a.Role = Some Repairer) (fst agents)
        let unknowns = List.filter (fun a -> a.Role = None) (fst agents)
        let explorers = List.filter (fun a -> a.Role = Some Explorer) (fst agents)
        let inspectors = List.filter (fun a -> a.Role = Some Inspector) (fst agents)
        let sentinels = List.filter (fun a -> a.Role = Some Sentinel) (fst agents)

        let target = getTarget [enemySabs;repairers;unknowns;explorers;inspectors;sentinels] myRank
        if target = None then (false,None) else (true,Some(Attack(target.Value)))



    