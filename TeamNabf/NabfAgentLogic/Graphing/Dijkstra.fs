namespace Graphing
    module Dijkstra =

        open Graph

        type 'a Problem when 'a : comparison =
            { GoalEvaluator : Vertex -> bool
            ; CostEvaluator : int -> 'a -> 'a
            ; InitialCost   : 'a
            }

        type Cost = int * int
        type CostMap<'a> = Map<string, 'a>
        type Frontier = string list
        type VertexTree = Map<string, string>

        type DijkstraState<'a> = Frontier * CostMap<'a> * VertexTree

        let getNeighbours current (graph : Graph) = 
            current.Edges 
            |> Set.map (fun (edgeCost, otherId) -> 
                match edgeCost with
                | Some cost -> (cost, otherId)
                | None -> (5, otherId))
            |> Set.toList

//        let updateFrontier vertex currentCost explored frontier =
//            vertex.Edges 
//            |> Set.toList 
//            |> List.filter (fun (_, id) -> not <| Set.contains id explored)
//            |> List.map (fun (cost, id) -> (1 + currentCost, id))
//            |> List.append frontier
//            |> List.sortBy (fun (cost, _) -> cost)


        let rec removeDuplicatesFromSortedList ls = 
            match ls with
            | first :: second :: tail when first = second ->
                second :: removeDuplicatesFromSortedList tail
            | head :: tail -> 
                head :: removeDuplicatesFromSortedList tail
            | [] -> []

        let sortFrontier (costMap : CostMap<'a>) (frontier : Frontier) = 
            List.sortBy (fun elem -> costMap.[elem]) frontier

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

        let dijkstra start (problem : 'a Problem) (graph : Graph) = 
            
            let rec constructPath (current : string) (vertexTree : Map<string, string>) =
                if current = start.Identifier then
                    []
                else
                    (constructPath vertexTree.[current] vertexTree) @ [current]

            let rec dijkstraHelper current explored frontier vertexTree (costMap : CostMap<'a>) =
                
                let addNeighbour (dState : DijkstraState<'a>) neighbour =
                    let (thisFrontier, thisCostMap, thisVertexTree) = dState
                    let (edgeCost, neighbourId) = neighbour
                    let neighbourCost = problem.CostEvaluator edgeCost costMap.[current.Identifier] 

                    if (Map.containsKey neighbourId thisCostMap) && (thisCostMap.[neighbourId] < neighbourCost) then
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

                match newFrontier with
                | bestId :: _ when problem.GoalEvaluator graph.[bestId] ->
                    Some <| constructPath bestId newVertexTree
                | bestId :: rest ->
                    dijkstraHelper graph.[bestId] newExplored rest newVertexTree newCostMap
                | [] -> 
                    None

            if problem.GoalEvaluator start then
                Some []
            else
                let explored = Set.singleton start.Identifier
                let frontier = List.empty
                let vertexTree = Map.empty<string, string>
                let costMap = Map.ofList [(start.Identifier, problem.InitialCost)]
                dijkstraHelper start explored frontier vertexTree costMap

             
            