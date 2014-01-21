namespace NabfAgentLogic.IiLang
    module IilTranslator = 
        open Graphing.Graph
        open IiLangDefinitions
        open NabfAgentLogic.AgentTypes

        exception InvalidIilException of string * (Element list)

        let deriveStatusFromHealth health =
            if health = 0 then 
                Disabled 
            else 
                Normal



        let parseIilRole iilRole = 
            match iilRole with
            | Identifier "Saboteur"  -> Some Saboteur
            | Identifier "Explorer"  -> Some Explorer
            | Identifier "Repairer"  -> Some Repairer
            | Identifier "Inspector" -> Some Inspector
            | Identifier "Sentinel"  -> Some Sentinel
            | Identifier ""          -> None
            | _ -> raise <| InvalidIilException ("Role", [iilRole])

        let parseIilAgentRole iilAgent =
            match iilAgent with
            | [ Function ("role", [role])
              ; Function ("agentId", [Identifier id])
              ; Function ("sureness", [Numeral sureness])
              ] -> AgentRolePercept (id, (parseIilRole role).Value, int <| sureness)
            | _ -> raise <| InvalidIilException ("AgentRole", iilAgent)
        
        let parseIilAgent iilData =
            match iilData with
            | Function ("inspectedEntity",  
                        [ Function ("energy", [Numeral energy])
                        ; Function ("health", [Numeral health])
                        ; Function ("maxEnergy", [Numeral maxEnergy])
                        ; Function ("maxHealth", [Numeral maxHealth])
                        ; Function ("name", [Identifier name])
                        ; Function ("node", [Identifier node])
                        ; Function ("role", [role])
                        ; Function ("strength", [Numeral strength])
                        ; Function ("team", [Identifier team])
                        ; Function ("visRange", [Numeral visionRange])
                        ])
                -> { Energy      = Some (int energy)
                   ; Health      = Some (int health)
                   ; MaxEnergy   = Some (int maxEnergy)
                   ; MaxHealth   = Some (int maxHealth)    
                   ; Name        = name
                   ; Node        = node
                   ; Role        = parseIilRole role
                   ; Strength    = Some (int strength)
                   ; Team        = team
                   ; VisionRange = Some (int visionRange)
                   ; Status      = deriveStatusFromHealth <| int health
                   }
            | _ -> raise <| InvalidIilException ("Agent", [iilData])

        let parseIilActionRequest iilData =
            match iilData with
            | [ Function ("id", [Numeral id])
              ; Function ("deadline", [Numeral deadline])
              ; Function ("timestamp", [Numeral timestamp])
              ] -> (uint32 deadline, uint32 timestamp, int id) : ActionRequestData
            | _ -> raise <| InvalidIilException ("ActionRequest", iilData)

        let parseIilProbedVertex iilData =
            match iilData with
            | Function ("probedVertex", [ Function ("name", [Identifier name])
                                        ; Function ("value", [Numeral value])
                                        ])

                -> (name, int value)

            | _ -> raise <| InvalidIilException ("probedVertex", [iilData])

        let stringToOption str = 
            if str = "" then 
                None 
            else 
                Some str

        let parseIilUpgrade iilUpgrade =
            match iilUpgrade with
            | Identifier "battery" -> Battery
            | Identifier "sensor" -> Sensor
            | Identifier "shield" -> Shield
            | Identifier "sabotageDevice" -> SabotageDevice
            | _ -> raise <| InvalidIilException ("upgrade", [iilUpgrade])

        let parseIilAction iilAction iilActionParam =
            match (iilAction, iilActionParam) with
            | (Identifier "noAction", _)                   
            | (Identifier "skip", _)                       -> Skip
            | (Identifier "recharge", _)                   -> Recharge
            | (Identifier "goto", Identifier vertexName)   -> Goto vertexName
            | (Identifier "probe", Identifier vertexName)  -> Probe <| stringToOption vertexName
            | (Identifier "survey", _)                     -> Survey
            | (Identifier "inspect", Identifier agentName) -> Inspect <| stringToOption agentName
            | (Identifier "repair", Identifier agentName)  -> Repair agentName
            | (Identifier "buy", upgrade)                  -> Buy <| parseIilUpgrade upgrade
            | _ -> raise <| InvalidIilException ("iilAction", [iilAction; iilActionParam])

        let parseIilActionResult iilActionResult =
            match iilActionResult with
            | Identifier "successful"          -> Successful
            | Identifier "failed"              -> Failed
            | Identifier "failed_resources"    -> FailedResources
            | Identifier "failed_attacked"     -> FailedAttacked
            | Identifier "failed_parried"      -> FailedParried
            | Identifier "failed_unreachable"  -> FailedUnreachable
            | Identifier "failed_out_of_range" -> FailedOutOfRange
            | Identifier "failed_in_range"     -> FailedInRange
            | Identifier "failed_wrong_param"  -> FailedWrongParam 
            | Identifier "failed_role"         -> FailedRole
            | Identifier "failed_status"       -> FailedStatus
            | Identifier "failed_limit"        -> FailedLimit
            | Identifier "failed_random"       -> FailedRandom
            | _ -> raise <| InvalidIilException ("iilActionResult", [iilActionResult])

        let parseIilSelf iilSelf = 
            match iilSelf with
            | [ Function ("energy", [Numeral energy])
              ; Function ("health", [Numeral health])
              ; Function ("lastAction", [action])
              ; Function ("lastActionParam", [actionParam])
              ; Function ("lastActionResult", [actionResult])
              ; Function ("maxEnergy", [Numeral maxEnergy])
              ; Function ("maxEnergyDisabled", [Numeral maxEnergyDisabled])
              ; Function ("maxHealth", [Numeral maxHealth])
              ; Function ("position", [Identifier node])
              ; Function ("strength", [Numeral strength])
              ; Function ("visRange", [Numeral visRange])
              ; Function ("zoneScore", [Numeral zoneScore])
              ] -> [ ZoneScore <| int zoneScore 
                   ; MaxEnergyDisabled <| int maxEnergyDisabled
                   ; LastAction <| parseIilAction action actionParam
                   ; LastActionResult <| parseIilActionResult actionResult
                   ; Self { Energy = Some (int energy)
                          ; Health = Some (int health)
                          ; MaxEnergy = Some (int maxEnergy)
                          ; MaxHealth = Some (int maxHealth)
                          ; Name = ""
                          ; Node = node
                          ; Role = None
                          ; Strength = Some (int strength)
                          ; Team = ""
                          ; VisionRange = Some (int visRange)
                          ; Status = deriveStatusFromHealth <| int health
                          }
                   ]
            | _ -> raise <| InvalidIilException ("iilSelf", iilSelf)

        let parseIilStep iilData =
            match iilData with
            | [Function ("step", [Numeral step])] -> int step
            | _ -> raise <| InvalidIilException ("step", iilData)

        let parseIilSurveyedEdge iilData =
            match iilData with
            | Function ("surveyedEdge",
                [ Function ("node1", [Identifier node1])
                ; Function ("node2", [Identifier node2])
                ; Function ("weight", [Numeral weight])
                ]) -> (Some (int weight), node1, node2) : Edge
            | _ -> raise <| InvalidIilException ("surveyedEdge", [iilData])

        let parseIilAchievement achievement =
            let (|Name|_|) name (str : string) = 
                if str.StartsWith name then
                    Some (int <| str.Substring name.Length)
                else 
                    None
            match achievement with
            | Identifier str ->
                match str with
                | Name "proved" score    -> ProbedVertices score
                | Name "surveyed" score  -> SurveyedEdges score
                | Name "area" score      -> ConqueredZone score
                | Name "parried" score   -> Parried score
                | Name "attacked" score  -> Attacked score
                | Name "inspected" score -> InspectedVehicles score
            | _ -> raise <| InvalidIilException ("achievement", [achievement])
                    

        let parseIilTeam iilData =
            match iilData with
            | [ Function ("lastStepScore", [Numeral lastStepScore])
              ; Function ("money", [Numeral money])
              ; Function ("score", [Numeral score])
              ; Function ("zoneScore", [Numeral zoneScore])
              ; Function ("achievements", achievements)
              ] -> { TeamState.LastStepScore = int lastStepScore
                   ; Money = int money
                   ; Score = int score
                   ; ZoneScore = int zoneScore
                   ; Achievements = List.map parseIilAchievement achievements
                   }
            | _ -> raise <| InvalidIilException ("team", iilData)

        let parseIilVisibleEdge visibleEdge =
            match visibleEdge with
            | Function ("visibleEdge", 
                [ Function ("node1", [Identifier node1])
                ; Function ("node2", [Identifier node2])
                ]) -> (None, node1, node2) : Edge
            | _ -> raise <| InvalidIilException ("visibleEdge", [visibleEdge])

        let parseIilStatus status =
            match status with
            | Identifier "disabled" -> Disabled
            | Identifier "normal" -> Normal
            | _ -> raise <| InvalidIilException ("status", [status])

        let parseIilVisibleEntity visibleEntity =
            match visibleEntity with
            | Function ("visibleEntity", 
                [ Function ("name", [Identifier name])
                ; Function ("team", [Identifier team])
                ; Function ("node", [Identifier node])
                ; Function ("status", [status])
                ]) -> { Energy = None
                      ; Health = None
                      ; MaxEnergy = None
                      ; MaxHealth = None
                      ; Name = name
                      ; Node = node
                      ; Role = None
                      ; Strength = None
                      ; Team = team
                      ; VisionRange = None
                      ; Status = parseIilStatus status
                      }
            | _ -> raise <| InvalidIilException ("visibleEntity", [visibleEntity])
        
        let parseIilTeamName teamName =
            match teamName with
            | Identifier team when team.Length > 0 -> Some team
            | _ -> None

        let parseIilVisibleVertex visibleVertex =
            match visibleVertex with
            | Function ("visibleVertex", 
                [ Function ("name", [Identifier name])
                ; Function ("team", [team])
                ]) -> (name, parseIilTeamName team)
            | _ -> raise <| InvalidIilException ("visibleVertex", [visibleVertex])

        let parseIilSimStart iilSimStart = 
            match iilSimStart with
            | [ Function ("id", [Numeral id])
              ; Function ("steps", [Numeral steps])
              ; Function ("edges", [Numeral edges])
              ; Function ("vertices", [Numeral vertices])
              ; Function ("role", [role])
              ] -> { SimId = int id
                   ; SimEdges = int steps
                   ; SimVertices = int vertices
                   ; SimRole = (parseIilRole role).Value
                   }
            | _ -> raise <| InvalidIilException ("simStart", iilSimStart)
        
        let parseIilSimEnd iilSimEnd =
            match iilSimEnd with
            | [ Function ("ranking", [Numeral ranking])
              ; Function ("steps", [Numeral steps])
              ] -> (int ranking, int steps)
            | _ -> raise <| InvalidIilException ("simEnd", iilSimEnd)

        let parseIilPercept iilPercept =
            match iilPercept with
            | Percept (name, data) -> 
                match name with
                | "inspectedEntities" -> List.map (parseIilAgent >> Percept.EnemySeen) data
                | "probedVertices"    -> List.map (parseIilProbedVertex >> Percept.VertexProbed) data
                | "self"              -> parseIilSelf data
                | "simulation"        -> [SimulationStep <| parseIilStep data]
                | "surveyedEdges"     -> List.map (parseIilSurveyedEdge >> Percept.EdgeSeen) data
                | "team"              -> [Team <| parseIilTeam data]
                | "visibleEdges"      -> List.map (parseIilVisibleEdge >> Percept.EdgeSeen) data
                | "visibleEntities"   -> List.map (parseIilVisibleEntity >> EnemySeen) data
                | "visibleVertices"   -> List.map (parseIilVisibleVertex >> VertexSeen) data
                | "roleKnowledge"     -> [parseIilAgentRole data]
                | _ -> raise <| InvalidIilException ("iilPercept", data)
            | _ -> failwith "no"    
        
        
                

        let parseIilServerMessage iilServerMessage =
            match iilServerMessage with
            | Percept (name, data) :: tail ->
                match name with
                | "actionRequest" -> 
                    let percepts = List.concat <| List.map parseIilPercept tail
                    MarsServerMessage <| ActionRequest (parseIilActionRequest data, percepts)
                | "simStart" -> 
                    MarsServerMessage <| (SimulationStart <| parseIilSimStart data)
                | "simEnd" ->
                    MarsServerMessage <| (SimulationEnd <| parseIilSimEnd data)
                | "newKnowledge" ->
                    let percepts = List.concat <| List.map parseIilPercept tail
                    AgentServerMessage <| SharedPercepts percepts
                | _ ->  raise <| InvalidIilException ("iilServerMessage", data)
            | _ -> failwith "nonono"
        
        let buildIilAction action id =
            match action with
            | Skip -> Action ("skip", [Numeral id])
            | Goto vn -> Action ("goto", [Numeral id; Identifier vn])
            | Attack a -> Action ("goto", [Numeral id; Identifier a])
            | Recharge -> Action ("recharge", [Numeral id])
            | Buy a -> Action ("buy", [Numeral id; Identifier (a.ToString().ToLower())])
            | Inspect a ->
                    if a.IsNone then
                        Action ("inspect", [Numeral id; Identifier a.Value])
                    else
                        Action ("inspect", [Numeral id])
            | Parry -> Action ("parry", [Numeral id])
            | Probe a ->
                if a.IsNone then
                        Action ("probe", [Numeral id; Identifier a.Value])
                    else
                        Action ("probe", [Numeral id])
            | Repair a -> Action ("repair", [Numeral id; Identifier a])
            | Survey -> Action ("survey", [Numeral id])
