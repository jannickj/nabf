
namespace NabfAgentLogic.AgentInterfaces
    open JSLibrary.IiLang;
    open JSLibrary.IiLang.DataContainers;
    open JSLibrary.Data.GenericEvents;
    open System;

    type IJob = 
        abstract member JobType : string 
        abstract member ID : int
        abstract member Value : int

    type IAgentLogic = 
        abstract member EvaluateJob : IJob -> int * bool
        abstract member LoadPercepts : IilPerceptCollection -> unit
        abstract member GetJobs : List<IJob>
        abstract member LoadJob : IJob
        abstract member GivenJob : IJob -> unit
        abstract member Start : unit
        abstract member GetDecision : IilAction
        [<CLIEvent>]
        abstract member JobCreated : IEvent<UnaryValueHandler<IJob>, UnaryValueEvent<IJob>>
        [<CLIEvent>]
        abstract member EvaluationCompleted : IEvent<EventHandler, EventArgs>
        
    
