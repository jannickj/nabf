module Dijkstra

open Graph

(* val getNeighbours : Vertex -> Graph -> Vertex list *)
let getNeighbours vertex (graph : Graph) = 
    Set.map (fun (_, id) -> graph.[id]) vertex.Edges

let updateFrontier vertex currentCost frontier =
    vertex.Edges 
    |> Set.toList 
    |> List.map (fun (cost, id) -> (1 + currentCost, id))
    |> List.append frontier
    |> List.sortBy (fun (cost, _) -> cost)

let dijkstra start (goal : Vertex) (graph : Graph) =
    let rec dijkstraHelper current explored frontier currentCost vertexTree =
        let hej = goal.Identifier
        let newExplored = Set.add current.Identifier explored
        let newFrontier = updateFrontier current currentCost frontier
        match newFrontier with
            | (_, hej) :: _
                -> Map.add goal.Identifier current.Identifier vertexTree
            | (bestCost, bestId) :: rest when not (Set.contains bestId explored) 
                -> dijkstraHelper graph.[bestId] newExplored rest bestCost (Map.add bestId current.Identifier vertexTree)
            | _ 
                -> vertexTree

    let rec constructPath (current : string) (vertexTree : Map<string, string>) =
        let parent = vertexTree.[current]
        if parent = start.Identifier then
            []
        else
            current :: (constructPath parent vertexTree)

 
    dijkstraHelper start (Set.singleton start.Identifier) (updateFrontier start 0 List.empty) 0 Map.empty<string, string>
    |> constructPath goal.Identifier
     
    