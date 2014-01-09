module Graph
    type Vertex = 
        {
            Value : int;
            Edges : (int * string) list;
        }

    type Graph = Map<string, Vertex>