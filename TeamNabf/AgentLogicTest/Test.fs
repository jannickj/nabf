
namespace AgentLogicTest
open System
open NUnit.Framework
open AgentLogic
open Graph

[<TestFixture>]
type GraphTest() = 

        [<Test>]
        member this.AddVertex_EmptyGraph_GraphWithOneVertex () =
            let testVertex = { Identifier = "a"; Value = Some 1; Edges = [] }
            let emptyGraph = Map.empty<string, Vertex>

            let expectedGraph = Map.add "a" testVertex Map.empty<string, Vertex> 
            let actualGraph = addVertex emptyGraph testVertex

            Assert.AreEqual (expectedGraph, actualGraph)

        [<Test>]
        member this.AddVertexWithEdgesToOtherVerticesInGraph_PopulatedGraph_EvenMoreSo () =
            let testVertex = { Identifier = "c"; Value = None; Edges = [(None, "a"); (None, "b")] }

            let initialGraph = [ ("a", { Identifier = "a"; Value = None; Edges = [(None, "b")] }) 
                               ; ("b", { Identifier = "b"; Value = None; Edges = [(None, "a")] })
                               ] |> Map.ofList

            let expectedGraph = [ ("a", { Identifier = "a"; Value = None; Edges = [(None, "c"); (None, "b")] }) 
                                ; ("b", { Identifier = "b"; Value = None; Edges = [(None, "c"); (None, "a")] })
                                ; ("c", { Identifier = "c"; Value = None; Edges = [(None, "a"); (None, "b")] }) 
                                ] |> Map.ofList

            let actualGraph = addVertex initialGraph testVertex
            let list1 = Map.toList expectedGraph
            let list2 = Map.toList actualGraph

            Assert.AreEqual (Map.toList expectedGraph, Map.toList actualGraph)
         
        
