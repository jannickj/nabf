namespace NabfAgentLogic
    module PathFinding = 
        open FsPlanning.Searching
        open Graphing.Graph
        open Graphing.Dijkstra
        open AgentTypes
        open System.Collections.Generic

        type pathState = 
            { Vertex : VertexName
            ; Energy : int
            }

        let definiteCost cost = match cost with 
                                | Some c -> c
                                | None -> Constants.UNKNOWN_EDGE_COST
                            
        let stepProblem agent (goalEvaluator : VertexName -> bool) (graph : Graph) = 
            { InitialState = { Vertex = agent.Node; Energy = agent.Energy.Value }
            ; GoalTest = fun state -> goalEvaluator (state.Vertex)
            ; Actions = fun state -> graph.[state.Vertex].Edges |> Set.toList
            ; Result = fun state (cost, otherVertex) -> 
                let energyLeft = match state.Energy - definiteCost cost with
                                 | e when e > 0 -> e
                                 | _ -> (agent.MaxEnergy.Value / 2) - definiteCost cost
                { Vertex = otherVertex; Energy = energyLeft }

            ; StepCost = fun state (cost, _) -> if definiteCost cost > state.Energy then 1 else 2
            }
        
        let rangeProblem agent goalEvaluator graph =
            let stepProb = stepProblem agent goalEvaluator graph
            { stepProb with StepCost = fun _ _ -> 1 }

        let pathToNearest agent goalEvaluator graph =
            aStar <| stepProblem agent goalEvaluator graph
        
        let pathTo agent goal graph = 
            pathToNearest agent (fun vertex -> vertex = goal) graph

        let distanceTo agent goal graph =
            aStar <| rangeProblem agent ((=) agent.Node) graph

        let pathToNearestWithCost agent goalEvaluator (graph : Graph) =
            match (agent.MaxEnergy, agent.Energy) with
                | (Some maxEnergy, Some energy) -> 
                    dijkstra graph.[agent.Node] (stepProblem maxEnergy energy goalEvaluator) graph 
                | (_, _) -> 
                    failwithf "agent %s has unknown energy and/or maxEnergy" agent.Name
        
        let pathToNearest agent goalEvaluator (graph : Graph) = 
            let pathAndCost = pathToNearestWithCost agent goalEvaluator graph
            match pathAndCost with
            | Some (_, path) -> Some path
            | None -> None
            
            
        let pathTo agent goal graph =
            pathToNearest agent (fun vertex -> vertex.Identifier = goal) graph

        let distanceTo agent goalId (graph : Graph) =
            let path = dijkstra graph.[agent.Node] (rangeProblem goalId) graph
            match path with
            | Some (_, ls) -> List.length ls
            | None -> 0

        let pathToNearestUnProbed agent graph =
            let goalEvaluator vertex = vertex.Value = None
            pathToNearest agent goalEvaluator graph

        let isUnexplored vertex = 
            Set.forall (fun (cost,_) -> cost = None) vertex.Edges

        let pathToNearestUnExplored agent graph =
            let goalEvaluator vertex = 
                Set.forall (fun (cost,_) -> cost = None) vertex.Edges
                && vertex.Identifier <> agent.Node
            let path = pathToNearest agent goalEvaluator graph
            match path with
            | Some [] -> None 
            | path -> path

        let pathsToNearestNUnexplored n agent graph =
            let rec helper n lastSteps goalEvaluator =
                match n with
                | -1 -> []
                | _ -> 
                    let costAndPath = pathToNearestWithCost agent goalEvaluator graph
                    match costAndPath with
                    | Some (cost, path) -> 
                        if Some cost.Steps = lastSteps || lastSteps = None then
                            let newGoalEvaluator = (fun vertex -> (goalEvaluator vertex) && vertex.Identifier <> (List.rev >> List.head <| path))
                            path :: helper (n - 1) (Some cost.Steps) newGoalEvaluator 
                        else 
                            []
                    | None -> []

            let goalEvaluator vertex = 
                (isUnexplored vertex) && vertex.Identifier <> agent.Node

            helper n None goalEvaluator
            
