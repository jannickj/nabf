namespace NabfAgentLogic
    module AgentLogic=
        open System
        open Graph
        open JSLibrary.IiLang
        open JSLibrary.IiLang.DataContainers
        open AgentTypes

        open Saboteur
        open Explorer
        open Inspector
        open Sentinel
        open Repairer
        

        (* handlePercept State -> Percept -> State *)
        let handlePercept state percept =
            match percept with
                | EnemySeen enemy   
                    -> { state with EnemyData = enemy :: state.EnemyData }
                | VertexSeen vertex 
                    -> { state with World = addVertex state.World vertex }
                | EdgeSeen edge          
                    -> { state with World = addEdge state.World edge }
                | Achievement achievement 
                    -> { state with Achievements = state.Achievements.Add achievement }
                | SimulationStep step
                    -> { state with SimulationStep = step }

        (* let updateState : State -> Percept list -> State *)
        let updateState state percepts = 
            List.fold handlePercept state percepts


        let buildIilAction (action:Action) =
            new IilAction "some action"

        let buildJobAccept (job:Job) =
            new IilAction "some action"

        let parseIilPercepts (perceptCollection:IilPerceptCollection) =
            PerceptCollection (List<Percept>.Empty)

        let generateJobs  (state:State) (jobs:Job list) = 
            List<Job>.Empty
        
        let rec addGotoActions neighbours =
            match neighbours with
            | [] -> []
            | (head:Vertex) :: tail -> List.append [Goto(head)] (addGotoActions tail)

        //Generate a list of possible actions given the current state
        let generateActions (state:State) =
            //Add generic actions
            let actionList = [Skip;Recharge;Survey;Buy(Battery);Buy(Sensor);Buy(Shield)]
            //Add goto actions
            let actionList = List.append (addGotoActions (getNeighbours state.Self.Position state.World)) actionList
            //Add type-specific actions
            match state.Self.Role with
            | Saboteur -> List.append (getSaboteurActions state) actionList
            | Explorer -> List.append (getExplorerActions state) actionList 
            | Repairer -> List.append (getRepairerActions state) actionList
            | Inspector -> List.append (getInspectorActions state) actionList
            | Sentinel -> List.append (getSentinelActions state) actionList

        //Returns an integer value representing the desirability of a given action
        let actionDesirability (state:State) (action:Action) = 
            0

        let actionDesirabilityBasedOnJob (state:State) (action:Action,oldDesirability:Desirability) (job:Job) =
            0
        let generateJob (jt:JobType) (s:State) (knownJobs:Job list)  =
            option<Job>.None

        let buildJob (job:Job) = 
            new IilAction "some action"
        let decideJob (job:Job) =
            let d:Desirability = 1
            d
        let buildEvaluationStarted =
            new IilAction "evaluation_started"
        let buildEvaluationEnded =
            new IilAction "evaluation_ended"