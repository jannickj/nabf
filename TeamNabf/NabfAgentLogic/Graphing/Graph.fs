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

        let getNeighbours identifier (graph:Graph) = 
            let vertices = List.filter (fun id -> graph.ContainsKey id) (snd (List.unzip (Set.toList graph.[identifier].Edges)))
            List.map (fun id -> graph.[id]) vertices

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

        let addVertexValue (graph : Graph) (vertex : Vertex) =
//            Map.add id {graph.[id] with Value = value} graph
            let vertex = graph.[vertex.Identifier]
            let updatedVertex = { vertex with Value = vertex.Value }
            Map.remove vertex.Identifier graph
            |> Map.add vertex.Identifier updatedVertex

        let addEdgeCost (graph : Graph) ((v,s1,s2) : Edge) =
            let v1 = graph.[s1]
            let e1 = Set.map (fun (e : DirectedEdge) -> if e = (None,s2) then (v,s2) else (None,s2)) v1.Edges
            let g1 = graph.Add (s1,{v1 with Edges = e1})
            let v2 = g1.[s2]
            let e2 = Set.map (fun (e : DirectedEdge) -> if e = (None,s1) then (v,s1) else (None,s1)) v2.Edges
            g1.Add (s2,{v2 with Edges = e2})
        
        let removeEdgeFromVertex edge vertex =
            { vertex with Edges = Set.remove edge vertex.Edges }

        let removeVertex vertex (graph : Graph) =
            let updateVertex (cost, id) (graph : Graph) = 
                Map.add id (removeEdgeFromVertex (cost, vertex.Identifier) graph.[id]) graph
            
            printfn "edges: %A" <| Set.toList vertex.Edges
             
            vertex.Edges
            |> Set.fold (fun graph edge -> updateVertex edge graph) graph
            |> Map.remove vertex.Identifier



