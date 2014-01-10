module Dijkstra

open Graph

let getNeighbours (v : Vertex) (graph : Graph) = Set.toList (Set.map (fun (_, id) -> graph.[id]) v.Edges)



    