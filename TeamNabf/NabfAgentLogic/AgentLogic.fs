module AgentLogic

open System
open Graph

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
        Type        : AgentType;
        Energy      : int;
        Health      : int;
        Strength    : int;
        VisionRange : int;
    }

type Percept =
    | EnemySeen of Agent
    | NodeSeen of Graph.Vertex

type State =
    {
        World : Graph;
        Self : Agent;
        Enemies : Agent list;
    }

(* handlePercept State -> Percept -> State *)
let handlePercept state percept =
    match percept with
        | EnemySeen enemy -> { state with Enemies = enemy :: state.Enemies }
//        | NodeSeen node -> { state with World = 
        | _ -> state

(* let updateState : State -> Percept list -> State *)
let updateState : State -> Percept list -> State = 
    List.fold handlePercept

(* chooseAction : State -> Percept list -> Action *)
let chooseAction currentState percepts =
    let newState = updateState currentState percepts
    0
