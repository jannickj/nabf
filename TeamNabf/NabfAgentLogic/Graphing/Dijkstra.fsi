namespace Graphing
    module Dijkstra =

        open Graph

        val dijkstra : Vertex -> Vertex -> int -> int -> Graph -> (string list) option