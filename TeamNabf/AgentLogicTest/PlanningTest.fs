namespace AgentLogicTest
    open System
    open NUnit.Framework
    open NabfAgentLogic.AgentTypes
    open NabfAgentLogic.PathFinding
    open NabfAgentLogic
    open NabfAgentLogic.AgentLogic
    open Graphing
    open FsPlanning

    [<TestFixture>]
    type PlanningTest() = 

        let buildEnemyAgent name node =   
                                {   Energy = Some 0
                                ;   MaxEnergy = Some 0
                                ;   Health = Some 0
                                ;   MaxHealth = Some 0
                                ;   Name = name
                                ;   Node = node
                                ;   Role = None
                                ;   Strength = Some 0
                                ;   Team = "EnemyTeam"
                                ;   Status = Normal
                                ;   VisionRange = Some 0
                                }
        
        [<Test>]
        member this.Solve_atV1AndEnemyAtV3_GotoV3AndKillAgent() =
            
            let world = Graph.Graph []
                        |> Graph.addVertexById "v1"
                        |> Graph.addVertexById "x1"
                        |> Graph.addVertexById "v2"
                        |> Graph.addVertexById "x2"
                        |> Graph.addVertexById "v3"
                        |> Graph.addVertexById "x3"
                        |> Graph.addEdge (None,"v1","v2")
                        |> Graph.addEdge (None,"v2","v3")
                        |> Graph.addEdge (None,"v1","x1")
                        |> Graph.addEdge (None,"x1","x2")
                        |> Graph.addEdge (None,"x2","x3")
            let init = buildInitState ("testbot",{ SimId = 0; SimEdges = 50; SimVertices = 50; SimRole = Saboteur })
            
            let enemies = [buildEnemyAgent "e1" "v3"]
            let s = { init with World = world; Self = { init.Self with Node = "v1"}; EnemyData = enemies; NearbyAgents = enemies }
             
            let enemyDead name s =
                let enem = List.find (fun e -> e.Name = name) s.EnemyData
                enem.Status = Disabled

            let plan = Planning.solve (enemyDead "e1") s
            let solu = plan.Value
            Assert.AreEqual(solu.Path,[Goto "v2"; Goto "v3"; Attack "e1"])
            ()