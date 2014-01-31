namespace NabfAgentLogic
module SharedLogic =

    open AgentTypes
    open Graphing.Graph
    open AgentLogicLib
    open Logging
    open PathFinding

    let workOnOccupyGoal (state : State) = 
        let occupyChooser = function
                            | JobGoal (OccupyGoal vertex) -> Some vertex
                            | _ -> None

        let occupyVertices = List.choose occupyChooser state.Goals
        
        match occupyVertices with
        | vertex :: _ when vertex = state.Self.Node -> recharge
        | vertex :: _ when vertex <> state.Self.Node ->  let p = pathTo state.Self vertex state.World
                                                         match p with
                                                         | Some (first::_) -> tryGo state.World.[first] state
                                                         | _ -> (false, None)
        | _ -> (false, None)
            
                
            


