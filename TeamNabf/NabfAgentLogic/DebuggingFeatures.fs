namespace NabfAgentLogic
    module DebuggingFeatures = 
        open AgentTypes
        open CommonLogic
        open Graphing.Graph
        open Graphing.Dijkstra
        open PathFinding
        open AgentLogicLib

        let moveTo vName (s:State) = 
            if s.World.ContainsKey(vName) then
                let p = pathTo s.Self vName s.World
                if p.IsSome && (not p.Value.IsEmpty) then
                    tryGo s.World.[p.Value.Head] s
                else
                    (false,None)
            else
                (false,None)

        let moveToDTree vName = 
            Options 
                    [   Choice(moveTo vName)
                    ;   Choice(exploreLocalGraph)
                    ]

        
