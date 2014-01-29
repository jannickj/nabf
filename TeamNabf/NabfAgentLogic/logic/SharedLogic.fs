namespace NabfAgentLogic
module SharedLogic =

    open AgentTypes
    open Graphing.Graph
    open AgentLogicLib
    
    let workOnOccupyGoal (state : State) = 
        let occupyChooser = function
                            | OccupyGoal vertex -> Some vertex
                            | _ -> None

        let occupyVertices = List.choose occupyChooser state.Goals

        match occupyVertices with
        | vertex :: _ when vertex = state.Self.Node -> recharge
        | vertex :: _ when vertex <> state.Self.Node -> tryGo state.World.[vertex] state
        | _ -> (false, None)
            
                
            


