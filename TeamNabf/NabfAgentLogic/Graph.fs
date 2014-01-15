module Graph
    
    type Edge = Option<int> * string * string
    type DirectedEdge = Option<int> * string

    type Vertex = 
        {
            Identifier : string;
            Value : Option<int>;
            Edges : Set<DirectedEdge>;
        }

    type Graph = Map<string, Vertex>

    let getNeighbours identifier (graph:Graph) = List.map (fun id -> graph.[id]) (snd (List.unzip (Set.toList graph.[identifier].Edges)))

    let isVertexAdjacentTo identifier vertex = 
        Set.exists (fun (_, toVertex) -> toVertex = identifier) vertex.Edges 

    let addEdgeToVertex vertexId (edge : DirectedEdge) (graph : Graph) =
        let vertex = graph.[vertexId]
        let updatedVertex = { vertex with Edges = vertex.Edges.Add edge }
        Map.remove vertex.Identifier graph
        |> Map.add vertex.Identifier updatedVertex

    let rec updateGraph (vertex : Vertex) (graph : Graph) = 
        match Set.toList vertex.Edges with
        | (weight, otherVertexId) :: rest -> 
            addEdgeToVertex otherVertexId (weight, vertex.Identifier) graph
            |> updateGraph { vertex with Edges = vertex.Edges.Remove (weight, otherVertexId) }
        | [] -> graph

    let addVertex graph vertex = 
        updateGraph vertex graph |> Map.add vertex.Identifier vertex

    let addEdge (graph : Graph) (weight, vertexId1, vertexId2) =
        addEdgeToVertex vertexId1 (weight, vertexId2) graph
        |> addEdgeToVertex vertexId2 (weight, vertexId1)

    let join (graph : Graph) (graph' : Graph) = 
        Map.empty<string, Vertex> : Graph