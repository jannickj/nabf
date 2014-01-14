namespace NabfAgentLogic
module DijkstraTest =
    open System
    open NUnit.Framework
    open NabfAgentLogic.AgentLogic
    open Graph

    [<TestFixture>]
    type GraphTest() = 
            
            [<Test>]
            member this.FindPath_SimpleGraph_FindCorrectPath () =

                let graph = [ ("a", { Identifier = "a"; Value = None; Edges = [(None, "c"); (None, "b")] |> Set.ofList}); 
                                        ("b", { Identifier = "b"; Value = None; Edges = [(None, "c"); (None, "a")] |> Set.ofList}); 
                                        ("c", { Identifier = "c"; Value = None; Edges = [(None, "a"); (None, "b")] |> Set.ofList}) ] |> Map.ofList
                let correctPath = Some ["c"]

                let actualPath = Dijkstra.dijkstra graph.["a"] graph.["c"] Int32.MaxValue Int32.MaxValue graph

                Assert.AreEqual (correctPath, actualPath)

            [<Test>]
            member this.FindPath_AlreadyThere_ReturnEmptyPath () =

                let graph = [ ("a", { Identifier = "a"; Value = None; Edges = [(None, "c"); (None, "b")] |> Set.ofList})
                            ; ("b", { Identifier = "b"; Value = None; Edges = [(None, "c"); (None, "a")] |> Set.ofList})
                            ; ("c", { Identifier = "c"; Value = None; Edges = [(None, "a"); (None, "b")] |> Set.ofList}) 
                            ] |> Map.ofList

                let correctPath = Some []

                let actualPath = Dijkstra.dijkstra graph.["a"] graph.["a"] Int32.MaxValue Int32.MaxValue graph

                Assert.AreEqual (correctPath, actualPath)

            [<Test>]
            member this.FindPath_NoPathExists_ReturnsNone () =

                let graph = [ ("a", { Identifier = "a"; Value = None; Edges = [] |> Set.ofList})
                            ; ("b", { Identifier = "b"; Value = None; Edges = [] |> Set.ofList}) 
                            ] |> Map.ofList
                let correctOutput = None

                let actualOutput = Dijkstra.dijkstra graph.["a"] graph.["b"] Int32.MaxValue Int32.MaxValue graph

                Assert.AreEqual (correctOutput,actualOutput)

            [<Test>]
            member this.FindPath_TwoPossiblePaths_ReturnsShortestPath () =
           
               //        B------C     
               //       /        \    Find a path
               //      /          \   from A to E
               //     /            \
               //    A-------D------E

                let graph = [ ("a", { Identifier = "a"; Value = None; Edges = [(None, "b"); (None, "d")] |> Set.ofList}); 
                                        ("b", { Identifier = "b"; Value = None; Edges = [(None, "a"); (None, "c")] |> Set.ofList}); 
                                        ("c", { Identifier = "c"; Value = None; Edges = [(None, "b"); (None, "e")] |> Set.ofList}); 
                                        ("d", { Identifier = "d"; Value = None; Edges = [(None, "a"); (None, "e")] |> Set.ofList}); 
                                        ("e", { Identifier = "e"; Value = None; Edges = [(None, "c"); (None, "d")] |> Set.ofList}); ] |> Map.ofList

                let correctOutput = Some ["d";"e"]

                let actualOutput = Dijkstra.dijkstra graph.["a"] graph.["e"] Int32.MaxValue Int32.MaxValue graph

                Assert.AreEqual (correctOutput,actualOutput)

            [<Test>]
            member this.FindPath_TwoPathsWithEdgeCosts_ReturnsFastestPath () =
           
                //          
                //            10
                //     B---------------C         
                //     |                |       Find a path
                //  10 |                | 10    from A to G
                //     |                |
                //     A---D---E---F---G
                //       1   1   1   1

                let agent = {   
                                Id = "a1";
                                Type = Explorer;
                                Energy = 10;
                                Health = 1;
                                Strength = 1;
                                VisionRange = 1
                            }

                let graph = [ ("a", { Identifier = "a"; Value = None; Edges = [(Some 10, "b"); (Some 1, "d")] |> Set.ofList}) 
                            ; ("b", { Identifier = "b"; Value = None; Edges = [(Some 10, "a"); (Some 10, "c")] |> Set.ofList}) 
                            ; ("c", { Identifier = "c"; Value = None; Edges = [(Some 10, "b"); (Some 10, "g")] |> Set.ofList}) 
                            ; ("d", { Identifier = "d"; Value = None; Edges = [(Some 1, "a"); (Some 1, "e")] |> Set.ofList}) 
                            ; ("e", { Identifier = "e"; Value = None; Edges = [(Some 1, "d"); (Some 1, "f")] |> Set.ofList})
                            ; ("f", { Identifier = "f"; Value = None; Edges = [(Some 1, "e"); (Some 1, "g")] |> Set.ofList})
                            ; ("g", { Identifier = "g"; Value = None; Edges = [(Some 1, "f"); (Some 10, "c")] |> Set.ofList})
                            ] |> Map.ofList

                let correctOutput = Some ["d";"e";"f";"g"]

                let actualOutput = Dijkstra.dijkstra graph.["a"] graph.["g"] 20 agent.Energy graph

                Assert.AreEqual (correctOutput, actualOutput)
            
            [<Test>]
            member this.FindPath_TwoPathsThatTakeTheSameAmountOfTurnsPart1_ReturnsPathWithHighestFinalEnergy () =

                //
                //          B
                //      2  / \  2
                //        /   \  
                //       A     D        Find a path
                //        \   /         from A to D
                //      1  \ /  1
                //          C

                let agent = {   
                                Id = "a1";
                                Type = Explorer;
                                Energy = 10;
                                Health = 1;
                                Strength = 1;
                                VisionRange = 1
                            }

                let graph1 = [ ("a", { Identifier = "a"; Value = None; Edges = [(Some 2, "b"); (Some 1, "c")] |> Set.ofList}); 
                                        ("b", { Identifier = "b"; Value = None; Edges = [(Some 2, "a"); (Some 2, "d")] |> Set.ofList}); 
                                        ("c", { Identifier = "c"; Value = None; Edges = [(Some 1, "a"); (Some 1, "d")] |> Set.ofList}); 
                                        ("d", { Identifier = "d"; Value = None; Edges = [(Some 2, "b"); (Some 1, "c")] |> Set.ofList}); ] |> Map.ofList

                let correctOutput1 = Some ["c","d"]

                let actualOutput1 = Dijkstra.dijkstra graph1.["a"] graph1.["d"] 20 agent.Energy graph1

                Assert.AreEqual (correctOutput1,actualOutput1)


            [<Test>]
            member this.FindPath_TwoPathsThatTakeTheSameAmountOfTurnsPart2_ReturnsPathWithHighestFinalEnergy () =
               
                //
                //          B
                //      1  / \  1
                //        /   \  
                //       A     D        Find a path
                //        \   /         from A to D
                //      2  \ /  2
                //          C

                let agent = {   
                                Id = "a1";
                                Type = Explorer;
                                Energy = 10;
                                Health = 1;
                                Strength = 1;
                                VisionRange = 1
                            }

                let graph2 = [ ("a", { Identifier = "a"; Value = None; Edges = [(Some 1, "b"); (Some 2, "c")] |> Set.ofList}); 
                                        ("b", { Identifier = "b"; Value = None; Edges = [(Some 1, "a"); (Some 1, "d")] |> Set.ofList}); 
                                        ("c", { Identifier = "c"; Value = None; Edges = [(Some 2, "a"); (Some 2, "d")] |> Set.ofList}); 
                                        ("d", { Identifier = "d"; Value = None; Edges = [(Some 1, "b"); (Some 2, "c")] |> Set.ofList}); ] |> Map.ofList

                let correctOutput = Some ["b","d"]

                let actualOutput = Dijkstra.dijkstra graph2.["a"] graph2.["d"] 20 agent.Energy graph2

                Assert.AreEqual (correctOutput, actualOutput)
