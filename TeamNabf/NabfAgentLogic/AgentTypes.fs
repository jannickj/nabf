namespace NabfAgentLogic

module AgentTypes =

    open Graphing.Graph

    type Decision<'a> =
        | Condition of 'a * Decision<'a>
        | Choice of 'a
        | Options of Decision<'a> list

        

    type Upgrade =
        | Battery
        | Sensor
        | Shield
        | SabotageDevice

    type AgentRole =
        | Saboteur
        | Explorer
        | Repairer
        | Inspector
        | Sentinel
            
            

    type Agent =
        { 
        Energy      : int; 
        Health      : int; 
        MaxEnergy   : int; 
        MaxHealth   : int; 
        Name        : string; 
        Node        : string; 
        Role        : Option<AgentRole>; 
        Strength    : int; 
        TeamName    : string; 
        VisionRange : int;
        }

            
            
    type Action =
        | Skip
        | Recharge
        | Goto      of Vertex
        | Probe     of Option<Vertex>
        | Survey    
        | Inspect   of Agent
        | Attack    of Agent
        | Parry
        | Repair    of Agent
        | Buy       of Upgrade


    type ActionResult =
        | Successful
        | FailedResources
        | FailedAttacked
        | FailedUnreachable
        | FailedOutOfRange
        | FailedInRange
        | FailedWrongParam
        | FailedRole
        | FailedStatus
        | FailedLimit
        | FailedRandom
        | Failed

    type JobID = int
    type JobValue = int
    type Desirability = int
    type AgentID = string

    type JobType = 
        | OccupyJob = 1
        | RepairJob = 2
        | DisruptJob = 3
        | AttackJob = 4

    type JobData =
        | OccupyJob of Vertex list
        | RepairJob of Vertex * AgentID
        | DisruptJob of Vertex
        | AttackJob of Vertex list

    type JobHeader = JobID * JobValue * JobType

    type Job = JobHeader * JobData

            
    type Achievement =
        | ConqueredZone10
        | ConqueredZone20
        | ConqueredZone40
        | ConqueredZone80
        | ConqueredZone160
        | ConqueredZone320
        | ConqueredZone640
        | ConqueredZone1280
        | ProbedVerices5
        | ProbedVerices10
        | ProbedVerices20
        | ProbedVerices40
        | ProbedVerices80
        | ProbedVerices160
        | ProbedVerices320
        | ProbedVerices640
        | SurveyedEdges10
        | SurveyedEdges20
        | SurveyedEdges40
        | SurveyedEdges80
        | SurveyedEdges160
        | SurveyedEdges320
        | SurveyedEdges640
        | SurveyedEdges1280
        | InspectedVehicles5
        | InspectedVehicles10
        | InspectedVehicles20
        | InspectedVehicles40
        | InspectedVehicles80
        | InspectedVehicles160
        | InspectedVehicles320
        | InspectedVehicles640
        | Attacked5
        | Attacked10
        | Attacked20
        | Attacked40
        | Attacked80
        | Attacked160
        | Attacked320
        | Attacked640
        | Parried5
        | Parried10
        | Parried20
        | Parried40
        | Parried80
        | Parried160
        | Parried320
        | Parried640

    type Team =
        {   
            LastStepScore   : int
            Money           : int
            Score           : int
            ZoneScores      : int
            Achievements    : Achievement list
        }

    type Percept =
        | EnemySeen         of Agent
        | VertexSeen        of Vertex
        | EdgeSeen          of Edge
        | SimulationStep    of int
        | Self              of Agent
        | TeamStats         of Team
        | LastAction        of Action*ActionResult

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
            World           : Graph; 
            Self            : Agent;
            LastAction      : Action*ActionResult 
            EnemyData       : Agent list; 
            SimulationStep  : int;
            NearbyAgents    : Agent list;
            TeamData        : Team 
        }

    type OptionFunc = State -> (bool*Option<Action>)

    type DecisionRank = int
