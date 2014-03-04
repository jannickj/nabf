namespace NabfAgentLogic
    module PathFinding = 
        open FsPlanning.Searching
        open Graphing.Graph
        open Graphing.Dijkstra
        open AgentTypes
        open System.Collections.Generic

        type pathState = 
            { Vertex : VertexName
            ; EnergyLeft : int
            }

        let definiteCost cost = match cost with 
                                | Some c -> c
                                | None -> Constants.UNKNOWN_EDGE_COST
                            
        let planProblem (start : VertexName) (goal : VertexName) agent (graph : Graph) = 
            { InitialState = { Vertex = start; EnergyLeft = agent.Energy.Value }
            ; GoalTest = fun state -> state.Vertex = goal
            ; Actions = fun state -> graph.[state.Vertex].Edges |> Set.toList
            ; Result = fun state (cost, otherVertex) -> 
                let energyLeft = match state.EnergyLeft - definiteCost cost with
                                 | e when e > 0 -> e
                                 | _ -> (agent.MaxEnergy.Value / 2) - definiteCost cost
                { Vertex = otherVertex; EnergyLeft = energyLeft }

            ; StepCost = fun state (cost, _) -> if definiteCost cost > state.EnergyLeft then 1 else 2
            }

        [<CustomComparison; CustomEquality>]
        type PathCost = 
            { Steps : int
            ; Energy : int
            }

            interface System.IComparable<PathCost> with
                member this.CompareTo (other : PathCost) = 
                    let (Steps', Energy') = (other.Steps, other.Energy)
                    if (this.Steps > Steps') then      1
                    elif (this.Steps < Steps') then   -1
                    elif (this.Energy > Energy') then -1
                    elif (this.Energy < Energy') then  1
                    else                               0 
            
            interface System.IEquatable<PathCost> with
                member this.Equals other =
                    (this.Steps = other.Steps) && (this.Energy = other.Energy)

            interface System.IComparable with
                member this.CompareTo obj = 
                    match obj with
                    | :? PathCost as other -> (this :> System.IComparable<PathCost>).CompareTo other
                    | _                    -> failwith "%s is not a of type PathCost"

        let costEvaluator maxEnergy edgeCost currentCost = 
            let edgeCostGuess = 
                match edgeCost with
                | Some cost -> cost
                | None -> 5
            if currentCost.Energy >= edgeCostGuess then
                { Steps  = currentCost.Steps + 1
                ; Energy = currentCost.Energy - edgeCostGuess
                }
            else
                { Steps  = currentCost.Steps + 2
                ; Energy = (currentCost.Energy - edgeCostGuess) + (maxEnergy / 2)
                }
        
        let stepProblem maxEnergy energy goalEvaluator = 
            { GoalEvaluator = goalEvaluator
            ; CostEvaluator = costEvaluator maxEnergy
            ; InitialCost   = { Steps = 0; Energy = energy }
            }

        let rangeProblem goalId =
            { GoalEvaluator = fun vertex -> vertex.Identifier = goalId
            ; CostEvaluator = fun _ currentCost -> currentCost + 1
            ; InitialCost   = 0
            }

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
            
