module Dijkstra

open Graph

type Cost = int * int
type CostMap = Map<string, Cost>
type Frontier = string list
type VertexTree = Map<string, string>

type DijkstraState = Frontier * CostMap * VertexTree

let getNeighbours current (graph : Graph) = 
    current.Edges 
    |> Set.map (fun (edgeCost, otherId) -> 
        match edgeCost with
        | Some cost -> (cost, otherId)
        | None -> (5, otherId))
    |> Set.toList

let updateFrontier vertex currentCost explored frontier =
    vertex.Edges 
    |> Set.toList 
    |> List.filter (fun (_, id) -> not <| Set.contains id explored)
    |> List.map (fun (cost, id) -> (1 + currentCost, id))
    |> List.append frontier
    |> List.sortBy (fun (cost, _) -> cost)


let rec removeDuplicatesFromSortedList ls = 
    match ls with
    | first :: second :: tail when first = second ->
        second :: removeDuplicatesFromSortedList tail
    | head :: tail -> 
        head :: removeDuplicatesFromSortedList tail
    | [] -> []

let sortFrontier (costMap : CostMap) (frontier : Frontier) = 
    List.sortBy (fun elem -> 
        let (steps, energyLeft) = costMap.[elem]
        (steps, -energyLeft)
        ) frontier

let evaluateCost maxEnergy currentCost edgeCost =
    let (steps, energyLeft) = currentCost
    if energyLeft >= edgeCost then
        (steps + 1, energyLeft - edgeCost)
    else
        (steps + 2, (energyLeft - edgeCost) + (maxEnergy / 2))

let compareCost cost cost' =
        let (steps, energy) = cost
        let (steps', energy') = cost'
        if (steps > steps') then
            1
        elif (steps < steps') then
            -1
        elif (energy > energy') then
            -1
        elif (energy < energy') then
            1
        else 
            0            

let dijkstra start (goal : Vertex) maxEnergy currentEnergy (graph : Graph) = 
    let rec dijkstraHelper current explored frontier vertexTree (costMap : CostMap) =
        
        let addNeighbour (dState : DijkstraState) neighbour =
            let (thisFrontier, thisCostMap, thisVertexTree) = dState
            let (edgeCost, neighbourId) = neighbour
            let neighbourCost = evaluateCost maxEnergy costMap.[current.Identifier] edgeCost 

            if (Map.containsKey neighbourId thisCostMap) && ((compareCost thisCostMap.[neighbourId] neighbourCost) = -1) then
                dState
            else
                let newFrontier = 
                    if (Map.containsKey neighbourId thisCostMap) then
                        thisFrontier
                    else
                        (neighbourId :: thisFrontier)
                let newCostMap = Map.add neighbourId neighbourCost thisCostMap
                let newVertexTree = Map.add neighbourId current.Identifier thisVertexTree
                (newFrontier |> sortFrontier newCostMap, newCostMap, newVertexTree)

        let newExplored = Set.add current.Identifier explored
        let (newFrontier, newCostMap, newVertexTree) = 
            getNeighbours current graph
            |> List.filter (fun (_, id) -> not <| Set.contains id newExplored)
            |> List.fold addNeighbour (frontier, costMap, vertexTree)
        printfn "current: %s \nexplored: %A \nfrontier: %A \nvertexTree: %A \ncostMap: %A\n" current.Identifier (Set.toList newExplored) newFrontier (Map.toList newVertexTree) (Map.toList newCostMap)

        match newFrontier with
        | bestId :: _ when bestId = goal.Identifier ->
            printfn "goal: %s\n" bestId
            Some newVertexTree
        | bestId :: rest ->
            printfn "bestId: %s\n" bestId
            dijkstraHelper graph.[bestId] newExplored rest newVertexTree newCostMap
        | [] -> 
            None

    let rec constructPath (current : string) (vertexTree : Map<string, string>) =
        printfn "vertexTree: %A" <| Map.toList vertexTree
        printfn "current: %s" current
        if current = start.Identifier then
            printfn "end of list"
            []
        else
            printfn "not end"
            (constructPath vertexTree.[current] vertexTree) @ [current]
    
    if start.Identifier = goal.Identifier then
        Some []
    else
        let vertexTree = dijkstraHelper start (Set.singleton start.Identifier) List.empty Map.empty<string, string> (Map.ofList [(start.Identifier, (0, currentEnergy))])
        match vertexTree with
        | Some tree -> Some <| constructPath goal.Identifier tree
        | None -> None
     
    