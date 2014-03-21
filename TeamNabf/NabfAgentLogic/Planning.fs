namespace NabfAgentLogic
module Planning = 
    
    open FsPlanning.Searching
    open AgentTypes
    open Graphing
    
    let getMoveActions (s:State) =
        List.map (fun (v:Graph.Vertex) -> Action.Goto v.Identifier) (Graph.getNeighbours s.Self.Node s.World)

    let getActions s =
        getMoveActions s

    let actionResult s a =
        match a with
        | Goto vn -> { s with Self = { s.Self with Node = vn }}
        | Attack agentname -> 
            let attacked = s.EnemyData
            { s with 

    let stepcost s a = 1

    let solve (goalTest: State -> bool) initState =
        let (problem:Problem<State,Action>) = { InitialState = initState; GoalTest = goalTest; Actions = getActions; Result = actionResult; StepCost = stepcost}
        ()