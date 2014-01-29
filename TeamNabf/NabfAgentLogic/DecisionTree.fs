namespace NabfAgentLogic

module DecisionTree =
    
    open AgentTypes
    open CommonLogic

    open SharedLogic
    open Saboteur
    open Explorer
    open Inspector
    open Sentinel
    open Repairer

    let isRole (r:Option<AgentRole>)(s:State) = (s.Self.Role = r,None)
            
    let getRoleDecision : Decision<(State -> (bool*Option<Action>))> =
        Options 
            [
                Condition(isRole (Some Explorer),
                    Options [ getExplorerTree ])
                Condition(isRole (Some Repairer),
                    Options [ getRepairerTree ])
                Condition(isRole (Some Saboteur),
                    Options [ getSaboteurTree ])
                Condition(isRole (Some Inspector),
                    Options [ getInspectorTree ])
                Condition(isRole (Some Sentinel),
                    Options [ getSentinelTree ])
            ]

    let getTree : Decision<(State -> (bool*Option<Action>))> =
        Options 
            [
                getRoleDecision
                
                Choice(reactToEnemyAgent)

                Choice workOnOccupyGoal

                Choice(exploreLocalGraph)

                Choice(idle)
            ]