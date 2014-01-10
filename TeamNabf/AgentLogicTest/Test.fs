
namespace AgentLogicTest
open System
open NUnit.Framework
open AgentLogic
open Graph

[<TestFixture>]
type GraphTest() = 

        [<Test>]
        member this.AddVertex_EmptyGraph_GraphWithOneVertex () =
            let testVertex = { Identifier = "a"; Value = Some 1; Edges = Set.empty }
            let emptyGraph = Map.empty<string, Vertex>

            let expectedGraph = Map.add "a" testVertex Map.empty<string, Vertex> 
            let actualGraph = addVertex emptyGraph testVertex

            Assert.AreEqual (expectedGraph, actualGraph)

        [<Test>]
        member this.AddVertexWithEdgesToOtherVerticesInGraph_PopulatedGraph_EvenMoreSo () =
            (*
             *  A       A - C       A - C
             *  |          /        |  /
             *  |   +     /     =   | /
             *  |        /          |/ 
             *  B       B           B
             *)
            let testVertex = { Identifier = "c"
                             ; Value = None
                             ; Edges = [(None, "a"); (None, "b")] |> Set.ofList 
                             }

            let initialGraph = [ ("a", { Identifier = "a"; Value = None; Edges = [(None, "b")] |> Set.ofList }) 
                               ; ("b", { Identifier = "b"; Value = None; Edges = [(None, "a")] |> Set.ofList })
                               ] |> Map.ofList

            let expectedGraph = [ ("a", { Identifier = "a"
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

            let actualGraph = addVertex initialGraph testVertex
            let list1 = Map.toList expectedGraph
            let list2 = Map.toList actualGraph

            Assert.AreEqual (Map.toList expectedGraph, Map.toList actualGraph)
        
        [<Test>]
        member this.AddEdge_Graph2Vertices0Edges_Graph2Vertices1Edge () =
            (*
             * A               A
             *     +   |   =   |
             * B               B
             *)

            let testEdge = (None, "a", "b")
            let initialGraph = [ ("a", {Identifier = "a"; Value = None; Edges = Set.empty })
                                ; ("b", {Identifier = "b"; Value = None; Edges = Set.empty })
                                ] |> Map.ofList

            let expected = [ ("a", {Identifier = "a"; Value = None; Edges = [(None, "b")] |> Set.ofList })
                           ; ("b", {Identifier = "b"; Value = None; Edges = [(None, "a")] |> Set.ofList })
                           ] |> Map.ofList
             
            let actual = addEdge initialGraph testEdge
            Assert.AreEqual (expected, actual)
