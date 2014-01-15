namespace NabfAgentLogic

module AgentTypes =

    open Graph

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
                Position    : string
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
                | VertexSeen     of Graph.Vertex
                | EdgeSeen       of Graph.Edge
                | Achievement    of string
                | SimulationStep of int

            type ServerData = 
                | PerceptCollection of Percept list
                | NewJobs of Job list
                | AcceptedJob of JobID
                | ActionRequest
                | SimulationEnd

            type State =
                { 
                    World          : Graph; 
                    Self           : Agent; 
                    EnemyData      : Agent list; 
                    Achievements   : Set<string>; 
                    SimulationStep : int;
                    NearbyAgents   : Agent list
                }