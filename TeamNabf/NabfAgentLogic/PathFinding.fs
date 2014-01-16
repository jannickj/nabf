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
        
        let stepProblem agent goalEvaluator = 
            { GoalEvaluator = goalEvaluator
            ; CostEvaluator = costEvaluator agent.MaxEnergy
            ; InitialCost   = { Steps = 0; Energy = agent.Energy }
            }
        
        let pathToNearest agent goalEvaluator (graph : Graph) = 
            dijkstra graph.[agent.Node] (stepProblem agent goalEvaluator) graph
            
        let pathTo agent goal graph =
            pathToNearest agent (fun vertex -> vertex.Identifier = goal) graph

