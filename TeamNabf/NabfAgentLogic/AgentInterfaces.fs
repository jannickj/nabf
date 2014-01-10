
namespace NabfAgentLogic.AgentInterfaces
    open JSLibrary.IiLang;
    open JSLibrary.IiLang.DataContainers;
    open JSLibrary.Data.GenericEvents;
    open System;

    type JobID = int
    type Desirability = int

    type IAgentLogic = 
        abstract member EvaluateJob : JobID -> IilAction*bool
        abstract member HandlePercepts : IilPerceptCollection -> unit
        abstract member GetJobs : List<JobID*Desirability>
        abstract member Start : unit
        abstract member CurrentDecision : IilAction
        [<CLIEvent>]
        abstract member JobCreated : IEvent<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>
        [<CLIEvent>]
        abstract member JobLoaded : IEvent<UnaryValueHandler<JobID>, UnaryValueEvent<JobID>>
        [<CLIEvent>]
        abstract member PerceptsLoaded : IEvent<EventHandler, EventArgs>
        [<CLIEvent>]
        abstract member EvaluationCompleted : IEvent<EventHandler, EventArgs>
        
    
