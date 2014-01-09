
namespace AgentLogicTest
open System
open NUnit.Framework
open AgentLogic
open Graph

[<TestFixture>]
type GraphTest() = 

        [<Test>]
        member this.AddVertex_EmptyGraph_GraphWithOneVertex () =
            let testVertex = { Value = Some 1; Edges = [] }
            let emptyGraph = Map.empty<string, Vertex>

            let expectedGraph = Map.add Map.empty<string, Vertex> testVertex
            let actualGraph = addVertex emptyGraph testVertex

            Assert.AreEqual (expectedGraph, actualGraph)

        [<Test>]
        member this.AddVertexWithEdgesToOtherVerticesInGraph_PopulatedGraph_EvenMoreSo () =
            let testVertex = { Value = None; Edges = [(None, "a"); (None, "b")] }

            let initialGraph = [ ("a", { Value = None; Edges = [(None, "b")] }) 
                               ; ("b", { Value = None; Edges = [(None, "a")] })
                               ] |> Map.ofList

            let expectedGraph = [ ("a", { Value = None; Edges = [(None, "b"); (None, "c")] }) 
                                ; ("b", { Value = None; Edges = [(None, "a"); (None, "c")] })
                                ; ("c", { Value = None; Edges = [(None, "a"); (None, "b")] }) 
                                ] |> Map.ofList

            let actualGraph = addVertex initialGraph testVertex
            Assert.AreEqual (expectedGraph, actualGraph)
         
        
