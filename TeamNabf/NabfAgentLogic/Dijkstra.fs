module Dijkstra

open Graph

let getNeighbours current (graph : Graph) = 
    current.Edges 
    |> Set.map (fun (edgeCost, otherId) -> 
        match edgeCost with
        | Some cost -> (float cost , otherId)
        | None -> (5., otherId))
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

let sortFrontier (costMap : Map<string, float>) frontier = 
    List.sortBy (fun elem -> costMap.[elem]) frontier 

let rec addNeighbours current neighbours frontier (costMap : Map<string, float>) vertexTree =
    match neighbours with
    | (neighbourCost, neighbourId) :: tail -> 
        if neighbourCost < costMap.[neighbourId] then
            let newCostMap = Map.add neighbourId neighbourCost costMap
            let newFrontier = 
                if costMap.[neighbourId] = infinity then
                    (neighbourId :: frontier) |> sortFrontier newCostMap 
                else
                    frontier |> sortFrontier newCostMap
            let newVertexTree = Map.add neighbourId current vertexTree
            addNeighbours current tail newFrontier newCostMap newVertexTree
        else 
            addNeighbours current tail frontier costMap vertexTree
    | [] -> (frontier, costMap, vertexTree)

let constructCostMap (graph : Graph) (start : Vertex) = 
    Map.toList graph
    |> List.unzip
    |> fst
    |> List.map (fun elem -> 
        if elem = start.Identifier then 
            (elem, 0.0) 
        else 
            (elem, infinity))
    |> Map.ofList

let dijkstra start (goal : Vertex) (graph : Graph) = 
    let rec dijkstraHelper current explored frontier currentCost vertexTree (costMap : Map<string, float>) =
        let newExplored = Set.add current.Identifier explored
        let (newFrontier, newCostMap, newVertexTree) = addNeighbours current.Identifier (getNeighbours current graph) frontier costMap vertexTree
        printfn "current: %s \nexplored: %A \nfrontier: %A \nvertexTree: %A\n" current.Identifier (Set.toList newExplored) newFrontier (Map.toList vertexTree)

        match newFrontier with
        | bestId :: _ when bestId = goal.Identifier ->
            printfn "goal: %s\n" bestId
            Some <| Map.add goal.Identifier current.Identifier vertexTree
        | bestId :: rest ->
            printfn "bestId: %s\n" bestId
            dijkstraHelper graph.[bestId] newExplored rest (currentCost + 1) newVertexTree newCostMap
        | [] -> 
            None

    let rec constructPath (current : string) (vertexTree : Map<string, string>) =
        if current = start.Identifier then
            []
        else
            (constructPath vertexTree.[current] vertexTree) @ [current]

    let costMap = constructCostMap graph start
    let vertexTree = dijkstraHelper start (Set.singleton start.Identifier) List.empty 0 Map.empty<string, string> costMap
    printfn "vertexTree: %A" <| Map.toList<string, string> vertexTree.Value
    match vertexTree with
    | Some tree -> Some <| constructPath goal.Identifier tree
    | None -> None
     
    