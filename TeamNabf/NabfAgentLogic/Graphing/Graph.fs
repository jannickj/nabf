namespace Graphing
    module Graph =
    
        type Edge = Option<int> * string * string
        type DirectedEdge = Option<int> * string

        type VertexName = string

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

        let addEdgeToVertex (vertexId : string) ((value, otherVertexId) : DirectedEdge) (graph : Graph) =
            let vertex = graph.[vertexId]
            let updatedVertex = 
                { vertex with Edges = 
                    Set.filter (fun (_, id) -> id <> otherVertexId) vertex.Edges
                    |> Set.add (value, otherVertexId) 
                } 
            Map.add vertex.Identifier updatedVertex graph

        let rec updateGraph (vertex : Vertex) (graph : Graph) = 
            match Set.toList vertex.Edges with
            | (weight, otherVertexId) :: rest -> 
                addEdgeToVertex otherVertexId (weight, vertex.Identifier) graph
                |> updateGraph { vertex with Edges = vertex.Edges.Remove (weight, otherVertexId) }
            | [] -> graph

        let addVertex graph vertex = 
            updateGraph vertex graph |> Map.add vertex.Identifier vertex

        let addEdge ((weight, vertexId1, vertexId2) : Edge) (graph : Graph) =
            addEdgeToVertex vertexId1 (weight, vertexId2) graph 
            |> addEdgeToVertex vertexId2 (weight, vertexId1)

        let join (graph : Graph) (graph' : Graph) = 
            Map.empty<string, Vertex> : Graph

        let addVertexValue (graph : Graph) (vertex : Vertex) =
            let vertex' = graph.[vertex.Identifier]
            let updatedVertex = { vertex' with Value = vertex.Value }
            Map.remove vertex.Identifier graph
            |> Map.add vertex.Identifier updatedVertex

        let addEdgeCost ((value, vertex1, vertex2) : Edge) (graph : Graph) =
            Map.empty<string,Vertex>
        
        let removeEdgeFromVertex edge vertex =
            { vertex with Edges = Set.remove edge vertex.Edges }

        let removeVertex vertex (graph : Graph) =
            let updateVertex (cost, id) (graph : Graph) = 
                Map.add id (removeEdgeFromVertex (cost, vertex.Identifier) graph.[id]) graph
            
            printfn "edges: %A" <| Set.toList vertex.Edges
             
            vertex.Edges
            |> Set.fold (fun graph edge -> updateVertex edge graph) graph
            |> Map.remove vertex.Identifier



