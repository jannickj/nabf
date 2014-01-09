module Agent

open System

type Percept = int
type Action = int

type AgentType =
    | Saboteur
    | Explorer
    | Repairer
    | Inspector
    | Sentinel

type Agent =
    {
        Id          : string;
        EnemyType   : AgentType;
        Energy      : int;
        Health      : int;
        Strength    : int;
        VisionRange : int;
    }

type State =
    {
        World : int;
        Self : Agent;
        Enemies : Agent list;
    }

(* handlePercept State -> Percept -> State *)
let handlePercept state percept =
    {World = 1;Self = {Id="";EnemyType=Saboteur;Energy=1;Health=1;Strength=1;VisionRange=1};Enemies = []} : State

(* let updateState : State -> Percept list -> State *)
let updateState : State -> Percept list -> State = 
    List.fold handlePercept

(* chooseAction : State -> Percept list -> Action *)
let chooseAction currentState percepts =
    let newState = updateState currentState percepts
    0
