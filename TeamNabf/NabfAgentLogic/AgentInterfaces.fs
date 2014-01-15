
namespace NabfAgentLogic.AgentInterfaces
    open JSLibrary.IiLang;
    open JSLibrary.IiLang.DataContainers;
    open JSLibrary.Data.GenericEvents;
    open System;
    open NabfAgentLogic.AgentLogic

    

    type IAgentLogic = 
        abstract member HandlePercepts : IilPerceptCollection -> unit
        abstract member CurrentDecision : IilAction
        abstract member Close : unit -> unit
        [<CLIEvent>]
        abstract member SendAgentServerAction : IEvent<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>
        [<CLIEvent>]
        abstract member SendMarsServerAction : IEvent<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>
        [<CLIEvent>]
        abstract member JobCreated : IEvent<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>
        [<CLIEvent>]
        abstract member JobDesired : IEvent<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>
        [<CLIEvent>]
        abstract member EvaluationCompleted : IEvent<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>
        [<CLIEvent>]
        abstract member EvaluationStarted : IEvent<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>
        [<CLIEvent>]
        abstract member ActionRequested : IEvent<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>
        [<CLIEvent>]
        abstract member SimulationEnded : IEvent<EventHandler, EventArgs>
        
    
