namespace NabfAgentLogic
    module PathFinding = 
        open Graphing.Graph
        open Graphing.Dijkstra
        open AgentTypes
        open System.Collections.Generic

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
        
        let pathToNearest agent goalEvaluator (graph : Graph) = 
            match (agent.MaxEnergy, agent.Energy) with
            | (Some maxEnergy, Some energy) -> 
                dijkstra graph.[agent.Node] (stepProblem maxEnergy energy goalEvaluator) graph
            | (_, _) -> 
                failwithf "agent %s has unknown energy and/or maxEnergy" agent.Name
            
        let pathTo agent goal graph =
            pathToNearest agent (fun vertex -> vertex.Identifier = goal) graph

        let distanceTo agent goalId (graph : Graph) =
            let path = dijkstra graph.[agent.Node] (rangeProblem goalId) graph
            match path with
            | Some ls -> List.length ls
            | None -> 0

        let pathToNearestUnProbed agent graph =
            let goalEvaluator vertex = vertex.Value = None
            pathToNearest agent goalEvaluator graph

        let pathToNearestUnExplored agent graph =
            let goalEvaluator vertex = 
                Set.forall (fun (cost,_) -> cost = None) vertex.Edges
                && vertex.Identifier <> agent.Node
            let path = pathToNearest agent goalEvaluator graph
            match path with
            | Some [] -> None
            | _ -> path
