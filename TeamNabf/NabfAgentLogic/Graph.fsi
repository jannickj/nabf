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

    val addVertex : Graph -> Vertex -> Graph
    val addEdge : Graph -> Edge -> Graph
    val join : Graph -> Graph -> Graph