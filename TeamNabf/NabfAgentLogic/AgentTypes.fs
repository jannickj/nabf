namespace NabfAgentLogic

module AgentTypes =

    open Graphing.Graph

    type TeamName = string
    type AgentName = string

    type Decision<'a> =
        | Condition of 'a * Decision<'a>
        | Choice of 'a
        | Options of Decision<'a> list

    type Upgrade =
        | Battery
        | Sensor
        | Shield
        | SabotageDevice

    type ActionResult =
        | Successful
        | Failed
        | FailedResources
        | FailedAttacked
        | FailedParried
        | FailedUnreachable
        | FailedOutOfRange
        | FailedInRange
        | FailedWrongParam 
        | FailedRole
        | FailedStatus
        | FailedLimit
        | FailedRandom

    type AgentRole =
        | Saboteur
        | Explorer
        | Repairer
        | Inspector
        | Sentinel

    type Level = int

    type Achievement =
        | ConqueredZone      of Level
        | ProbedVertices     of Level
        | SurveyedEdges      of Level
        | InspectedVehicles  of Level
        | Attacked           of Level
        | Parried            of Level
    
    type EntityStatus =
        | Normal
        | Disabled

    type Agent =
        { Energy      : Option<int>
        ; Health      : Option<int>
        ; MaxEnergy   : Option<int>
        ; MaxHealth   : Option<int>
        ; Name        : string
        ; Node        : string
        ; Role        : Option<AgentRole>
        ; Strength    : Option<int>
        ; Team        : string
        ; VisionRange : Option<int>
        ; Status      : EntityStatus
        }

    type TeamState =
        { LastStepScore : int
        ; Money         : int
        ; Score         : int
        ; ZoneScore     : int
        ; Achievements  : Achievement list
        }

    type Action =
        | Skip
        | Recharge
        | Goto      of VertexName
        | Probe     of Option<VertexName>
        | Survey    
        | Inspect   of Option<AgentName>
        | Attack    of AgentName
        | Parry
        | Repair    of AgentName
        | Buy       of Upgrade

    type JobID = int
    type JobValue = int
    type Desirability = int

    type JobType = 
        | OccupyJob = 1
        | RepairJob = 2
        | DisruptJob = 3
        | AttackJob = 4

    type JobData =
        | OccupyJob of VertexName list
        | RepairJob of VertexName * AgentName
        | DisruptJob of VertexName
        | AttackJob of VertexName list

    type JobHeader = JobID * JobValue * JobType

    type Job = JobHeader * JobData

    let levelToPoints start level =
        start * (pown 2 level)

    let achievementPoints achievement =
        levelToPoints <||
            match achievement with
            | ConqueredZone l     -> (l, 10)
            | ProbedVertices l    -> (l, 5)
            | SurveyedEdges l     -> (l, 10)
            | InspectedVehicles l -> (l, 5)
            | Attacked l          -> (l, 5)
            | Parried  l          -> (l, 5)

    type SeenVertex = VertexName * TeamName

    type Percept =
        | EnemySeen         of Agent
        | VertexSeen        of SeenVertex
        | VertexProbed      of VertexName * int
        | EdgeSeen          of Edge
        | SimulationStep    of int
        | ActionRequest     of (int * int * int)
        | Health            of int
        | MaxHealth         of int
        | Energy            of int
        | MaxEnergy         of int
        | MaxEnergyDisabled of int
        | LastActionResult  of ActionResult
        | ZoneScore         of int
        | Team              of TeamState
        | Position          of VertexName
        | Strength          of int
        | VisionRange       of int
        | Self              of Agent


    type Deadline = int
    type CurrentTime = int
    type Rank = int
    type Score = int
    type ActionID = int
    
    type SimStartData =
        {
            SimId          :   int;
            SimEdges       :   int;
            SimVertices    :   int;
            SimRole        :   AgentRole;
            SimTotalSteps  :   int
        }

    type AgentServerMessage =
        | NewJobs of Job List
        | AcceptedJob of JobID
        | SharedPercepts of Percept list

    type MarsServerMessage =  
        | ActionRequest of  Deadline*CurrentTime*ActionID*(Percept list)
        | SimulationStart of SimStartData
        | SimulationEnd of Rank*Score
        | ServerClosed

    type ServerMessage = 
        | AgentServerMsg of AgentServerMessage
        | MarsServerMsg of MarsServerMessage

    type State =
        { 
            World            : Graph; 
            Self             : Agent; 
            EnemyData        : Agent list; 
            SimulationStep   : int;
            NearbyAgents     : Agent list
            OwnedVertices    : Map<VertexName, TeamName>
            NewVertices      : SeenVertex list
            NewEdges         : Edge list
            LastStepScore    : int
            Money            : int
            Score            : int
            ZoneScore        : int
            Achievements     : Achievement list
            LastActionResult : ActionResult
            LastAction       : Action

        }

    type OptionFunc = State -> (bool*Option<Action>)

    type DecisionRank = int
