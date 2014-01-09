module Graph
    
    type Edge = Option<int> * string * string
    type DirectedEdge = Option<int> * string

    type Vertex = 
        {
            Identifier : string;
            Value : Option<int>;
            Edges : DirectedEdge list;
        }

    type Graph = Map<string, Vertex>

    let isVertexAdjacentTo identifier vertex = 
        List.exists (fun (_, toVertex) -> toVertex = identifier) vertex.Edges 

    let rec updateGraph (vertex : Vertex) (graph : Graph) = 
        match vertex.Edges with
        | (weight, otherVertexId) :: rest -> 
            let otherVertex : Vertex = graph.[otherVertexId]
            Map.remove otherVertexId graph
            |> Map.add otherVertex.Identifier 
                { otherVertex with Edges = (weight, vertex.Identifier) :: otherVertex.Edges }
            |> updateGraph { vertex with Edges = rest }
        | [] -> graph

    let addVertex (graph : Graph) (vertex : Vertex) = 
        updateGraph vertex graph |> Map.add vertex.Identifier vertex

    let addEdge (graph : Graph) (edge : Edge) = 
        Map.empty<string, Vertex> : Graph

    let join (graph : Graph) (graph' : Graph) = 
        Map.empty<string, Vertex> : Graph