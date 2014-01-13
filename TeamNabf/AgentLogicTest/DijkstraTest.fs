
namespace AgentLogicTest
open System
open NUnit.Framework
open Graph

[<TestFixture>]
type DijkstraTest() = 
        [<Test>]
        member this.AddVertex_EmptyGraph_GraphWithOneVertex2 () =
            let graph = [ ("a", { Identifier = "a"
                                ; Value = None
                                ; Edges = [(None, "c"); (None, "b")] |> Set.ofList 
                                }) 
                        ; ("b", { Identifier = "b"
                                ; Value = None
                                ; Edges = [(None, "c"); (None, "a")] |> Set.ofList 
                                })
                        ; ("c", { Identifier = "c"
                                ; Value = None
                                ; Edges = [(None, "a"); (None, "b")] |> Set.ofList 
                                }) 
                        ] |> Map.ofList
            
            let path = Dijkstra.dijkstra graph.["a"] graph.["b"] graph

            Assert.Fail ()
