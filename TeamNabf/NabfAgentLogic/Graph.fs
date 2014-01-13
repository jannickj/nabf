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

    let contains l elem = List.exists ((=) elem) l

    let containsId id l = List.exists (fun elem -> elem.Identifier = id) l

    let getElem id l = List.find (fun elem -> elem.Identifier = id) l

    let rec joinLists (l : Vertex List) (l' : Vertex List) =
        match l with
        | [] -> l'
        | head :: tail -> 
          if (containsId head.Identifier l') then 
            let (e : Vertex) = getElem head.Identifier l'
            let newEdgeSet = Set.union head.Edges e.Edges
            let newList = List.filter ((<>) e) l'
            joinLists tail ({Identifier=head.Identifier;Value=head.Value;Edges=newEdgeSet}::newList)
          else
            joinLists tail (head::l')

    let rec addToMap l (m : Map<string, Vertex>) =
        match l with
        | [] -> m
        | head :: tail -> addToMap tail (m.Add (head.Identifier, head))

    let listFromGraph (graph : Graph) = List.map snd <| Map.toList graph
              
    let join (graph : Graph) (graph' : Graph) =
        let joinedList = joinLists (listFromGraph graph) (listFromGraph graph')
        (addToMap joinedList Map.empty<string, Vertex>) : Graph