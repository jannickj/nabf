module Graph
    
    type Edge = Option<int> * string * string
    type DirectedEdge = Option<int> * string

    type Vertex = 
        {
            Value : Option<int>;
            Edges : DirectedEdge list;
        }

    type Graph = Map<string, Vertex>

    let addVertex (graph : Graph) (vertex : Vertex) = 
        Map.empty<string, Vertex> : Graph

    let addEdge (graph : Graph) (edge : Edge) = 
        Map.empty<string, Vertex> : Graph

    let join (graph : Graph) (graph' : Graph) = 
        Map.empty<string, Vertex> : Graph