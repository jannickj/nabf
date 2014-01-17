namespace NabfAgentLogic
module AgentLogicLib =

    open Graph
    open AgentTypes

    let ProbeCost = 1
    let SurveyCost = 1
    let InspectCost = 2
    let AttackCost = 2
    let ParryCost = 2
    let RepairCost = 2
    let RepairCostDisabled = 3
    let BuyCost = 2

    let tryDo (action : Action) (s:State) =
        match action with
        | Probe(o)  -> if s.Self.Energy >= ProbeCost then (true,Some(Probe(o))) else (true,Some(Recharge))
        | Survey    -> if s.Self.Energy >= SurveyCost then (true,Some(Survey)) else (true,Some(Recharge))
        | Inspect(a)-> if s.Self.Energy >= InspectCost then (true,Some(Inspect(a))) else (true,Some(Recharge))
        | Attack(a) -> if s.Self.Energy >= AttackCost then (true,Some(Attack(a))) else (true,Some(Recharge))
        | Parry     -> if s.Self.Energy >= ParryCost then (true,Some(Parry)) else (true,Some(Recharge))
        | Repair(a) -> if ((s.Self.Energy >= RepairCost) && (s.Self.Health > 0)) || (s.Self.Energy >= RepairCostDisabled) then (true,Some(Repair(a))) else (true,Some(Recharge))
        | Buy(u)    -> if s.Self.Energy > BuyCost then (true,Some(Buy(u))) else (true,Some(Recharge))
        | _         -> (false,None)

    let tryGo (v:Vertex) (s:State) =
        let edges = Set.toList s.World.[s.Self.Node].Edges
        let edge = List.find (fun (_,id) -> id = v.Identifier) edges
        if (fst edge) = None 
        then 
            if s.Self.Energy < 10 then (true,Some(Recharge)) else (true,Some(Goto(v)))
        else
            let value = (fst edge).Value
            if value > s.Self.Energy then (true,Some(Recharge)) else (true,Some(Goto(v)))

    let rec getSafeVertex (s:State) (l:Vertex list) (enemySabs:Agent list)=
        match l with
        | [] -> None
        | head :: tail -> 
            if not (List.tryFind (fun (enemy:Agent) -> s.World.[enemy.Node] = head) enemySabs = None) 
            then 
                getSafeVertex s tail enemySabs
            else
                Some head

    let runAway (s:State) (enemySabs:Agent list)= 
        let neighbours = getNeighbours s.Self.Node s.World
        let partition = List.partition (fun (v:Vertex) -> v.Edges.Count > 1) neighbours
        let sortedNeighbours = List.append (fst partition) (snd partition)
        let v = getSafeVertex s sortedNeighbours enemySabs
        if not (v = None) 
        then 
            tryGo v.Value s
        else
            tryGo sortedNeighbours.Head s
