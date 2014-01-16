namespace NabfAgentLogic

module AgentTypes =

    open Graphing.Graph

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
                Role        : AgentRole; 
                Strength    : int; 
                Team        : string; 
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

            type Percept =
                | EnemySeen      of Agent
                | VertexSeen     of Vertex
                | EdgeSeen       of Edge
                | Achievement    of string
                | SimulationStep of int

            type Deadline = int
            type CurrentTime = int
            type Rank = int
            type Score = int
            type ActionID = int
            

            type AgentServerMessage =
                | NewJobs of Job List
                | AcceptedJob of JobID
                | SharedPercepts of Percept list

            type MarsServerMessage =  
                | ActionRequest of  Deadline*CurrentTime*ActionID*(Percept list)
                | SimulationStart
                | SimulationEnd of Rank*Score
                | ServerClosed

            type ServerMessage = 
                | AgentServerMsg of AgentServerMessage
                | MarsServerMsg of MarsServerMessage


            type State =
                { 
                    World          : Graph; 
                    Self           : Agent; 
                    EnemyData      : Agent list; 
                    Achievements   : Set<string>; 
                    SimulationStep : int;
                    NearbyAgents   : Agent list
                }