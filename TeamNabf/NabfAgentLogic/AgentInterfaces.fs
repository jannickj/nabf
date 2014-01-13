
namespace NabfAgentLogic.AgentInterfaces
    open JSLibrary.IiLang;
    open JSLibrary.IiLang.DataContainers;
    open JSLibrary.Data.GenericEvents;
    open System;
    open NabfAgentLogic.AgentLogic

    

    type IAgentLogic = 
        abstract member HandlePercepts : IilPerceptCollection -> unit
        abstract member GetJobs : List<JobID*Desirability>
        abstract member CurrentDecision : IilAction
        abstract member Close : unit -> unit
        [<CLIEvent>]
        abstract member JobCreated : IEvent<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>
        [<CLIEvent>]
        abstract member EvaluationCompleted : IEvent<EventHandler, EventArgs>
        [<CLIEvent>]
        abstract member SimulationEnded : IEvent<EventHandler, EventArgs>
        
    
